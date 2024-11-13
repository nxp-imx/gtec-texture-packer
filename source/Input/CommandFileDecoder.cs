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

using MB.Base;
using MB.Base.MathEx.Pixel;
using MB.Graphics2.Patch.Advanced;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using TexturePacker.Commands;
using TexturePacker.Commands.Atlas;
using TexturePacker.Config;

namespace TexturePacker.Input
{
  static class CommandFileDecoder
  {
    //private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    private const string ElementTexturePacker = "TexturePacker";

    private const string CmdAtlasConfig = "AtlasConfig";
    private const string CmdCreateAtlas = "CreateAtlas";
    private const string CmdAtlasFlavors = "AtlasFlavors";

    private static readonly string[] Cmds =
    {
      CmdAtlasConfig,
      CmdCreateAtlas,
      CmdAtlasFlavors,
    };

    private static readonly string[] AtlasFlavorCmds =
    {
      CmdCreateAtlas,
    };

    private const string AtlasCmdAddBitmapFont = "AddBitmapFont";
    private const string AtlasCmdAddFolder = "AddFolder";
    private const string AtlasCmdAddImage = "AddImage";
    private static readonly string[] AtlasCmds =
    {
      AtlasCmdAddBitmapFont,
      AtlasCmdAddFolder,
      AtlasCmdAddImage
    };

    private static readonly string[] AtlasCmdAddBitmapFontElements =
    {
      ConfigFileDecoder.ElementAtlasElementConfig,
    };

    private const string AddFolderChildMod = "Folder";
    private const string AddFolderChildFileMod = "File";


    private static readonly string[] AtlasCmdAddFolderElements =
    {
      ConfigFileDecoder.ElementAtlasElementConfig,
      AddFolderChildMod,
      AddFolderChildFileMod
    };

    private const string AtlasCmdAddNineSlice = "AddNineSlice";
    private const string AtlasCmdAddComplexPatch = "AddComplexPatch";
    private const string AtlasCmdAddAnchor = "AddAnchor";
    private static readonly string[] AddFolderChildElementEntryElements =
    {
      ConfigFileDecoder.ElementAtlasElementConfig,
      AtlasCmdAddNineSlice,
      AtlasCmdAddComplexPatch,
      AtlasCmdAddAnchor
    };

    private static readonly string[] AtlasCmdAddImageElements =
    {
      ConfigFileDecoder.ElementAtlasElementConfig,
    };

    private const string TexturePackerAttribute_Version = "Version";
    private const string TexturePackerAttribute_CreationYear = "CreationYear";
    private const string TexturePackerAttribute_CompanyName = "CompanyName";
    private const string TexturePackerAttribute_NamespaceName = "Namespace";

    private static readonly string[] TexturePackerAttributes =
    {
      TexturePackerAttribute_Version,
      TexturePackerAttribute_CreationYear,
      TexturePackerAttribute_CompanyName,
      TexturePackerAttribute_NamespaceName
    };

    private const string AtlasFlavorsAttribute_Name = "Name";
    private static readonly string[] AtlasFlavorsAttributes =
    {
      AtlasFlavorsAttribute_Name
    };

    private const string CreateAtlasAttribute_Name = "Name";
    private const string CreateAtlasAttribute_SourcePath = "SourcePath";
    private const string CreateAtlasAttribute_OutputAtlasFormat = "OutputAtlasFormat";
    private const string CreateAtlasAttribute_DefaultDpi = "DefaultDpi";
    private const string CreateAtlasAttribute_License = "License";
    private const string CreateAtlasAttribute_TransparencyMode = "TransparencyMode";
    private static readonly string[] CreateAtlasAttributes =
    {
      CreateAtlasAttribute_Name,
      CreateAtlasAttribute_SourcePath,
      CreateAtlasAttribute_OutputAtlasFormat,
      CreateAtlasAttribute_DefaultDpi,
      CreateAtlasAttribute_License,
      CreateAtlasAttribute_TransparencyMode,
    };

