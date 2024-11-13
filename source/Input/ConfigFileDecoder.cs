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
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using TexturePacker.Config;

namespace TexturePacker.Input
{
  static class ConfigFileDecoder
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    private const string ElementTexturePackerConfig = "TexturePackerConfig";
    private const string ElementCreateAtlasConfig = "CreateAtlasConfig";
    public const string ElementLicenseConfig = "LicenseConfig";
    private static readonly string[] ElementTexturePackerConfigElements =
    {
      ElementCreateAtlasConfig,
      ElementLicenseConfig
    };


    public const string ElementAtlasConfig = "AtlasConfig";
    public const string ElementAddBitmapFontConfig = "AddBitmapFontConfig";
    private static readonly string[] ElementCreateAtlasConfigElements =
    {
      ElementAtlasConfig, ElementAddBitmapFontConfig
    };


    public const string ElementAtlasElementConfig = "ElementConfig";
    private const string ElementAtlasLayoutConfig = "LayoutConfig";
    private const string ElementAtlasTextureConfig = "TextureConfig";
    private static readonly string[] ElementAtlasConfigConfigElements =
    {
      ElementAtlasElementConfig, ElementAtlasLayoutConfig, ElementAtlasTextureConfig
    };

    private const string TexturePackerConfigAttribute_Version = "Version";
    private const string TexturePackerConfigAttribute_DefaultCompany = "DefaultCompany";
    private const string TexturePackerConfigAttribute_DefaultNamespace = "DefaultNamespace";
    private const string TexturePackerConfigAttribute_DefaultFilename = "DefaultFilename";

    private static readonly string[] TexturePackerConfigAttributes =
    {
      TexturePackerConfigAttribute_Version,
      TexturePackerConfigAttribute_DefaultCompany,
      TexturePackerConfigAttribute_DefaultNamespace,
      TexturePackerConfigAttribute_DefaultFilename
    };

    private const string CreateAtlasConfigAttribute_OutputAtlasFormat = "OutputAtlasFormat";
    private const string CreateAtlasConfigAttribute_DefaultDp = "DefaultDpi";
    private static readonly string[] CreateAtlasConfigAttributes =
    {
      CreateAtlasConfigAttribute_OutputAtlasFormat,
      CreateAtlasConfigAttribute_DefaultDp,
    };

    private const string AtlasConfigAttribute_TransparencyMode = "TransparencyMode";
    private static readonly string[] AtlasConfigAttributes =
    {
      AtlasConfigAttribute_TransparencyMode,
    };

    private const string AddBitmapFontConfigAttribute_FontType = "Type";
    private const string AddBitmapFontConfigAttribute_OutputFontFormat = "OutputFontFormat";
    private static readonly string[] AddBitmapFontConfigAttributes =
    {
      AddBitmapFontConfigAttribute_FontType,
      AddBitmapFontConfigAttribute_OutputFontFormat
    };

    private const string LicenseConfigAttribute_File = "File";
    private const string LicenseConfigAttribute_Type = "Type";
    private const string LicenseConfigAttribute_AllowMasterFileCreation = "AllowMasterFileCreation";
    private const string LicenseConfigAttribute_RequiredForAllContent = "RequiredForAllContent";
    private static readonly string[] LicenseConfigAttributes =
    {
      LicenseConfigAttribute_File,
      LicenseConfigAttribute_Type,
      LicenseConfigAttribute_AllowMasterFileCreation,
      LicenseConfigAttribute_RequiredForAllContent
    };

