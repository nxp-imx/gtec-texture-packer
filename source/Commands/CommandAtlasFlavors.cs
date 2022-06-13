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
using TexturePacker.Commands.Atlas;
using TexturePacker.Input;

namespace TexturePacker.Commands
{
  public class CommandAtlasFlavors : ActionCommandGroup
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    [DoNotUseDefaultConstruction()]
    private readonly struct FontValidationInfo : IEquatable<FontValidationInfo>
    {
      public readonly string Name;
      public readonly ImmutableHashSet<OutputFontFormat> OutputFontFormats;

      public FontValidationInfo(string name, ImmutableHashSet<OutputFontFormat> outputFontFormats)
      {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OutputFontFormats = outputFontFormats;
      }

      public static string DiffString(FontValidationInfo lhs, FontValidationInfo rhs)
      {
        string diff = string.Empty;
        if (lhs.Name != rhs.Name)
          diff += $"'{lhs.Name}'!='{rhs.Name}'";
        if (!lhs.OutputFontFormats.SetEquals(rhs.OutputFontFormats))
        {
          diff += $"{(diff.Length > 1 ? " " : "")}{ToNamesString(lhs.OutputFontFormats)}!={ToNamesString(rhs.OutputFontFormats)}";
        }
        return diff;
      }

      private static string ToNamesString(ImmutableHashSet<OutputFontFormat> formats)
      {
        var sortedArray = new string[formats.Count];
        int index = 0;
        foreach (OutputFontFormat entry in formats)
        {
          sortedArray[index] = entry.ToString();
          ++index;
        }
        Array.Sort(sortedArray);
        return string.Join(',', sortedArray);
      }

      public static bool operator ==(FontValidationInfo lhs, FontValidationInfo rhs) => lhs.Name == rhs.Name && lhs.OutputFontFormats.SetEquals(rhs.OutputFontFormats);

      public static bool operator !=(FontValidationInfo lhs, FontValidationInfo rhs) => !(lhs == rhs);

      public override bool Equals(object obj)
      {
        return !(obj is FontValidationInfo) ? false : (this == (FontValidationInfo)obj);
      }


      public override int GetHashCode() => (Name != null ? Name.GetHashCode(StringComparison.Ordinal) : 0) ^ OutputFontFormats.GetHashCode();


      public bool Equals(FontValidationInfo other) => this == other;
    }

    [DoNotUseDefaultConstruction]
    private readonly struct AtlasValidationInfo
    {
      public readonly List<ImageFileInfo> ImageFiles;
      public readonly List<string> GeneratedFiles;
      public readonly List<FontValidationInfo> Fonts;

      public AtlasValidationInfo(List<ImageFileInfo> imageFiles, List<string> generatedFiles, List<FontValidationInfo> fonts)
      {
        ImageFiles = imageFiles ?? throw new ArgumentNullException(nameof(imageFiles));
        GeneratedFiles = generatedFiles ?? throw new ArgumentNullException(nameof(generatedFiles));
        Fonts = fonts ?? throw new ArgumentNullException(nameof(fonts));
      }
    }


    [DoNotUseDefaultConstruction]
    private struct ImageFileInfo
    {
      public string Name;
      public ResolvedImageMetaData MetaData;

      public ImageFileInfo(string name, ResolvedImageMetaData metaData)
      {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MetaData = metaData;
      }
    }


    private class FontRecord
    {
      public readonly FontValidationInfo FontInfo;
      public List<ResolvedCommandCreateAtlas> Source;

      public FontRecord(FontValidationInfo fontInfo, ResolvedCommandCreateAtlas command)
      {
        FontInfo = fontInfo;
        Source = new List<ResolvedCommandCreateAtlas>();
        Source.Add(command);
      }
    }

    private class ImageFileRecord
    {
      public readonly ImageFileInfo FileInfo;
      public List<ResolvedCommandCreateAtlas> Source;

