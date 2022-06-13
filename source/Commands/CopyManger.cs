/****************************************************************************************************************************************************
 * Copyright 2020 NXP
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *    * Redistributions of source code must retain the above copyright notice,
 *      this list of conditions and the following disclaimer.
 *
 *    * Redistributions in binary form must reproduce the above copyright notice,
 *      this list of conditions and the following disclaimer in the documentation
 *      and/or other materials provided with the distribution.
 *
 *    * Neither the name of the NXP. nor the names of
 *      its contributors may be used to endorse or promote products derived from
 *      this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 ****************************************************************************************************************************************************/

using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using TexturePacker.Config;
using TexturePacker.License;

namespace TexturePacker.Commands
{
  public sealed class CopyManager
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    private enum FileoperationType
    {
      Normal,
      License,
      MasterLicense,
      DynamicMasterLicense
    }

    private class CopyRecord
    {
      public readonly FileoperationType Operation;
      public readonly string From;
      public readonly string To;
      public ALicenseInfo CachedLicenceInfo;

      public CopyRecord(FileoperationType operation, string from, string to, ALicenseInfo cachedLicenceInfo = null)
      {
        Debug.Assert(from != null);
        Debug.Assert(to != null);
        Operation = operation;
        From = from;
        To = to;
        CachedLicenceInfo = cachedLicenceInfo;
      }
    }


    private Dictionary<string, CopyRecord> m_dict = new Dictionary<string, CopyRecord>();
    private LicenseConfig m_licenseConfig;
    private bool m_ignoreLicenseFiles;
    private LicenseInfoNxpJsonDecoder m_licenseDecoder = new LicenseInfoNxpJsonDecoder();

    public CopyManager(LicenseConfig licenseConfig, bool ignoreLicenseFiles)
    {
      m_licenseConfig = licenseConfig;
      m_ignoreLicenseFiles = ignoreLicenseFiles;
    }