    private const string AtlasElementConfigAttribute_DefaultDp = "DefaultDpi";
    private const string AtlasElementConfigAttribute_Extrude = "Extrude";
    private const string AtlasElementConfigAttribute_Trim = "Trim";
    private const string AtlasElementConfigAttribute_TrimMargin = "TrimMargin";
    private const string AtlasElementConfigAttribute_TransparencyThreshold = "TransparencyThreshold";
    private const string AtlasElementConfigAttribute_TransparencyThresholdDeprecated = "TransparencyTreshold";
    private const string AtlasElementConfigAttribute_ShapePadding = "ShapePadding";
    private const string AtlasElementConfigAttribute_BorderPadding = "BorderPadding";
    private static readonly string[] AtlasElementConfigAttributes =
    {
      AtlasElementConfigAttribute_DefaultDp,
      AtlasElementConfigAttribute_Extrude,
      AtlasElementConfigAttribute_Trim,
      AtlasElementConfigAttribute_TrimMargin,
      AtlasElementConfigAttribute_TransparencyThreshold,
      AtlasElementConfigAttribute_TransparencyThresholdDeprecated,
      AtlasElementConfigAttribute_ShapePadding,
      AtlasElementConfigAttribute_BorderPadding
    };

    private const string AtlasLayoutConfigAttribute_AllowRotation = "AllowRotation";
    private static readonly string[] AtlasLayoutConfigAttributes =
    {
      AtlasLayoutConfigAttribute_AllowRotation,
    };

    private const string AtlasTextureConfigAttribute_MaxSize = "MaxSize";
    private const string AtlasTextureConfigAttribute_SizeRestriction = "SizeRestriction";
    private static readonly string[] AtlasTextureConfigAttributes =
    {
      AtlasTextureConfigAttribute_MaxSize,
      AtlasTextureConfigAttribute_SizeRestriction,
    };




    public static TexturePackerConfig Decode(string strXmlContent, in TexturePackerConfig defaultValues)
    {
      if (strXmlContent == null)
        throw new ArgumentNullException(nameof(strXmlContent));

      using (var reader = new StringReader(strXmlContent))
      {
        var element = XElement.Load(reader);
        return DecodeTexturePackerConfig(element, defaultValues);
      }
    }