      public ImageFileRecord(ImageFileInfo fileInfo, ResolvedCommandCreateAtlas command)
      {
        FileInfo = fileInfo;
        Source = new List<ResolvedCommandCreateAtlas>();
        Source.Add(command);
      }
    }

    private class GeneratedFileRecord
    {
      public readonly string Filename;
      public List<ResolvedCommandCreateAtlas> Source;

      public GeneratedFileRecord(string filename, ResolvedCommandCreateAtlas command)
      {
        Filename = filename ?? throw new ArgumentNullException(nameof(filename));
        Source = new List<ResolvedCommandCreateAtlas>();
        Source.Add(command);
      }
    }


    [DoNotUseDefaultConstruction]
    private readonly struct ValidationInfo
    {
      public readonly Dictionary<string, ImageFileRecord> AllImageDict;
      public readonly Dictionary<string, GeneratedFileRecord> AllGeneratedDict;
      public readonly Dictionary<string, FontRecord> AllFontsDict;

      public ValidationInfo(Dictionary<string, ImageFileRecord> allImageDict, Dictionary<string, GeneratedFileRecord> allGeneratedDict,
                            Dictionary<string, FontRecord> allFontsDict)
      {
        AllImageDict = allImageDict ?? throw new ArgumentNullException(nameof(allImageDict));
        AllGeneratedDict = allGeneratedDict ?? throw new ArgumentNullException(nameof(allGeneratedDict));
        AllFontsDict = allFontsDict ?? throw new ArgumentNullException(nameof(allFontsDict));
      }
    }


    private readonly string m_atlasName;
    private readonly List<CommandCreateAtlas> m_subCommands;

