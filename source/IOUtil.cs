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
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;

namespace TexturePacker
{
  static class IOUtil
  {
    // We use this instead of a bool to ensure that no one supplies a bool true by accident
    public enum OverWritePolicy
    {
      NotAllowed = 0,
      Allowed = 1
    }

    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public static string NormalizePath(string path)
    {
      return path.Replace('\\', '/');
    }

    public static bool PathStartsWithDir(string path, string dir)
    {
      Debug.Assert(!path.Contains('\\', StringComparison.Ordinal));
      Debug.Assert(!dir.Contains('\\', StringComparison.Ordinal));
      Debug.Assert(!dir.EndsWith('/'));
      return path.StartsWith($"{dir}/", StringComparison.Ordinal) || path == dir;
    }

    public static string ToAbsoluteSanitizedFilename(string basePath, string filename)
    {
      if (!Path.IsPathRooted(basePath))
        throw new NotSupportedException($"basePath must be be rooted '{basePath}'");
      if (Path.IsPathRooted(filename))
        throw new NotSupportedException($"path can not be rooted '{filename}'");
      var result = Path.Combine(basePath, filename);
      SanitizeFilename(ref result);
      return NormalizePath(result);
    }

    internal static void CreateDirectoryIfMissing(string dirName)
    {
      if (!Directory.Exists(dirName))
      {
        Directory.CreateDirectory(dirName);
      }
    }

    public static void CreateFileDirectoryIfMissing(string filename)
    {
      var dstDirName = Path.GetDirectoryName(filename);
      if (dstDirName.Length > 0)
      {
        CreateDirectoryIfMissing(dstDirName);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public static void SaveFileIfModified(string dstFilename, string content, OverWritePolicy overWritePolicy)
    {
      if (dstFilename == null)
        throw new ArgumentNullException(nameof(dstFilename));
      if (content == null)
        throw new ArgumentNullException(nameof(content));

      var existingContent = TryReadAllText(dstFilename);
      if (existingContent != content)
      {
        if (existingContent != null && overWritePolicy == OverWritePolicy.NotAllowed)
          throw new Exception($"File exists '{dstFilename}'");

        // free the existingContent as we dont need it anymore
        existingContent = null;
        File.WriteAllText(dstFilename, content);
        g_logger.Trace("Saved '{0}'", dstFilename);
      }
      else
      {
        g_logger.Trace("Save '{0}' was skipped as existing file was identical", dstFilename);
      }
    }

    public static string TryRemoveStartDirectoryName(string path, string dirNameToRemove)
    {
      if (path == dirNameToRemove)
        return string.Empty;
      if (!IOUtil.PathStartsWithDir(path, dirNameToRemove))
        return null;
      return path.Substring(dirNameToRemove.Length + 1);
    }

    public static string RemoveStartDirectoryName(string path, string dirNameToRemove)
    {
      var res = TryRemoveStartDirectoryName(path, dirNameToRemove);
      if (res != null)
        return res;
      throw new Exception($"Path '{path}' does not start with the directory name '{dirNameToRemove}'");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public static void SaveFileIfModified(string dstFilename, byte[] content, OverWritePolicy overWritePolicy)
    {
      if (dstFilename == null)
        throw new ArgumentNullException(nameof(dstFilename));
      if (content == null)
        throw new ArgumentNullException(nameof(content));

      var existingContent = TryReadAllBytes(dstFilename);
      if (!IsContentEqual(existingContent, content))
      {
        if (existingContent != null && overWritePolicy == OverWritePolicy.NotAllowed)
          throw new Exception($"File exists '{dstFilename}'");

        // free the existingContent as we dont need it anymore
        existingContent = null;
        File.WriteAllBytes(dstFilename, content);
        g_logger.Trace("Saved '{0}'", dstFilename);
      }
      else
      {
        g_logger.Trace("Save '{0}' was skipped as existing file was identical", dstFilename);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All non parameter errors should cause null to be returned")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public static string TryReadAllText(string filename)
    {
      if (filename == null)
        throw new ArgumentNullException(nameof(filename));
      try
      {
        // We do early check here to prevent most exception (the read might still fail with FileNotFound)
        if (File.Exists(filename))
        {
          return File.ReadAllText(filename);
        }
      }
      catch (FileNotFoundException)
      {
        // this is expected, so ignore it
        g_logger.Trace("File not found '{0}'", filename);
      }
      catch (Exception ex)
      {
        g_logger.Warn(ex, "A exception occurred");
      }
      return null;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All non parameter errors should cause null to be returned")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public static byte[] TryReadAllBytes(string filename)
    {
      if (filename == null)
        throw new ArgumentNullException(nameof(filename));
      try
      {
        // We do early check here to prevent most exception (the read might still fail with FileNotFound)
        if (File.Exists(filename))
        {
          return File.ReadAllBytes(filename);
        }
      }
      catch (FileNotFoundException)
      {
        // this is expected, so ignore it
        g_logger.Trace("File not found '{0}'", filename);
      }
      catch (Exception ex)
      {
        g_logger.Warn(ex, "A exception occurred");
      }
      return null;
    }

    public static string Combine(string path0, string path1)
    {
      return NormalizePath(Path.Combine(path0, path1));
    }


    public static string GetDirectoryName(string path)
    {
      return NormalizePath(Path.GetDirectoryName(path));
    }

    private static string SanitizeFilename(ref string rFilename)
    {
      return Path.GetFullPath(rFilename);
    }


    public static bool IsContentEqual(ImmutableArray<byte> lhs, ImmutableArray<byte> rhs)
    {
      if (lhs.Length != rhs.Length)
        return false;
      for (int i = 0; i < lhs.Length; ++i)
      {
        if (lhs[i] != rhs[i])
          return false;
      }
      return true;
    }

    public static bool IsContentEqual(byte[] lhs, byte[] rhs)
    {
      if (lhs == null)
      {
        return lhs == rhs;
      }
      if (rhs == null)
      {
        return false;
      }
      if (lhs.Length != rhs.Length)
        return false;
      for (int i = 0; i < lhs.Length; ++i)
      {
        if (lhs[i] != rhs[i])
          return false;
      }
      return true;
    }

    public static void CopyIfDifferent(string from, string to, OverWritePolicy overWritePolicy)
    {
      if (from == to)
        return;

      var dstContent = TryReadAllBytes(to);
      if (dstContent == null)
      {
        // Dst does not exist, so just copy
        File.Copy(from, to, overWritePolicy == OverWritePolicy.Allowed);
        g_logger.Trace("Copied '{0}' to '{1}'", from, to);
        return;
      }
      var fromContent = File.ReadAllBytes(from);
      if (IsContentEqual(fromContent, dstContent))
      {
        g_logger.Trace("Copy '{0}' to '{1}' ignored as content is identical", from, to);
        return;
      }
      if (overWritePolicy == OverWritePolicy.NotAllowed)
        throw new Exception($"File exists '{to}'");
      g_logger.Trace("Copied '{0}' to '{1}'", from, to);
      File.WriteAllBytes(to, fromContent);
    }
  }
}
