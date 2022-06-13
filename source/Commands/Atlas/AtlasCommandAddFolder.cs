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
using System.IO;
using TexturePacker.Config;

namespace TexturePacker.Commands.Atlas
{
  public class AtlasCommandAddFolder : AtlasCommand
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly string FolderPath;
    public readonly AtlasElementConfig ElementConfig;
    public readonly bool KeepDpiInFilename;
    public readonly List<AtlasCommandAddFolderMod> FolderModList;
    public readonly List<AtlasCommandAddFolderFileMod> FileModList;

    public AtlasCommandAddFolder(AtlasElementConfig elementConfig, string folderPath, bool keepDpiInFilename,
                                 List<AtlasCommandAddFolderMod> folderModList, List<AtlasCommandAddFolderFileMod> fileModList)
      : base(AtlasCommandId.AddFolder)
    {
      if (!elementConfig.IsValid)
        throw new ArgumentException("invalid ElementConfig", nameof(elementConfig));

      NameUtil.ValidatePathName(folderPath);
      FolderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
      ElementConfig = elementConfig;
      KeepDpiInFilename = keepDpiInFilename;
      FolderModList = folderModList ?? throw new ArgumentNullException(nameof(folderModList));
      FileModList = fileModList ?? throw new ArgumentNullException(nameof(fileModList));
    }

    public override ResolvedAtlasCommand Resolve(in AtlasCommandResolveInfo info)
    {
      var resolvedPath = info.SrcPathResolver.Combine(info.SourcePath, FolderPath);

      var resolvedImageFiles = ResolveImageFiles(info.SrcPathResolver, resolvedPath, info.ImageExtensions, ElementConfig, KeepDpiInFilename,
                                                 FolderModList, FileModList);
      // Check for license files
      info.CopyManager.ResolveLicenseFiles(resolvedImageFiles, info.DstAtlasPath.AbsolutePath);

      return new ResolvedAtlasCommandAddFolder(resolvedPath, resolvedImageFiles);
    }

    private class FolderModRecord
    {
      public AtlasCommandAddFolderMod Mod;
      public bool Touched;

      public FolderModRecord(AtlasCommandAddFolderMod mod)
      {
        Mod = mod;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static ImmutableArray<ResolvedImageFile> ResolveImageFiles(PathResolver pathResolver, ResolvedPath srcPath,
                                                                       ImmutableHashSet<string> imageExtensions, AtlasElementConfig elementConfig,
                                                                       bool keepDpiInFilename, List<AtlasCommandAddFolderMod> folderModList,
                                                                       List<AtlasCommandAddFolderFileMod> fileModList)
    {
      // Build the entry mod list dictionary and check for duplicates
      var folderModDict = new Dictionary<string, FolderModRecord>(folderModList.Count);
      foreach (var entry in folderModList)
      {
        NameUtil.ValidatePathName(entry.Path);
        if (folderModDict.ContainsKey(entry.Path))
          throw new Exception($"AddFolder Folder Path='{entry.Path}'> must be unique.");
        folderModDict[entry.Path] = new FolderModRecord(entry);
      }

      // Build the entry mod list dictionary and check for duplicates
      var fileModDict = new Dictionary<string, AtlasCommandAddFolderFileMod>(fileModList.Count);
      foreach (var entry in fileModList)
      {
        NameUtil.ValidatePathName(entry.Path);
        if (fileModDict.ContainsKey(entry.Path))
          throw new Exception($"AddFolder File Path='{entry.Path}' mod entry must be unique");
        fileModDict[entry.Path] = entry;
      }

      var dirFiles = Directory.GetFiles(srcPath.AbsolutePath, "*.*", SearchOption.AllDirectories);
      var imageFiles = new List<ResolvedImageFile>(dirFiles.Length);
      foreach (var fileEntry in dirFiles)
      {
        var extension = Path.GetExtension(fileEntry).ToUpperInvariant();
        if (imageExtensions.Contains(extension))
        {
          var filePath = IOUtil.NormalizePath(fileEntry);
          g_logger.Trace("- Found image file '{0}'", filePath);

          var relativeFilePath = filePath.Substring(srcPath.AbsolutePath.Length + 1);
          var resolvedPath = pathResolver.Combine(srcPath, relativeFilePath);

          // Analyze the filename
          var filenameInfo = Input.FileNameUtil.AnalyzeFilename(resolvedPath.AbsolutePath, keepDpiInFilename);

          bool isPatch = filenameInfo.IsPatch;
          UInt16 dpi = (filenameInfo.DpiOverride != 0 ? filenameInfo.DpiOverride : elementConfig.DefaultDpi);

          //var relativeResolvedSourcePath = IOUtil.Combine(IOUtil.GetDirectoryName(resolvedPath.RelativeResolvedSourcePath),
          //                                                Path.GetFileName(filenameInfo.NormalizedName));

          var resolvedAtlasPath = new ResolvedPath(filenameInfo.NormalizedName, resolvedPath.UnresolvedSourcePath, resolvedPath.ParentPath);

          { // Apply folder mods if any
            string relativeEntryName = IOUtil.TryRemoveStartDirectoryName(resolvedAtlasPath.UnresolvedSourcePath, srcPath.UnresolvedSourcePath);
            if (relativeEntryName != null)
            {
              string relativeDirName = IOUtil.GetDirectoryName(relativeEntryName);
              if (relativeDirName.Length > 0 && folderModDict.TryGetValue(relativeDirName, out FolderModRecord folderModRecord))
              {
                // Override element
                elementConfig = folderModRecord.Mod.ElementConfig;
                folderModRecord.Touched = true;
                if (filenameInfo.DpiOverride == 0)
                {
                  dpi = elementConfig.DefaultDpi;
                }
              }
            }
          }

          AddNineSlice addNineSlice = null;
          AddComplexPatch addComplexPatch = null;
          AddAnchor addAnchor = null;
          if (fileModDict.TryGetValue(relativeFilePath, out AtlasCommandAddFolderFileMod entryMod))
          {
            fileModDict.Remove(relativeFilePath);
            // Override element
            elementConfig = entryMod.ElementConfig;
            addNineSlice = entryMod.AddNineSlice;
            addComplexPatch = entryMod.AddComplexPatch;
            addAnchor = entryMod.AddAnchor;
            if (filenameInfo.DpiOverride == 0)
            {
              dpi = elementConfig.DefaultDpi;
            }
          }

          imageFiles.Add(new ResolvedImageFile(elementConfig, resolvedPath, resolvedAtlasPath, isPatch, dpi, addNineSlice, addComplexPatch, addAnchor));
        }
      }
      if (folderModDict.Count > 0)
      {
        var untouchedEntries = new List<string>(folderModDict.Count);
        foreach (var record in folderModDict.Values)
        {
          if (!record.Touched)
            untouchedEntries.Add(record.Mod.Path);
        }
        if (untouchedEntries.Count > 0)
        {
          throw new Exception($"AddFolder contained Folder commands for elements that was not found: {string.Join(',', untouchedEntries)}");
        }
      }
      if (fileModDict.Count > 0)
      {
        throw new Exception($"AddFolder contained File commands for elements that was not found: {string.Join(',', fileModDict.Keys)}");
      }
      return imageFiles.ToImmutableArray();
    }
  }
}