    private const string AddBitmapFontAttribute_OutputFontFormat = "OutputFontFormat";
    private const string AddBitmapFontAttribute_Path = "Path";
    private const string AddBitmapFontAttribute_Type = "Type";
    private const string AddBitmapFontAttribute_BaseLine = "BaseLinePx";
    private const string AddBitmapFontAttribute_LineSpacing = "LineSpacingPx";
    private const string AddBitmapFontAttribute_MeasureChar = "MeasureChar";
    private const string AddBitmapFontAttribute_MeasureHeight = "MeasureHeightPx";
    private const string AddBitmapFontAttribute_SdfSpread = "SdfSpread";
    private const string AddBitmapFontAttribute_SdfDesiredBaseLinePx = "SdfDesiredBaseLinePx";
    private const string AddBitmapFontAttribute_Name = "Name";

    private static readonly string[] AddBitmapFontAttributes =
    {
      AddBitmapFontAttribute_OutputFontFormat,
      AddBitmapFontAttribute_Path,
      AddBitmapFontAttribute_Type,
      AddBitmapFontAttribute_BaseLine,
      AddBitmapFontAttribute_LineSpacing,
      AddBitmapFontAttribute_MeasureChar,
      AddBitmapFontAttribute_MeasureHeight,
      AddBitmapFontAttribute_SdfSpread,
      AddBitmapFontAttribute_SdfDesiredBaseLinePx,
      AddBitmapFontAttribute_Name
    };

    private const string AddFolderAttribute_Path = "Path";
    private const string AddFolderAttribute_DefaultDpi = "DefaultDpi";
    private const string AddFolderAttribute_KeepDpiInFilename = "KeepDpiInFilename";

    private static readonly string[] AddFolderAttributes =
    {
      AddFolderAttribute_Path,
      AddFolderAttribute_DefaultDpi,
      AddFolderAttribute_KeepDpiInFilename
    };

    private const string AddFolderMod_Path = "Path";
    private static readonly string[] AddFolderModAttributes =
    {
      AddFolderMod_Path
    };

    private const string AddFolderFileMode_Path = "Path";
    private static readonly string[] AddFolderFileModAttributes =
    {
      AddFolderFileMode_Path
    };

    // AtlasCmdAddNineSlice

    private const string AddFolderEntryAddNineSlice_NineSlice = "NineSlice";
    private const string AddFolderEntryAddNineSlice_ContentMargin = "ContentMargin";
    private static readonly string[] AddFolderEntryAddNineSliceAttributes =
    {
      AddFolderEntryAddNineSlice_NineSlice,
      AddFolderEntryAddNineSlice_ContentMargin
    };

    private const string AddFolderEntryAddComplexPatch_SliceX = "SliceX";
    private const string AddFolderEntryAddComplexPatch_SliceY = "SliceY";
    private const string AddFolderEntryAddComplexPatch_ContentSliceX = "ContentSliceX";
    private const string AddFolderEntryAddComplexPatch_ContentSliceY = "ContentSliceY";
    private const string AddFolderEntryAddComplexPatch_Flags = "Flags";

    private static readonly string[] AddFolderEntryAddComplexPatchAttributes =
    {
      AddFolderEntryAddComplexPatch_SliceX,
      AddFolderEntryAddComplexPatch_SliceY,
      AddFolderEntryAddComplexPatch_ContentSliceX,
      AddFolderEntryAddComplexPatch_ContentSliceY,
      AddFolderEntryAddComplexPatch_Flags,
    };

    private const string AddFolderEntryAddAnchor_Position = "Position";

    private static readonly string[] AddFolderEntryAddAnchorAttributes =
    {
      AddFolderEntryAddAnchor_Position,
    };

    private const string AddImageAttribute_Path = "Path";
    private const string AddImageAttribute_Dpi = "Dpi";


    private static readonly string[] AddImageAttributes =
    {
      AddImageAttribute_Path,
      AddImageAttribute_Dpi
    };

