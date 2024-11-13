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

using FslGraphics.Font.BF;
using MB.Base.MathEx.Pixel;
using MB.RectangleBinPack.TexturePack;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace TexturePacker.Input
{
  public static class XmlUtil
  {
    public static readonly string XmlNamespace = "http://www.w3.org/2000/xmlns/";
    private static readonly IFormatProvider g_invariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    public static string GetAttributeValueAsString(XElement xmlElement, string attributeName)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var value = TryGetAttributeValueAsString(xmlElement, attributeName);
      if (value == null)
        throw new Exception($"Could not find attribute named '{attributeName}'");
      return value;
    }

    public static string GetAttributeValueAsString(XElement xmlElement, string attributeName, string defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var value = TryGetAttributeValueAsString(xmlElement, attributeName);
      return value != null ? value : defaultValue;
    }


    public static string? TryGetAttributeValueAsString(XElement xmlElement, string attributeName)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var attr = xmlElement.Attribute(attributeName);
      return attr != null ? attr.Value : null;
    }

    public static string[] GetAttributeValueAsStringArray(XElement xmlElement, string attributeName)
    {
      var str = GetAttributeValueAsString(xmlElement, attributeName);
      var res = str.Split(',');
      for (int i = 0; i < res.Length; ++i)
        res[i] = res[i].Trim();
      return res;
    }


    public static bool GetAttributeValueAsBool(XElement xmlElement, string attributeName, bool defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      if (result == null)
        return defaultValue;
      if (result == "true")
        return true;
      if (result == "false")
        return false;
      throw new Exception($"Expected 'true' or 'false' not '{result}'");
    }

    public static UInt16 GetAttributeValueAsUInt16(XElement xmlElement, string attributeName, UInt16 defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? UInt16.Parse(result, g_invariantCulture) : defaultValue);
    }

    public static char GetAttributeValueAsChar(XElement xmlElement, string attributeName, char defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? ExtractCharStringLengthMustBeOne(result) : defaultValue);
    }

    private static char ExtractCharStringLengthMustBeOne(string str)
    {
      if (str.Length != 1)
        throw new ArgumentException($"string '{str}' did not contain exactly one char");
      return str[0];
    }

    public static bool TryGetAttributeValueAsUInt16(XElement xmlElement, string attributeName, out UInt16 rResult)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      rResult = result != null ? UInt16.Parse(result, g_invariantCulture) : (UInt16)0u;
      return result != null;
    }

    public static UInt32 GetAttributeValueAsUInt32(XElement xmlElement, string attributeName, UInt32 defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? UInt32.Parse(result, g_invariantCulture) : defaultValue);
    }

    public static bool TryGetAttributeValueAsUInt32(XElement xmlElement, string attributeName, out UInt32 rResult)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      string? result = TryGetAttributeValueAsString(xmlElement, attributeName);
      rResult = result != null ? UInt32.Parse(result, g_invariantCulture) : 0u;
      return result != null;
    }


    public static float GetAttributeValueAsFloat(XElement xmlElement, string attributeName, float defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? float.Parse(result, g_invariantCulture) : defaultValue);
    }

    public static bool TryGetAttributeValueAsFloat(XElement xmlElement, string attributeName, out float rResult)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      rResult = result != null ? float.Parse(result, g_invariantCulture) : 0u;
      return result != null;
    }


    public static PxSize2D GetAttributeValueAsPxSize2D(XElement xmlElement, string attributeName, PxSize2D defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsPxSize2D(result) : defaultValue);
    }

    public static PxThicknessU GetAttributeValueAsPxThicknessU(XElement xmlElement, string attributeName, PxThicknessU defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsPxThicknessU(result) : defaultValue);
    }


    public static OutputAtlasFormat GetAttributeValueAsOutputAtlasFormat(XElement xmlElement, string attributeName, OutputAtlasFormat defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsOutputAtlasFormat(result) : defaultValue);
    }

    public static BitmapFontType GetAttributeValueAsBitmapFontType(XElement xmlElement, string attributeName, BitmapFontType defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsBitmapFontType(result) : defaultValue);
    }

    public static TransparencyMode GetAttributeValueAsTransparencyMode(XElement xmlElement, string attributeName, TransparencyMode defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsTransparencyMode(result) : defaultValue);
    }

    public static bool TryGetAttributeValueAsTransparencyMode(XElement xmlElement, string attributeName, out TransparencyMode rResult)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      if (result == null)
      {
        rResult = TransparencyMode.Normal;
        return false;
      }
      rResult = StringParser.ParseAsTransparencyMode(result);
      return true;
    }

    public static PxPoint2[] GetAttributeValueAsPxPointArray(XElement xmlElement, string attributeName)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      if (result == null)
      {
        throw new Exception($"Anchor attribute '{nameof(attributeName)}' not found");
      }

      if (result.StartsWith("{", StringComparison.Ordinal))
      {
        // array of points
        var entries = result.Split('}');
        if (entries.Length <= 1)
          throw new NotSupportedException($"Invalid array ending '{entries[0]}'");
        var points = new PxPoint2[entries.Length - 1];
        for (int i = 0; i < entries.Length - 1; ++i)
        {
          var value = entries[i].Trim();
          if (value.StartsWith(','))
            value = value.Substring(1).Trim();
          if (value.StartsWith('{'))
            value = value.Substring(1).Trim();
          points[i] = StringParser.ParseAsPxPoint2(value);
        }
        if (entries[entries.Length - 1].Trim().Length > 0)
          throw new NotSupportedException($"Invalid array ending '{entries[entries.Length - 1]}'");
        return points;
      }
      else
      {
        // A single point
        return new PxPoint2[1] { StringParser.ParseAsPxPoint2(result) };
      }
    }


    public static ImmutableHashSet<string> GetAttributeValueAsStringHashSet(XElement xmlElement, string attributeName, char splitChar, ImmutableHashSet<string> defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      if (result == null)
      {
        return defaultValue;
      }

      var finalResult = new HashSet<string>();
      var entries = result.Split(splitChar);
      if (entries.Length > 0)
      {
        foreach (var entry in entries)
        {
          finalResult.Add(entry.Trim());
        }
      }
      return ImmutableHashSet.Create(finalResult.ToArray());
    }


    public static ImmutableHashSet<OutputFontFormat> GetAttributeValueAsOutputFontFormatHashSet(XElement xmlElement, string attributeName, OutputFontFormat defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var finalResult = new HashSet<OutputFontFormat>();
      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      if (result == null)
      {
        finalResult.Add(defaultValue);
      }
      else
      {
        var entries = result.Split(",");
        if (entries.Length <= 0)
        {
          finalResult.Add(defaultValue);
        }
        else
        {
          foreach (var entry in entries)
          {
            finalResult.Add(StringParser.ParseAsOutputFontFormat(entry));
          }
        }
      }
      return ImmutableHashSet.Create(finalResult.ToArray());
    }

    public static LicenseFormat GetAttributeValueAsLicenseFormat(XElement xmlElement, string attributeName, LicenseFormat defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsLicenseFormat(result) : defaultValue);
    }

    public static OutputFontFormat GetAttributeValueAsOutputFontFormat(XElement xmlElement, string attributeName, OutputFontFormat defaultValue)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }

      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsOutputFontFormat(result) : defaultValue);
    }

    public static TextureSizeRestriction GetAttributeValueAsTextureSizeRestriction(XElement xmlElement, string attributeName, TextureSizeRestriction defaultValue)
    {
      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsTextureSizeRestriction(result) : defaultValue);
    }

    public static ComplexPatchFlags GetAttributeValueAsComplexPatchFlags(XElement xmlElement, string attributeName, ComplexPatchFlags defaultValue)
    {
      var result = TryGetAttributeValueAsString(xmlElement, attributeName);
      return (result != null ? StringParser.ParseAsComplexPatchFlags(result) : defaultValue);
    }


    public static void ValidateNoChildren(XElement xmlElement)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }
      if (xmlElement.HasElements)
      {
        throw new Exception($"Element '{xmlElement}' can not contain any children");
      }
    }


    public static void ValidateAttributes(XElement xmlElement, string[] validAttributes, string? ignoreNamespaceName = null)
    {
      if (xmlElement == null)
      {
        throw new ArgumentNullException(nameof(xmlElement));
      }
      if (validAttributes == null)
      {
        throw new ArgumentNullException(nameof(validAttributes));
      }

      bool ignoreNamespace = ignoreNamespaceName != null;
      foreach (var entry in xmlElement.Attributes())
      {
        if (!Contains(validAttributes, entry.Name) && (!ignoreNamespace || entry.Name.NamespaceName != ignoreNamespaceName))
        {
          throw new Exception($"Element {xmlElement.Name} has unknown attribute: {entry.Name}. Valid attributes are: '{string.Join(", ", validAttributes)}'");
        }
      }
    }

    private static bool Contains(string[] validAttributes, XName name)
    {
      for (int i = 0; i < validAttributes.Length; ++i)
      {
        if (validAttributes[i] == name)
          return true;
      }
      return false;
    }
  }
}