    private static TexturePackerConfig DecodeTexturePackerConfig(XElement element, in TexturePackerConfig defaultValues)
    {
      if (element.Name != ElementTexturePackerConfig)
        throw new Exception($"Expected a '{ElementTexturePackerConfig}' root element not '{element.Name}'");

      var strVersion = XmlUtil.GetAttributeValueAsString(element, TexturePackerConfigAttribute_Version);
      if (strVersion != "1")
        throw new NotSupportedException($"Unsupported version '{strVersion}' expected '1'");

      string? strDefaultCompany = XmlUtil.TryGetAttributeValueAsString(element, TexturePackerConfigAttribute_DefaultCompany);
      string? strDefaultNamespace = XmlUtil.TryGetAttributeValueAsString(element, TexturePackerConfigAttribute_DefaultNamespace);
      string defaultFilename = XmlUtil.GetAttributeValueAsString(element, TexturePackerConfigAttribute_DefaultFilename, defaultValues.DefaultFilename);

      if (strDefaultCompany == null)
      {
        g_logger.Warn("Empty default company name found, please supply one");
        strDefaultCompany = defaultValues.DefaultCompany;
      }

      XmlUtil.ValidateAttributes(element, TexturePackerConfigAttributes, XmlUtil.XmlNamespace);


      var createAtlasConfig = defaultValues.CreateAtlas;
      var licenseConfig = defaultValues.License;
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ElementCreateAtlasConfig)
        {
          createAtlasConfig = DecodeCreateAtlasConfig(decendant, defaultValues.CreateAtlas);
        }
        else if (decendant.Name == ElementLicenseConfig)
        {
          licenseConfig = DecodeLicenseConfig(decendant, defaultValues.License);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are '{string.Join(", ", ElementTexturePackerConfigElements)}' ");
        }
      }
      return new TexturePackerConfig(strDefaultCompany, strDefaultNamespace, defaultFilename, createAtlasConfig, licenseConfig);
    }

    private static LicenseConfig DecodeLicenseConfig(XElement element, in LicenseConfig defaultValues)
    {
      Debug.Assert(element.Name == ElementLicenseConfig);
      XmlUtil.ValidateAttributes(element, LicenseConfigAttributes);

      var file = XmlUtil.GetAttributeValueAsString(element, LicenseConfigAttribute_File);
      var licenseFormat = XmlUtil.GetAttributeValueAsLicenseFormat(element, LicenseConfigAttribute_Type, defaultValues.LicenseFormat);
      var allowMasterFileCreation = XmlUtil.GetAttributeValueAsBool(element, LicenseConfigAttribute_AllowMasterFileCreation, defaultValues.AllowMasterFileCreation);
      var requiredForAllContent = XmlUtil.GetAttributeValueAsBool(element, LicenseConfigAttribute_RequiredForAllContent, defaultValues.RequiredForAllContent);
      return new LicenseConfig(file, licenseFormat, allowMasterFileCreation, requiredForAllContent);
    }


    public static CreateAtlasConfig DecodeCreateAtlasConfig(XElement element, in CreateAtlasConfig defaultValues)
    {
      Debug.Assert(element.Name == ElementCreateAtlasConfig);
      XmlUtil.ValidateAttributes(element, CreateAtlasConfigAttributes);

      var outputFormat = XmlUtil.GetAttributeValueAsOutputAtlasFormat(element, CreateAtlasConfigAttribute_OutputAtlasFormat, defaultValues.OutputFormat);

      var atlasConfig = defaultValues.Atlas;
      var addBitmapFontConfig = defaultValues.AddBitmapFont;
      bool wasTransparencyModeModified = false;
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ElementAtlasConfig)
        {
          atlasConfig = DecodeAtlasConfig(decendant, defaultValues.Atlas, out wasTransparencyModeModified);
        }
        else if (decendant.Name == ElementAddBitmapFontConfig)
        {
          addBitmapFontConfig = DecodeAddBitmapFontConfig(decendant, defaultValues.AddBitmapFont);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: {string.Join(", ", ElementCreateAtlasConfigElements)}");
        }
      }
      return new CreateAtlasConfig(outputFormat, atlasConfig, addBitmapFontConfig);
    }

    private static AddBitmapFontConfig DecodeAddBitmapFontConfig(XElement element, AddBitmapFontConfig defaultValues)
    {
      Debug.Assert(element.Name == ElementAddBitmapFontConfig);
      XmlUtil.ValidateAttributes(element, AddBitmapFontConfigAttributes);

      var bitmapFontType = XmlUtil.GetAttributeValueAsBitmapFontType(element, AddBitmapFontConfigAttribute_FontType, defaultValues.FontType);
      var outputFormat = XmlUtil.GetAttributeValueAsOutputFontFormat(element, AddBitmapFontConfigAttribute_OutputFontFormat, defaultValues.OutputFormat);
      return new AddBitmapFontConfig(bitmapFontType, outputFormat);
    }

    public static AtlasConfig DecodeAtlasConfig(XElement element, in AtlasConfig defaultValues, out bool rModifiedTransparencyMode)
    {
      Debug.Assert(element.Name == ElementAtlasConfig);

      XmlUtil.ValidateAttributes(element, AtlasConfigAttributes);

      rModifiedTransparencyMode = XmlUtil.TryGetAttributeValueAsTransparencyMode(element, AtlasConfigAttribute_TransparencyMode, out TransparencyMode transparencyMode);
      if (!rModifiedTransparencyMode)
        transparencyMode = defaultValues.TransparencyMode;

      var textureConfig = defaultValues.Texture;
      var layoutConfig = defaultValues.Layout;
      var elementConfig = defaultValues.Element;

      // parse the child config elements
      foreach (var decendant in element.Elements())
      {
        if (decendant.Name == ElementAtlasElementConfig)
        {
          elementConfig = DecodeAtlasElementConfig(decendant, defaultValues.Element);
        }
        else if (decendant.Name == ElementAtlasLayoutConfig)
        {
          layoutConfig = DecodeAtlasLayoutConfig(decendant, defaultValues.Layout);
        }
        else if (decendant.Name == ElementAtlasTextureConfig)
        {
          textureConfig = DecodeAtlasTextureConfig(decendant, defaultValues.Texture);
        }
        else
        {
          throw new NotSupportedException($"Unsupported element '{decendant.Name}', allowed elements are: {string.Join(", ", ElementAtlasConfigConfigElements)}");
        }
      }
      return new AtlasConfig(transparencyMode, textureConfig, layoutConfig, elementConfig);
    }


    public static AtlasElementConfig DecodeAtlasElementConfig(XElement element, AtlasElementConfig defaultValues)
    {
      Debug.Assert(element.Name == ElementAtlasElementConfig);
      XmlUtil.ValidateAttributes(element, AtlasElementConfigAttributes);

      var defaultDp = XmlUtil.GetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_DefaultDp, defaultValues.DefaultDpi);
      var extrude = XmlUtil.GetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_Extrude, defaultValues.Extrude);
      var trim = XmlUtil.GetAttributeValueAsBool(element, AtlasElementConfigAttribute_Trim, defaultValues.Trim);
      var trimMargin = XmlUtil.GetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_TrimMargin, defaultValues.TrimMargin);
      UInt16 transparencyThreshold = GetTransparencyThreshold(element, defaultValues);

      var shapePadding = XmlUtil.GetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_ShapePadding, defaultValues.ShapePadding);
      var borderPadding = XmlUtil.GetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_BorderPadding, defaultValues.BorderPadding);

      return new AtlasElementConfig(defaultDp, extrude, trim, trimMargin, transparencyThreshold, shapePadding, borderPadding);
    }

    private static UInt16 GetTransparencyThreshold(XElement element, AtlasElementConfig defaultValues)
    {
      ushort transparencyThreshold;
      if (!XmlUtil.TryGetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_TransparencyThreshold, out transparencyThreshold))
      {
        // Try the deprecated value
        if (!XmlUtil.TryGetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_TransparencyThresholdDeprecated, out transparencyThreshold))
        {
          transparencyThreshold = defaultValues.TransparencyThreshold;
        }
        else
        {
          g_logger.Warn($"Deprecated element '{AtlasElementConfigAttribute_TransparencyThresholdDeprecated}' found please update it to '{AtlasElementConfigAttribute_TransparencyThreshold}'");
        }
      }
      else
      {
        UInt16 dummy;
        if (XmlUtil.TryGetAttributeValueAsUInt16(element, AtlasElementConfigAttribute_TransparencyThresholdDeprecated, out dummy))
        {
          g_logger.Warn($"Found both the new '{AtlasElementConfigAttribute_TransparencyThreshold}' and the deprecated '{AtlasElementConfigAttribute_TransparencyThresholdDeprecated}' element so the deprecated one was ignored, please remove it");
        }
      }

      return transparencyThreshold;
    }

    private static AtlasLayoutConfig DecodeAtlasLayoutConfig(XElement element, AtlasLayoutConfig defaultValues)
    {
      Debug.Assert(element.Name == ElementAtlasLayoutConfig);
      XmlUtil.ValidateAttributes(element, AtlasLayoutConfigAttributes);

      var allowRotation = XmlUtil.GetAttributeValueAsBool(element, AtlasLayoutConfigAttribute_AllowRotation, defaultValues.AllowRotation);
      return new AtlasLayoutConfig(allowRotation);
    }

    private static AtlasTextureConfig DecodeAtlasTextureConfig(XElement element, AtlasTextureConfig defaultValues)
    {
      Debug.Assert(element.Name == ElementAtlasTextureConfig);
      XmlUtil.ValidateAttributes(element, AtlasTextureConfigAttributes);

      var maxSize = XmlUtil.GetAttributeValueAsPxSize2D(element, AtlasTextureConfigAttribute_MaxSize, defaultValues.MaxSize);
      var sizeRestriction = XmlUtil.GetAttributeValueAsTextureSizeRestriction(element, AtlasTextureConfigAttribute_SizeRestriction, defaultValues.SizeRestriction);

      return new AtlasTextureConfig(maxSize, sizeRestriction);
    }

  }
}