    public static CommandGroup Decode(string strXmlContent, in TexturePackerConfig defaultConfig, string atlasFileSourceDirectoryPath,
                                      PathResolver pathResolver, string dstPath)
    {
      if (strXmlContent == null)
        throw new ArgumentNullException(nameof(strXmlContent));
      if (atlasFileSourceDirectoryPath == null)
        throw new ArgumentNullException(nameof(atlasFileSourceDirectoryPath));
      if (pathResolver == null)
        throw new ArgumentNullException(nameof(pathResolver));
      if (dstPath == null)
        throw new ArgumentNullException(nameof(dstPath));

      using (var reader = new StringReader(strXmlContent))
      {
        var element = XElement.Load(reader);
        return DecodeTexturePacker(element, defaultConfig, atlasFileSourceDirectoryPath, pathResolver, dstPath);
      }
    }

    public static CommandGroup DecodeTexturePacker(XElement element, in TexturePackerConfig defaultConfig, string atlasFileSourceDirectoryPath,
                                                   PathResolver pathResolver, string dstPath)
    {
      if (atlasFileSourceDirectoryPath == null)
        throw new ArgumentNullException(nameof(atlasFileSourceDirectoryPath));
      if (element.Name != ElementTexturePacker)
        throw new Exception($"Expected a {ElementTexturePacker} root element not {element.Name}");

      XmlUtil.ValidateAttributes(element, TexturePackerAttributes, XmlUtil.XmlNamespace);

      var strVersion = XmlUtil.GetAttributeValueAsString(element, TexturePackerAttribute_Version);
      if (strVersion != "1")
        throw new NotSupportedException($"Unsupported version '{strVersion}' expected '1'");

      var creationYear = XmlUtil.GetAttributeValueAsUInt32(element, TexturePackerAttribute_CreationYear, NumericCast.ToUInt32(DateTime.Now.Year));
      var companyName = XmlUtil.GetAttributeValueAsString(element, TexturePackerAttribute_CompanyName, defaultConfig.DefaultCompany);
      var namespaceName = XmlUtil.GetAttributeValueAsString(element, TexturePackerAttribute_NamespaceName, defaultConfig.DefaultNamespaceName);

      TexturePackerConfig currentConfig = defaultConfig;
      var commands = new List<Command>();
      bool configAlreadyModified = false;
      foreach (var descendant in element.Elements())
      {
        if (descendant.Name == CmdAtlasConfig)
        {
          if (commands.Count > 0)
            throw new Exception($"The '{CmdAtlasConfig}' must come before any other commands");
          if (configAlreadyModified)
            throw new Exception($"Only one '{CmdAtlasConfig}' cmd is allowed");
          configAlreadyModified = true;
          var newAtlasConfig = ConfigFileDecoder.DecodeAtlasConfig(descendant, currentConfig.CreateAtlas.Atlas, out bool modifiedTransparencyMode);
          currentConfig = TexturePackerConfig.PatchCreateAtlas(currentConfig, CreateAtlasConfig.PatchAtlasConfig(currentConfig.CreateAtlas, newAtlasConfig));
        }
        else if (descendant.Name == CmdCreateAtlas)
        {
          commands.Add(DecodeCreateAtlas(descendant, currentConfig.CreateAtlas, atlasFileSourceDirectoryPath));
        }
        else if (descendant.Name == CmdAtlasFlavors)
        {
          commands.Add(DecodeAtlasFlavors(descendant, currentConfig, atlasFileSourceDirectoryPath));
        }
        else
        {
          throw new NotSupportedException($"Unknown atlas command '{descendant.Name}'. Valid commands: '{string.Join(", ", Cmds)}'");
        }
      }

      return new CommandGroup(companyName, namespaceName, creationYear, commands, pathResolver, defaultConfig, atlasFileSourceDirectoryPath, dstPath);
    }

    private static CommandAtlasFlavors DecodeAtlasFlavors(XElement element, in TexturePackerConfig defaultConfig, string atlasFileSourceDirectoryPath)
    {
      if (atlasFileSourceDirectoryPath == null)
        throw new ArgumentNullException(nameof(atlasFileSourceDirectoryPath));
      if (element.Name != CmdAtlasFlavors)
        throw new Exception($"Expected a {CmdAtlasFlavors} root element not {element.Name}");
      XmlUtil.ValidateAttributes(element, AtlasFlavorsAttributes, XmlUtil.XmlNamespace);

      var name = XmlUtil.GetAttributeValueAsString(element, AtlasFlavorsAttribute_Name);

      var subCommands = new List<CommandCreateAtlas>();
      foreach (var descendant in element.Elements())
      {
        if (descendant.Name == CmdCreateAtlas)
        {
          subCommands.Add(DecodeCreateAtlas(descendant, defaultConfig.CreateAtlas, atlasFileSourceDirectoryPath));
        }
        else
        {
          throw new NotSupportedException($"Unknown atlas command '{descendant.Name}'. Valid commands: '{string.Join(", ", AtlasFlavorCmds)}'");
        }
      }
      return new CommandAtlasFlavors(name, subCommands);
    }