    public void AddLicenseFileCopy(string from, string to)
    {
      if (from == null)
        throw new ArgumentNullException(nameof(from));
      if (to == null)
        throw new ArgumentNullException(nameof(to));

      if (m_ignoreLicenseFiles)
      {
        g_logger.Trace("License file copy is disabled, ignoring request (from: '{0}' to: '{1}')", from, to);
        return;
      }

      var cachedLicenseInfo = ReadLicenseInfo(from);
      var record = new CopyRecord(FileoperationType.License, from, to, cachedLicenseInfo);
      // We do not allow filenames to only differ by casing
      var toId = to.ToUpperInvariant();
      if (m_dict.TryGetValue(toId, out CopyRecord oldRecord))
      {
        HandleLicenseConflict(from, to, oldRecord);
        return;
      }
      m_dict.Add(toId, record);

      g_logger.Trace("Adding license file copy from '{0}' to '{1}'", from, to);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private void HandleLicenseConflict(string from, string to, CopyRecord oldRecord)
    {
      switch (oldRecord.Operation)
      {
        case FileoperationType.Normal:
          g_logger.Error("Found copy to existing license target file '{0}'");
          throw new Exception($"Copy of {from} to target file {to} conflicts with the copy from {oldRecord.From} of type {oldRecord.Operation}");
        case FileoperationType.License:
        case FileoperationType.DynamicMasterLicense:
          HandleLicensToLicenseConflict(from, to, oldRecord);
          return;
        case FileoperationType.MasterLicense:
          if (!m_licenseConfig.Enabled || !m_licenseConfig.AllowMasterFileCreation)
            g_logger.Trace("Found master license for target file '{0}', request ignored.", to);
          else
            g_logger.Warn("Found master license for target file '{0}', consider removing it as we support license merging.", to);
          return;
        default:
          throw new NotSupportedException($"Unknown operation type {oldRecord.Operation}");
      }
    }

    private void MergeToDyam(string from, string to, CopyRecord oldRecord)
    {
      throw new NotImplementedException();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private void HandleLicensToLicenseConflict(string from, string to, CopyRecord oldRecord)
    {
      if (oldRecord.CachedLicenceInfo == null)
        throw new Exception($"Internal error, missing cached license info ('{from}'->'{to}')");

      // So we are already copying a license file to the target file, so lets compare it
      var newLicenseInfo = ReadLicenseInfo(from);
      if (!oldRecord.CachedLicenceInfo.IsContentConsideredEqual(newLicenseInfo))
      {
        if (m_licenseConfig.AllowMasterFileCreation && oldRecord.CachedLicenceInfo.CanMerge)
        {
          g_logger.Trace("Found license copy to existing license target file '{0}' and the content was different, creating dynamic master license by merge.", to);
          var dynamicMasterLicense = oldRecord.CachedLicenceInfo.Merge(newLicenseInfo);
          var toId = to.ToUpperInvariant();
          m_dict[toId] = new CopyRecord(FileoperationType.DynamicMasterLicense, oldRecord.From, oldRecord.To, dynamicMasterLicense);
          return;
        }
        else
        {
          g_logger.Error("Found license copy to existing license target file '{0}' and the content was different.", to);
          throw new Exception($"Copy of {from} to target file {to} conflicts with the copy from {oldRecord.From} as the content is different. Please manually set a LicenseFile in the command file.");
        }
      }

      // the files are identical, so just ignore the new request
      g_logger.Trace("Found license copy to existing license target file '{0}' and the content was identical, request ignored.", to);
    }

    public void AddMasterLicenseFile(string from, string to)
    {
      if (from == null)
        throw new ArgumentNullException(nameof(from));
      if (to == null)
        throw new ArgumentNullException(nameof(to));

      if (m_ignoreLicenseFiles)
      {
        g_logger.Trace("License file copy is disabled, ignoring request (from: '{0}' to: '{1}')", from, to);
        return;
      }
      var cachedLicenseInfo = ReadLicenseInfo(from);
      var record = new CopyRecord(FileoperationType.MasterLicense, from, to, cachedLicenseInfo);
      // We do not allow filenames to only differ by casing
      var toId = to.ToUpperInvariant();
      m_dict[toId] = record;
      g_logger.Trace("Add master license file copy from '{0}' to '{1}'", from, to);
    }

    public void AddFileCopy(string from, string to)
    {
      if (from == null)
        throw new ArgumentNullException(nameof(from));
      if (to == null)
        throw new ArgumentNullException(nameof(to));

      var record = new CopyRecord(FileoperationType.Normal, from, to);
      // We do not allow filenames to only differ by casing
      var toId = to.ToUpperInvariant();
      if (m_dict.TryGetValue(toId, out CopyRecord oldRecord))
      {
        throw new Exception($"Copy of {from} to target file {to} conflicts with the copy from {oldRecord.From} of type {oldRecord.Operation}");
      }
      m_dict.Add(toId, record);

      g_logger.Trace("Adding file copy from '{0}' to '{1}'", from, to);
    }

    public void ClearCommands()
    {
      m_dict.Clear();
    }

    public ResolvedCommandCopyFiles TryBuildCopyCommand()
    {
      var dynamicLicenses = new List<ResolvedCommandCopyFiles.DynamicMasterLicenseRecord>(m_dict.Count);

      var filesToCopy = new List<ResolvedCommandCopyFiles.CopyFileRecord>(m_dict.Count);
      var dstDirectories = new HashSet<string>();
      foreach (var record in m_dict.Values)
      {
        if (record.Operation != FileoperationType.DynamicMasterLicense)
        {
          filesToCopy.Add(new ResolvedCommandCopyFiles.CopyFileRecord(record.From, record.To));
        }
        else
        {
          dynamicLicenses.Add(new ResolvedCommandCopyFiles.DynamicMasterLicenseRecord(record.From, record.To, record.CachedLicenceInfo));
        }
        var dstDirectory = IOUtil.GetDirectoryName(record.To);
        dstDirectories.Add(dstDirectory);
      }
      foreach (var entry in dstDirectories)
      {
        IOUtil.CreateDirectoryIfMissing(entry);
      }
      return new ResolvedCommandCopyFiles(ImmutableArray.Create(filesToCopy.ToArray()), ImmutableArray.Create(dynamicLicenses.ToArray()));
    }

    public void ResolveFolderLicenseFiles(string sourcePath, string dstPath)
    {
      if (m_licenseConfig.Filename == null)
        return;

      ResolveLicenseFiles(sourcePath, dstPath);

      var allDirectories = Directory.GetDirectories(sourcePath, "*.*", SearchOption.AllDirectories);
      foreach (var directoryName in allDirectories)
      {
        ResolveLicenseFiles(directoryName, dstPath);
      }
    }

    public void ResolveLicenseFiles(ImmutableArray<ResolvedImageFile> resolvedImageFiles, string dstPath)
    {
      var uniqueDirs = new HashSet<string>(resolvedImageFiles.Length);
      foreach (var entry in resolvedImageFiles)
      {
        uniqueDirs.Add(IOUtil.GetDirectoryName(entry.Path.AbsolutePath));
      }

      foreach (var entry in uniqueDirs)
      {
        ResolveLicenseFiles(entry, dstPath);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public void ResolveLicenseFiles(string sourcePath, string dstPath)
    {
      if (m_licenseConfig.Filename == null)
        return;

      var srcFilename = IOUtil.Combine(sourcePath, m_licenseConfig.Filename);
      if (File.Exists(srcFilename))
      {
        g_logger.Trace("Found license file '{0}'", srcFilename);
        var dstFilename = IOUtil.Combine(dstPath, m_licenseConfig.Filename);
        AddLicenseFileCopy(srcFilename, dstFilename);
      }
      else if (m_licenseConfig.RequiredForAllContent)
      {
        throw new Exception($"Required license file not found at '{srcFilename}");
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private ALicenseInfo ReadLicenseInfo(string from)
    {
      g_logger.Trace("Caching license file at '{0}'", from);
      switch (m_licenseConfig.LicenseFormat)
      {
        case LicenseFormat.NxpJson:
          return m_licenseDecoder.DecodeFile(from);
        case LicenseFormat.Unknown:
          return CacheUnknownLicense(from);
        default:
          throw new NotSupportedException($"Unsupported license format {m_licenseConfig.LicenseFormat}");
      }
    }

    private static ALicenseInfo CacheUnknownLicense(string from)
    {
      return new UnknownLicenseInfo(File.ReadAllBytes(from));
    }
  }
}