    public CommandAtlasFlavors(string atlasName, List<CommandCreateAtlas> subCommands)
      : base(CommandId.AtlasFlavors)
    {
      m_atlasName = atlasName ?? throw new ArgumentNullException(nameof(subCommands));
      m_subCommands = subCommands ?? throw new ArgumentNullException(nameof(subCommands));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public override List<ResolvedCommand> Resolve(in CommandResolveInfo info)
    {
      g_logger.Trace("AtlasFlavor Name='{0}'", m_atlasName);
      // NamePattern="{AtlasFlavor.Name}_{dpi}dpi"

      string atlasName = $"{m_atlasName}_";

      var flavors = new ResolvedCommandCreateAtlasFlavor[m_subCommands.Count];
      { // Resolve the create atlas commands and do basic validation
        for (int i = 0; i < m_subCommands.Count; ++i)
        {
          var entry = m_subCommands[i];
          g_logger.Trace("Resolving CreateAtlas Name='{0}' as flavor of {1}", m_atlasName, entry.Name);

          { // Validate that all flavor CreateAtlas names follow the correct pattern
            if (!entry.Name.StartsWith(atlasName, StringComparison.Ordinal))
            {
              throw new Exception($"AtlasFlavors name='{m_atlasName}' CreateAtlas '{entry.Name}' is invalid as it did not start with '{atlasName}'. A CreateAtlas flavor must be named like this: '<AtlasFlavor.Name>_<dpi>dpi'");
            }
            var nameSpan = entry.Name.AsSpan(atlasName.Length - 1);
            if (nameSpan.Slice(1).IndexOf('_') > 0)
              throw new Exception($"AtlasFlavors name='{m_atlasName}' CreateAtlas '{entry.Name}' is invalid. A CreateAtlas flavor must be named like this: '<AtlasFlavor.Name>_<dpi>dpi'");
            UInt32 dpi = FileNameUtil.TryDecodeNameDpi(nameSpan);
            if (dpi == 0)
              throw new Exception($"AtlasFlavors name='{m_atlasName}' CreateAtlas '{entry.Name}' is invalid. A CreateAtlas flavor must be named like this: '<AtlasFlavor.Name>_<dpi>dpi'");
          }

          flavors[i] = new ResolvedCommandCreateAtlasFlavor((ResolvedCommandCreateAtlas)entry.Resolve(info));
        }
      }
      ValidateFlavors(flavors, m_atlasName);
      ValidateAtlasSpecificMetaData(flavors, m_atlasName);

      // Build a immutable array of all the flavors
      var immutableResult = ImmutableArray.Create(flavors);

      // Add all the commands to a subb command list that we can be processed
      var subCommandList = new List<ResolvedCommand>(flavors.Length + 1);
      subCommandList.Add(new ResolvedCommandAtlasFlavors(m_atlasName, immutableResult));
      subCommandList.AddRange(immutableResult);
      return subCommandList;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void ValidateAtlasSpecificMetaData(ResolvedCommandCreateAtlasFlavor[] flavors, string debugAtlasName)
    {
      if (flavors.Length <= 0)
        return;

      var expectedOutputFormat = flavors[0].CreateAtlasCommand.OutputFormat;
      var expectedTransparencyMode = flavors[0].CreateAtlasCommand.Config.TransparencyMode;
      var expectedLayoutConfig = flavors[0].CreateAtlasCommand.Config.Layout;
      var expectedTextureConfig = flavors[0].CreateAtlasCommand.Config.Texture;
      var exceptionList = new List<Exception>();
      foreach (var flavor in flavors)
      {
        if (flavor.CreateAtlasCommand.OutputFormat != expectedOutputFormat)
        {
          exceptionList.Add(new Exception($"Flavors in {debugAtlasName} do not use the same atlas format. First flavor used '{expectedOutputFormat}'. The flavor '{flavor.CreateAtlasCommand.Name}' uses '{flavor.CreateAtlasCommand.OutputFormat}'"));
        }
        if (flavor.CreateAtlasCommand.Config.TransparencyMode != expectedTransparencyMode)
        {
          exceptionList.Add(new Exception($"Flavors in {debugAtlasName} do not use the same transparency mode. First flavor used '{expectedTransparencyMode}'. The flavor '{flavor.CreateAtlasCommand.Name}' uses '{flavor.CreateAtlasCommand.Config.TransparencyMode}'"));
        }
        if (flavor.CreateAtlasCommand.Config.Layout != expectedLayoutConfig)
        {
          exceptionList.Add(new Exception($"Flavors in {debugAtlasName} do not use the same layout configuration. First flavor used '{expectedLayoutConfig}'. The flavor '{flavor.CreateAtlasCommand.Name}' uses '{flavor.CreateAtlasCommand.Config.Layout}'"));
        }
        if (flavor.CreateAtlasCommand.Config.Texture != expectedTextureConfig)
        {
          exceptionList.Add(new Exception($"Flavors in {debugAtlasName} do not use the same texture configuration. First flavor used '{expectedTextureConfig}'. The flavor '{flavor.CreateAtlasCommand.Name}' uses '{flavor.CreateAtlasCommand.Config.Texture}'"));
        }
      }
      if (exceptionList.Count > 0)
      {
        throw new AggregateException(exceptionList);
      }
      g_logger.Trace("AtlasFlavors '{0}' all use the atlas format '{1}', transparency mode '{2}', layout config '{3}', texture config '{4}", debugAtlasName, expectedOutputFormat, expectedTransparencyMode, expectedLayoutConfig, expectedTextureConfig);
    }

    private static void ValidateFlavors(ResolvedCommandCreateAtlasFlavor[] flavors, string debugAtlasName)
    {
      // First pass extract a complete image and generated file dictionary
      ValidationInfo validationInfo = BuildValidationInfo(flavors);

      // Second pass
      // - validate that all images exist in all flavors!
      // - validate that all generated files exist in all flavors!
      // - validate that all fonts contain the same information and generates the same outputs
      ValidateFlavorImages(validationInfo.AllImageDict, flavors, debugAtlasName);
      ValidateFlavorGeneratedFiles(validationInfo.AllGeneratedDict, flavors, debugAtlasName);
      ValidateFlavorFonts(validationInfo.AllFontsDict, flavors, debugAtlasName);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static ValidationInfo BuildValidationInfo(ResolvedCommandCreateAtlasFlavor[] flavors)
    {
      // List<FontValidationInfo> Fonts

      var allImageDict = new Dictionary<string, ImageFileRecord>();
      var allGeneratedDict = new Dictionary<string, GeneratedFileRecord>();
      var allFontsDict = new Dictionary<string, FontRecord>();
      foreach (ResolvedCommandCreateAtlasFlavor source in flavors)
      {
        g_logger.Trace("Examining flavor defined by CreateAtlas Name='{0}'", source.CreateAtlasCommand.Name);

        var sourceCommand = source.CreateAtlasCommand;
        var validationInfo = ExtractValidationInfo(source);
        AddImageFilesToDict(allImageDict, sourceCommand, validationInfo.ImageFiles);
        AddGeneratedFilesToDict(allGeneratedDict, sourceCommand, validationInfo.GeneratedFiles);
        AddFontsToDict(allFontsDict, sourceCommand, validationInfo.Fonts);
      }

      return new ValidationInfo(allImageDict, allGeneratedDict, allFontsDict);
    }

    private static void AddFontsToDict(Dictionary<string, FontRecord> allFontsDict, ResolvedCommandCreateAtlas sourceCommand, List<FontValidationInfo> fonts)
    {
      foreach (FontValidationInfo fontInfo in fonts)
      {
        if (!allFontsDict.TryGetValue(fontInfo.Name, out var record))
        {
          record = new FontRecord(fontInfo, sourceCommand);
          allFontsDict[fontInfo.Name] = record;
        }
        else
        {
          if (record.FontInfo != fontInfo)
            throw new Exception($"'{fontInfo.Name}' from CreateAtlas '{sourceCommand.Name}' does not contain the same font information '{FontValidationInfo.DiffString(fontInfo, record.FontInfo)}' as defined by the other flavor atlases '{GetAtlasNameList(record.Source)}'");
          record.Source.Add(sourceCommand);
        }
      }
    }

    private static void AddImageFilesToDict(Dictionary<string, ImageFileRecord> allImageDict, ResolvedCommandCreateAtlas sourceCommand, List<ImageFileInfo> imageFiles)
    {
      foreach (ImageFileInfo fileInfo in imageFiles)
      {
        var fileId = fileInfo.Name.ToUpperInvariant();
        if (!allImageDict.TryGetValue(fileId, out ImageFileRecord record))
        {
          record = new ImageFileRecord(fileInfo, sourceCommand);
          allImageDict.Add(fileId, record);
        }
        else
        {
          // Filenames must match exactly (this detects file names with different casings)
          if (record.FileInfo.Name != fileInfo.Name)
            throw new Exception($"'{fileInfo.Name}' from CreateAtlas '{sourceCommand.Name}' clashes with '{record.FileInfo.Name}' defined by the atlases '{GetAtlasNameList(record.Source)}' due to different casing. Flavor atlas image files must use the exact same casing");

          // Verify that all meta data for the element match
          if (record.FileInfo.MetaData != fileInfo.MetaData)
          {
            throw new Exception($"'{fileInfo.Name}' from CreateAtlas '{sourceCommand.Name}' does not contain the same meta data '{record.FileInfo.MetaData}' != '{fileInfo.MetaData}' as defined by the other flavor atlases '{GetAtlasNameList(record.Source)}'");
          }

          record.Source.Add(sourceCommand);
        }
      }
    }

    private static void AddGeneratedFilesToDict(Dictionary<string, GeneratedFileRecord> allGeneratedDict, ResolvedCommandCreateAtlas sourceCommand, List<string> generatedFiles)
    {
      foreach (string filename in generatedFiles)
      {
        var fileId = filename.ToUpperInvariant();
        if (!allGeneratedDict.TryGetValue(fileId, out GeneratedFileRecord record))
        {
          record = new GeneratedFileRecord(filename, sourceCommand);
          allGeneratedDict.Add(fileId, record);
        }
        else
        {
          // Filenames must match exactly (this detects file names with different casings)
          if (record.Filename != filename)
            throw new Exception($"'{filename}' from CreateAtlas '{sourceCommand.Name}' clashes with '{record.Filename}' defined by the atlases '{GetAtlasNameList(record.Source)}' due to different casing. Flavor atlas image files must use the exact same casing");

          record.Source.Add(sourceCommand);
        }
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void ValidateFlavorImages(Dictionary<string, ImageFileRecord> allImagesDict, ResolvedCommandCreateAtlasFlavor[] flavorArray, string debugAtlasName)
    {
      int expectedRecordCount = flavorArray.Length;

      // Validate that each file was defined in all packages
      g_logger.Trace("AtlasFlavor '{0}' validating that all {1} atlases contain all {2} image files", debugAtlasName, expectedRecordCount, allImagesDict.Count);

      var exceptions = new List<Exception>();
      foreach (var record in allImagesDict.Values)
      {
        if (record.Source.Count == expectedRecordCount)
        {
          g_logger.Trace("The file '{0}' is present in all flavors with compatible meta data", record.FileInfo.Name);
        }
        else
        {
          g_logger.Warn("The file '{0}' is not present in all flavors", record.FileInfo.Name);
          exceptions.Add(new Exception($"'{record.FileInfo.Name}' was only defined in {GetAtlasNameList(record.Source)} but not in {GetMissingAtlasNameList(flavorArray, record.Source)}"));
        }
      }
      if (exceptions.Count > 0)
      {
        throw new AggregateException(exceptions);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void ValidateFlavorGeneratedFiles(Dictionary<string, GeneratedFileRecord> allFilesDict, ResolvedCommandCreateAtlasFlavor[] flavorArray, string debugAtlasName)
    {
      int expectedRecordCount = flavorArray.Length;
      // Validate that each file was defined in all packages
      g_logger.Trace("AtlasFlavor '{0}' validating that all {1} atlases contain all {2} generated files", debugAtlasName, expectedRecordCount, allFilesDict.Count);

      var exceptions = new List<Exception>();
      foreach (var record in allFilesDict.Values)
      {
        if (record.Source.Count == expectedRecordCount)
        {
          g_logger.Trace("The generated file '{0}' is present in all flavors", record.Filename);
        }
        else
        {
          g_logger.Warn("The generated file '{0}' is not present in all flavors", record.Filename);
          exceptions.Add(new Exception($"'{record.Filename}' was only defined in {GetAtlasNameList(record.Source)} but not in {GetMissingAtlasNameList(flavorArray, record.Source)}"));
        }
      }
      if (exceptions.Count > 0)
      {
        throw new AggregateException(exceptions);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void ValidateFlavorFonts(Dictionary<string, FontRecord> allFontsDict, ResolvedCommandCreateAtlasFlavor[] flavors, string debugAtlasName)
    {
      int expectedRecordCount = flavors.Length;
      // Validate that each file was defined in all packages
      g_logger.Trace("AtlasFlavor '{0}' validating that all {1} atlases contain the same {2} fonts", debugAtlasName, expectedRecordCount, allFontsDict.Count);

      var exceptions = new List<Exception>();
      foreach (var record in allFontsDict.Values)
      {
        if (record.Source.Count == expectedRecordCount)
        {
          g_logger.Trace("The font '{0}' is present in all flavors", record.FontInfo.Name);
        }
        else
        {
          g_logger.Warn("The font '{0}' is not present in all flavors", record.FontInfo.Name);
          exceptions.Add(new Exception($"'{record.FontInfo.Name}' was only defined in {GetAtlasNameList(record.Source)} but not in {GetMissingAtlasNameList(flavors, record.Source)}"));
        }
      }
      if (exceptions.Count > 0)
      {
        throw new AggregateException(exceptions);
      }
    }


    public static string GetMissingAtlasNameList(ResolvedCommandCreateAtlasFlavor[] allArray, List<ResolvedCommandCreateAtlas> definedList)
    {
      if (allArray == null)
        throw new ArgumentNullException(nameof(allArray));
      if (definedList == null)
        throw new ArgumentNullException(nameof(definedList));
      var missingList = new List<ResolvedCommandCreateAtlas>(allArray.Length);
      foreach (var entry in allArray)
      {
        if (!definedList.Contains(entry.CreateAtlasCommand))
          missingList.Add(entry.CreateAtlasCommand);
      }
      return GetAtlasNameList(missingList);
    }

    public static string GetAtlasNameList(List<ResolvedCommandCreateAtlas> source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof(source));
      var names = new string[source.Count];
      for (int i = 0; i < source.Count; ++i)
        names[i] = source[i].Name;
      Array.Sort(names);
      return string.Join(',', names);
    }
    public static string GetAtlasNameList(ResolvedCommandCreateAtlas[] source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof(source));
      var names = new string[source.Length];
      for (int i = 0; i < source.Length; ++i)
        names[i] = source[i].Name;
      Array.Sort(names);
      return string.Join(',', names);
    }

    private static AtlasValidationInfo ExtractValidationInfo(ResolvedCommandCreateAtlasFlavor entry)
    {
      var flavors = new List<ImageFileInfo>();
      var generatedFiles = new List<string>();
      var fonts = new List<FontValidationInfo>();
      foreach (var command in entry.CreateAtlasCommand.Commands)
      {
        switch (command.Id)
        {
          case Atlas.AtlasCommandId.AddBitmapFont:
            {
              // FUTURE IMPROVEMENTS:
              // We do not know what files are added by the add bitmap font as that requires the loading and parsing of the file to have been completed
              // Because of this the verification done at this time will not be 100% correct.
              var commandEx = (ResolvedAtlasCommandAddBitmapFont)command;
              //result.Add(commandEx.ImageFile.Path.AbsolutePath);
              generatedFiles.Add(commandEx.DstFilename.RelativeUnresolvedSourcePath);
              fonts.Add(new FontValidationInfo(commandEx.DstFilename.UnresolvedSourcePath, commandEx.OutputFontFormats));
            }
            break;
          case Atlas.AtlasCommandId.AddFolder:
            {
              var commandEx = (ResolvedAtlasCommandAddFolder)command;
              foreach (var imageFileEntry in commandEx.ImageFiles)
              {
                flavors.Add(AnalyzeFile(imageFileEntry));
              }
            }
            break;
          case Atlas.AtlasCommandId.AddImage:
            {
              var commandEx = (ResolvedAtlasCommandAddImage)command;
              flavors.Add(AnalyzeFile(commandEx.ImageFile));
            }
            break;
          default:
            throw new NotSupportedException($"Unsupported create atlas command: {command.Id}");
        }
      }

      generatedFiles.Add(entry.CreateAtlasCommand.DstAtlasFilename.RelativeUnresolvedSourcePath);

      // Sort the list to get the errors in the same order each time
      flavors.Sort((lhs, rhs) => string.CompareOrdinal(lhs.Name, rhs.Name));
      generatedFiles.Sort();
      fonts.Sort((lhs, rhs) => string.CompareOrdinal(lhs.Name, rhs.Name));
      return new AtlasValidationInfo(flavors, generatedFiles, fonts);
    }

    private static ImageFileInfo AnalyzeFile(ResolvedImageFile entry)
    {
      return new ImageFileInfo(entry.AtlasPath.RelativeResolvedSourcePath, entry.GetMetaData());
    }

  }
}