    public static CommandCreateAtlas DecodeCreateAtlas(XElement element, in CreateAtlasConfig defaultConfig, string atlasFileSourceDirectoryPath)
    {
      Debug.Assert(element.Name == CmdCreateAtlas);
      XmlUtil.ValidateAttributes(element, CreateAtlasAttributes);

      var name = XmlUtil.GetAttributeValueAsString(element, CreateAtlasAttribute_Name);
      var sourcePath = XmlUtil.GetAttributeValueAsString(element, CreateAtlasAttribute_SourcePath, "");
      var licenseFile = XmlUtil.TryGetAttributeValueAsString(element, CreateAtlasAttribute_License);
      var outputAtlasFormat = XmlUtil.GetAttributeValueAsOutputAtlasFormat(element, CreateAtlasAttribute_OutputAtlasFormat, defaultConfig.OutputFormat);

      bool overrideTransparencyMode = XmlUtil.TryGetAttributeValueAsTransparencyMode(element, CreateAtlasAttribute_TransparencyMode, out TransparencyMode transparencyModeOverride);

      UInt16 defaultDpi;
      bool hasDpi = XmlUtil.TryGetAttributeValueAsUInt16(element, CreateAtlasAttribute_DefaultDpi, out defaultDpi);

      UInt16 nameDefaultDpi = FileNameUtil.TryDecodeNameDpi(name);

      var defaultAtlasElement = defaultConfig.Atlas.Element;
      if (!hasDpi)
      {
        // Use the name encoded one if it exist, before the default
        defaultDpi = nameDefaultDpi > 0 ? nameDefaultDpi : defaultAtlasElement.DefaultDpi;
      }
      else if (nameDefaultDpi > 0 && nameDefaultDpi != defaultDpi)
      {
        throw new NotSupportedException($"Atlas name '{name}' default dpi of {nameDefaultDpi} does not match the set defaultDpi {defaultDpi}");
      }

      // patch the AtlasConfig
      var activeAtlasElement = new AtlasElementConfig(defaultDpi, defaultAtlasElement.Extrude, defaultAtlasElement.Trim, defaultAtlasElement.TrimMargin,
                                                      defaultAtlasElement.TransparencyThreshold, defaultAtlasElement.ShapePadding,
                                                      defaultAtlasElement.BorderPadding);
      var activeDefaultAtlasConfig = new AtlasConfig(defaultConfig.Atlas.TransparencyMode, defaultConfig.Atlas.Texture, defaultConfig.Atlas.Layout, activeAtlasElement);
      var activeConfig = new CreateAtlasConfig(outputAtlasFormat, activeDefaultAtlasConfig, defaultConfig.AddBitmapFont);
      var (atlasConfig, modifiedTransparencyMode, atlasCommands) = ParseAtlasChildren(element, activeConfig);

      if (overrideTransparencyMode && atlasConfig.TransparencyMode != transparencyModeOverride)
      {
        if (modifiedTransparencyMode)
        {
          throw new Exception($"CreateAtlas Attribute '{CreateAtlasAttribute_OutputAtlasFormat}' was specified and a sub AtlasConfig with a different transparency mode was specified. Remove one of them");
        }
        atlasConfig = new AtlasConfig(transparencyModeOverride, atlasConfig.Texture, atlasConfig.Layout, atlasConfig.Element);
      }

      return new CommandCreateAtlas(atlasFileSourceDirectoryPath, name, sourcePath, licenseFile, atlasConfig, atlasCommands, outputAtlasFormat);
    }

    private static Tuple<AtlasConfig, bool, AtlasCommand[]> ParseAtlasChildren(XElement element, in CreateAtlasConfig activeConfig)
    {
      var atlasConfig = activeConfig.Atlas;

      bool atlasConfigFound = false;
      bool modifiedTransparencyMode = false;
      var commands = new List<AtlasCommand>();
      foreach (var entry in element.Elements())
      {
        if (entry.Name == ConfigFileDecoder.ElementAtlasConfig)
        {
          if (commands.Count > 0)
            throw new Exception($"The '{ConfigFileDecoder.ElementAtlasConfig}' must come before any atlas commands");
          if (atlasConfigFound)
            throw new Exception($"The '{ConfigFileDecoder.ElementAtlasConfig}' can only be listed once");
          atlasConfig = ConfigFileDecoder.DecodeAtlasConfig(entry, atlasConfig, out modifiedTransparencyMode);
          atlasConfigFound = true;
        }
        else
        {
          commands.Add(ParseAtlasCommand(entry, activeConfig));
        }
      }
      return Tuple.Create(atlasConfig, modifiedTransparencyMode, commands.ToArray());
    }



    private static AtlasCommand ParseAtlasCommand(XElement element, in CreateAtlasConfig defaultConfig)
    {
      if (element.Name == AtlasCmdAddBitmapFont)
      {
        return ParseAtlasCommandAddBitmapFont(element, defaultConfig.AddBitmapFont, defaultConfig.Atlas.Element);
      }
      else if (element.Name == AtlasCmdAddFolder)
      {
        return ParseAtlasCommandAddFolder(element, defaultConfig.Atlas.Element);
      }
      else if (element.Name == AtlasCmdAddImage)
      {
        return ParseAtlasCommandAddImage(element, defaultConfig.Atlas.Element);
      }
      else
      {
        throw new NotSupportedException($"Unknown atlas command '{element.Name}'. Valid commands: '{string.Join(", ", AtlasCmds)}'");
      }
    }

    private static AtlasCommand ParseAtlasCommandAddBitmapFont(XElement element, AddBitmapFontConfig defaultConfig, AtlasElementConfig defaultElementConfig)
    {
      // Force bitmap fonts to default to zero extrude and zero trim margin
      const int extrude = 0;
      const int trimMargin = 0;
      defaultElementConfig = new AtlasElementConfig(defaultElementConfig.DefaultDpi, extrude, defaultElementConfig.Trim,
                                                    trimMargin, defaultElementConfig.TransparencyThreshold,
                                                    defaultElementConfig.ShapePadding, defaultElementConfig.BorderPadding);

      Debug.Assert(element.Name == AtlasCmdAddBitmapFont);
      XmlUtil.ValidateAttributes(element, AddBitmapFontAttributes);
      var elementConfig = ParseAtlasCommandAddBitmapFontChildren(element, defaultElementConfig);

      string path = XmlUtil.GetAttributeValueAsString(element, AddBitmapFontAttribute_Path);
      string? name = XmlUtil.TryGetAttributeValueAsString(element, AddBitmapFontAttribute_Name);
      var fontType = XmlUtil.GetAttributeValueAsBitmapFontType(element, AddBitmapFontAttribute_Type, defaultConfig.FontType);
      var outputFontFormats = XmlUtil.GetAttributeValueAsOutputFontFormatHashSet(element, AddBitmapFontAttribute_OutputFontFormat, defaultConfig.OutputFormat);

      UInt16 forcedBaseLinePx = XmlUtil.GetAttributeValueAsUInt16(element, AddBitmapFontAttribute_BaseLine, 0);
      UInt16 forcedLineSpacingPx = XmlUtil.GetAttributeValueAsUInt16(element, AddBitmapFontAttribute_LineSpacing, 0);
      UInt32 measureCharId = Convert.ToUInt32(XmlUtil.GetAttributeValueAsChar(element, AddBitmapFontAttribute_MeasureChar, (char)0));
      UInt16 measureHeightPx = XmlUtil.GetAttributeValueAsUInt16(element, AddBitmapFontAttribute_MeasureHeight, 0);

      BitmapFontSdfConfig? sdfConfig = null;
      if (fontType == FslGraphics.Font.BF.BitmapFontType.SDF)
      {
        UInt16 sdfSpread;
        if (!XmlUtil.TryGetAttributeValueAsUInt16(element, AddBitmapFontAttribute_SdfSpread, out sdfSpread))
        {
          throw new Exception(string.Format("SDF fonts must define a '{0}' attribute in the range >= 1u)", AddBitmapFontAttribute_SdfSpread));
        }
        UInt16 sdfDesiredBaseLinePx = XmlUtil.GetAttributeValueAsUInt16(element, AddBitmapFontAttribute_SdfDesiredBaseLinePx, 0);
        sdfConfig = new BitmapFontSdfConfig(sdfSpread, sdfDesiredBaseLinePx);
      }

      var bitmapFontTweak = new BitmapFontTweakConfig(forcedBaseLinePx, forcedLineSpacingPx, measureCharId, measureHeightPx, sdfConfig);
      return new AtlasCommandAddBitmapFont(elementConfig, path, name, fontType, outputFontFormats, bitmapFontTweak);
    }

    private static AtlasCommand ParseAtlasCommandAddFolder(XElement element, AtlasElementConfig defaultElementConfig)
    {
      Debug.Assert(element.Name == AtlasCmdAddFolder);
      XmlUtil.ValidateAttributes(element, AddFolderAttributes);

      var folderModList = new List<AtlasCommandAddFolderMod>();
      var fileModList = new List<AtlasCommandAddFolderFileMod>();
      var elementConfig = ParseAtlasCommandAddFolderChildren(element, folderModList, fileModList, defaultElementConfig);

      string path = XmlUtil.GetAttributeValueAsString(element, AddFolderAttribute_Path);
      UInt16 defaultDpi;
      if (!XmlUtil.TryGetAttributeValueAsUInt16(element, AddFolderAttribute_DefaultDpi, out defaultDpi))
      {
        defaultDpi = elementConfig.DefaultDpi;
      }
      bool keepDpiInFilename = XmlUtil.GetAttributeValueAsBool(element, AddFolderAttribute_KeepDpiInFilename, false);


      return new AtlasCommandAddFolder(elementConfig, path, keepDpiInFilename, folderModList, fileModList);
    }

    private static AtlasCommand ParseAtlasCommandAddImage(XElement element, AtlasElementConfig defaultElementConfig)
    {
      Debug.Assert(element.Name == AtlasCmdAddImage);
      XmlUtil.ValidateAttributes(element, AddImageAttributes);
      var (elementConfig, addNineSlice, addComplexPatch, addAnchor) = ParseAtlasCommandAddImageChildren(element, defaultElementConfig);

      UInt16 dpi;
      if (!XmlUtil.TryGetAttributeValueAsUInt16(element, AddImageAttribute_Dpi, out dpi))
      {
        dpi = 0;
      }

      string path = XmlUtil.GetAttributeValueAsString(element, AddImageAttribute_Path);
      return new AtlasCommandAddImage(elementConfig, path, dpi, addNineSlice, addComplexPatch, addAnchor);
    }

    private static AtlasElementConfig ParseAtlasCommandAddBitmapFontChildren(XElement element, AtlasElementConfig elementConfig)
    {
      bool isFirstElement = true;
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ConfigFileDecoder.ElementAtlasElementConfig)
        {
          if (!isFirstElement)
            throw new NotSupportedException($"The element '{decendant.Name}' must be the first child element");
          elementConfig = ConfigFileDecoder.DecodeAtlasElementConfig(decendant, elementConfig);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: '{string.Join(", ", AtlasCmdAddBitmapFontElements)}'");
        }
        isFirstElement = false;
      }
      return elementConfig;
    }

    private static AtlasElementConfig ParseAtlasCommandAddFolderChildren(XElement element, List<AtlasCommandAddFolderMod> folderModList,
                                                                         List<AtlasCommandAddFolderFileMod> fileModList,
                                                                         AtlasElementConfig elementConfig)
    {
      bool isFirstElement = true;
      bool blockAddFolder = false;
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ConfigFileDecoder.ElementAtlasElementConfig)
        {
          if (!isFirstElement)
            throw new NotSupportedException($"The element '{decendant.Name}' must be the first child element");
          elementConfig = ConfigFileDecoder.DecodeAtlasElementConfig(decendant, elementConfig);
        }
        else if (decendant.Name == AddFolderChildMod)
        {
          if (blockAddFolder)
            throw new NotSupportedException($"The element '{decendant.Name}' must can not follow {AddFolderChildFileMod}");

          folderModList.Add(ParseFolderModElement(decendant, elementConfig));
        }
        else if (decendant.Name == AddFolderChildFileMod)
        {
          fileModList.Add(ParseFileElement(decendant, elementConfig));
          blockAddFolder = true;
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: '{string.Join(", ", AtlasCmdAddFolderElements)}'");
        }
        isFirstElement = false;
      }
      return elementConfig;
    }




    private static AtlasCommandAddFolderMod ParseFolderModElement(XElement element, AtlasElementConfig elementConfig)
    {
      Debug.Assert(element.Name == AddFolderChildMod);
      XmlUtil.ValidateAttributes(element, AddFolderModAttributes);

      string path = XmlUtil.GetAttributeValueAsString(element, AddFolderMod_Path);

      bool isFirstElement = true;
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ConfigFileDecoder.ElementAtlasElementConfig)
        {
          if (!isFirstElement)
            throw new NotSupportedException($"The element '{decendant.Name}' must be the first child element");
          elementConfig = ConfigFileDecoder.DecodeAtlasElementConfig(decendant, elementConfig);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: '{string.Join(", ", AddFolderChildElementEntryElements)}'");
        }
        isFirstElement = false;
      }
      return new AtlasCommandAddFolderMod(path, elementConfig);
    }


    private static AtlasCommandAddFolderFileMod ParseFileElement(XElement element, AtlasElementConfig elementConfig)
    {
      Debug.Assert(element.Name == AddFolderChildFileMod);
      XmlUtil.ValidateAttributes(element, AddFolderFileModAttributes);

      string path = XmlUtil.GetAttributeValueAsString(element, AddFolderFileMode_Path);

      bool isFirstElement = true;
      AddNineSlice? nineSlice = null;
      AddComplexPatch? complexPatch = null;
      AddAnchor? anchor = null;

      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ConfigFileDecoder.ElementAtlasElementConfig)
        {
          if (!isFirstElement)
            throw new NotSupportedException($"The element '{decendant.Name}' must be the first child element");
          elementConfig = ConfigFileDecoder.DecodeAtlasElementConfig(decendant, elementConfig);
        }
        else if (decendant.Name == AtlasCmdAddNineSlice)
        {
          if (complexPatch != null)
            throw new NotSupportedException($"Can not add a nineslice to a element '{element.Name}' that contains a complex patch");
          nineSlice = ParseNineSlice(decendant);
        }
        else if (decendant.Name == AtlasCmdAddComplexPatch)
        {
          if (nineSlice != null)
            throw new NotSupportedException($"Can not add a complex patch to a element '{element.Name}' that contains a nineslice");
          complexPatch = ParseComplexPatch(decendant);
        }
        else if (decendant.Name == AtlasCmdAddAnchor)
        {
          if (anchor != null)
            throw new NotSupportedException($"Can not add a anchor to a element '{element.Name}' that contains a anchor");
          anchor = ParseAddAnchor(decendant);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: '{string.Join(", ", AddFolderChildElementEntryElements)}'");
        }
        isFirstElement = false;
      }
      return new AtlasCommandAddFolderFileMod(path, elementConfig, nineSlice, complexPatch, anchor);
    }

    private static AddNineSlice ParseNineSlice(XElement element)
    {
      Debug.Assert(element.Name == AtlasCmdAddNineSlice);
      XmlUtil.ValidateAttributes(element, AddFolderEntryAddNineSliceAttributes);

      var nineSlicePx = XmlUtil.GetAttributeValueAsPxThicknessU(element, AddFolderEntryAddNineSlice_NineSlice, new PxThicknessU());
      var contentMarginPx = XmlUtil.GetAttributeValueAsPxThicknessU(element, AddFolderEntryAddNineSlice_ContentMargin, new PxThicknessU());

      return new AddNineSlice(nineSlicePx, contentMarginPx);
    }

    private static AddComplexPatch ParseComplexPatch(XElement element)
    {
      Debug.Assert(element.Name == AtlasCmdAddComplexPatch);
      XmlUtil.ValidateAttributes(element, AddFolderEntryAddComplexPatchAttributes);

      var strSliceX = XmlUtil.GetAttributeValueAsStringArray(element, AddFolderEntryAddComplexPatch_SliceX);
      var strSliceY = XmlUtil.GetAttributeValueAsStringArray(element, AddFolderEntryAddComplexPatch_SliceY);
      var strContentSliceX = XmlUtil.GetAttributeValueAsStringArray(element, AddFolderEntryAddComplexPatch_ContentSliceX);
      var strContentSliceY = XmlUtil.GetAttributeValueAsStringArray(element, AddFolderEntryAddComplexPatch_ContentSliceY);
      ComplexPatchFlags flags = XmlUtil.GetAttributeValueAsComplexPatchFlags(element, AddFolderEntryAddComplexPatch_Flags, ComplexPatchFlags.None);

      ImmutableComplexPatchSlice[] sliceArrayX = StringParser.ParseAsImmutableComplexPatchSliceArray(strSliceX);
      ImmutableComplexPatchSlice[] sliceArrayY = StringParser.ParseAsImmutableComplexPatchSliceArray(strSliceY);
      ImmutableContentSlice[] contentSliceArrayX = StringParser.ParseAsImmutableContentSliceArray(strContentSliceX);
      ImmutableContentSlice[] contentSliceArrayY = StringParser.ParseAsImmutableContentSliceArray(strContentSliceY);

      var patch = ImmutablePatchHelper.CreateTransparentComplexPatch(sliceArrayX, sliceArrayY, contentSliceArrayX, contentSliceArrayY, flags);
      return new AddComplexPatch(patch);
    }


    private static AddAnchor? ParseAddAnchor(XElement element)
    {
      Debug.Assert(element.Name == AtlasCmdAddAnchor);
      XmlUtil.ValidateAttributes(element, AddFolderEntryAddAnchorAttributes);
      var points = XmlUtil.GetAttributeValueAsPxPointArray(element, AddFolderEntryAddAnchor_Position);
      if (points.Length <= 0)
        return null;
      return new AddAnchor(ImmutableArray.Create(points));
    }


    private static Tuple<AtlasElementConfig, AddNineSlice?, AddComplexPatch?, AddAnchor?> ParseAtlasCommandAddImageChildren(XElement element, AtlasElementConfig elementConfig)
    {
      bool isFirstElement = true;
      AddNineSlice? nineSlice = null;
      AddComplexPatch? complexPatch = null;
      AddAnchor? anchor = null;
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ConfigFileDecoder.ElementAtlasElementConfig)
        {
          if (!isFirstElement)
            throw new NotSupportedException($"The element '{decendant.Name}' must be the first child element");
          elementConfig = ConfigFileDecoder.DecodeAtlasElementConfig(decendant, elementConfig);
        }
        else if (decendant.Name == AtlasCmdAddNineSlice)
        {
          if (complexPatch != null)
            throw new NotSupportedException($"Can not add a nineslice to a element '{element.Name}' that contains a complex patch");

          nineSlice = ParseNineSlice(decendant);
        }
        else if (decendant.Name == AtlasCmdAddComplexPatch)
        {
          if (nineSlice != null)
            throw new NotSupportedException($"Can not add a complex patch to a element '{element.Name}' that contains a nineslice");
          complexPatch = ParseComplexPatch(decendant);
        }
        else if (decendant.Name == AtlasCmdAddAnchor)
        {
          if (anchor != null)
            throw new NotSupportedException($"Can not add a anchor to a element '{element.Name}' that contains a anchor");
          anchor = ParseAddAnchor(decendant);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: '{string.Join(", ", AtlasCmdAddImageElements)}'");
        }
        isFirstElement = false;
      }
      return Tuple.Create(elementConfig, nineSlice, complexPatch, anchor);
    }
  }
}
