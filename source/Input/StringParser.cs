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
using MB.Base;
using MB.Base.MathEx.Pixel;
using MB.Graphics2.Patch.Advanced;
using MB.RectangleBinPack.TexturePack;
using NLog;
using System;
using System.Globalization;

namespace TexturePacker.Input
{
  static class StringParser
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public static PxSize2D ParseAsPxSize2D(string value)
    {
      var inputArray = value.Split(",");
      if (inputArray.Length != 2)
        throw new Exception($"'{value}' did not contain two values as expected 'val0, val1'");

      return new PxSize2D(int.Parse(inputArray[0], CultureInfo.InvariantCulture), int.Parse(inputArray[1], CultureInfo.InvariantCulture));
    }

    public static PxPoint2 ParseAsPxPoint2(string value)
    {
      var inputArray = value.Split(",");
      if (inputArray.Length != 2)
        throw new Exception($"'{value}' did not contain two values as expected 'val0, val1'");

      return new PxPoint2(int.Parse(inputArray[0], CultureInfo.InvariantCulture), int.Parse(inputArray[1], CultureInfo.InvariantCulture));
    }

    public static PxThicknessU ParseAsPxThicknessU(string value)
    {
      var inputArray = value.Split(",");
      if (inputArray.Length != 4)
        throw new Exception($"'{value}' did not contain four values as expected 'val0, val1, val2, val3'");

      return new PxThicknessU(UInt32.Parse(inputArray[0], CultureInfo.InvariantCulture), UInt32.Parse(inputArray[1], CultureInfo.InvariantCulture),
                              UInt32.Parse(inputArray[2], CultureInfo.InvariantCulture), UInt32.Parse(inputArray[3], CultureInfo.InvariantCulture));
    }

    public static OutputAtlasFormat ParseAsOutputAtlasFormat(string value)
    {
      if (value == "bta3")
      {
        return OutputAtlasFormat.BTA3;
      }
      if (value == "bta4")
      {
        return OutputAtlasFormat.BTA4;
      }
      if (value == "bta4C#")
      {
        return OutputAtlasFormat.BTA4CSharp;
      }
      throw new NotSupportedException($"Unsupported OutputAtlasFormat '{value}', allowed values: {{'bta3', 'bta4', 'bta4C#'}}");
    }

    public static BitmapFontType ParseAsBitmapFontType(string value)
    {
      if (value == "bitmap")
      {
        return BitmapFontType.Bitmap;
      }
      if (value == "sdf")
      {
        return BitmapFontType.SDF;
      }
      if (value == "msdf")
      {
        return BitmapFontType.MSDF;
      }
      if (value == "mtsdf")
      {
        return BitmapFontType.MTSDF;
      }
      throw new NotSupportedException($"Unsupported BitmapFontType '{value}', allowed values: {{'bitmap', 'sdf', 'msdf', 'mtsdf'}}");
    }

    public static TransparencyMode ParseAsTransparencyMode(string value)
    {
      if (value == "normal")
      {
        return TransparencyMode.Normal;
      }
      if (value == "premultiply")
      {
        return TransparencyMode.Premultiply;
      }
      if (value == "premultiply-linear")
      {
        return TransparencyMode.PremultiplyUsingLinearColors;
      }
      throw new NotSupportedException($"Unsupported TransparencyMode '{value}', allowed values: {{'normal', 'premultiply', 'premultiply-linear'}}");
    }

    public static LicenseFormat ParseAsLicenseFormat(string value)
    {
      if (value == "unknown")
      {
        return LicenseFormat.Unknown;
      }
      if (value == "nxpJson")
      {
        return LicenseFormat.NxpJson;
      }
      throw new NotSupportedException($"Unsupported LicenseFormat '{value}', allowed values: {{'unknown','nxpJson'}}");
    }

    public static OutputFontFormat ParseAsOutputFontFormat(string value)
    {
      if (value == "fbk")
      {
        return OutputFontFormat.FBK;
      }
      if (value == "nbf")
      {
        return OutputFontFormat.NBF;
      }
      if (value == "nbfC#")
      {
        return OutputFontFormat.NBFCSharp;
      }
      if (value == "jsonBitmapFont")
      {
        return OutputFontFormat.JsonBitmapFont;
      }
      throw new NotSupportedException($"Unsupported OutputFontFormat '{value}', allowed values: {{'fbk', 'nbf', 'nbfC#', 'jsonBitmapFont'}}");
    }


    public static TextureSizeRestriction ParseAsTextureSizeRestriction(string value)
    {
      if (value == "any")
      {
        return TextureSizeRestriction.Any;
      }
      else if (value == "pow2")
      {
        return TextureSizeRestriction.Pow2;
      }
      else if (value == "pow2square")
      {
        return TextureSizeRestriction.Pow2Square;
      }
      throw new NotSupportedException($"Unsupported TextureSizeRestriction '{value}', allowed values: {{'any', 'pow2', 'pow2square'}}");
    }

    public static ComplexPatchFlags ParseAsComplexPatchFlags(string value)
    {
      // split then parse each individual flag
      var inputArray = value.Split('|');

      var result = ComplexPatchFlags.None;
      for (int i = 0; i < inputArray.Length; ++i)
      {
        result |= ParseAsComplexPatchFlag(inputArray[i].AsSpan().Trim());
      }
      return result;
    }

    private static ComplexPatchFlags ParseAsComplexPatchFlag(ReadOnlySpan<char> value)
    {
      if (MemoryExtensions.Equals(value, "None", StringComparison.Ordinal))
        return ComplexPatchFlags.None;
      if (MemoryExtensions.Equals(value, "MirrorX", StringComparison.Ordinal))
        return ComplexPatchFlags.MirrorX;
      if (MemoryExtensions.Equals(value, "MirrorY", StringComparison.Ordinal))
        return ComplexPatchFlags.MirrorY;
      throw new NotSupportedException($"Unsupported ComplexPatchFlags '{value.ToString()}', allowed values: {{'None', 'MirrorX', 'MirrorY'}}");
    }

    public static ImmutableComplexPatchSlice[] ParseAsImmutableComplexPatchSliceArray(string[] slices)
    {
      if (slices == null)
        throw new ArgumentNullException(nameof(slices));
      if (slices.Length < 2)
        throw new ArgumentException($"A slice array must contain at least two entries. {string.Join(',', slices)}");

      int previousPosition = -1;
      var result = new ImmutableComplexPatchSlice[slices.Length];
      for (int i = 0; i < slices.Length; ++i)
      {
        result[i] = ParseAsImmutableComplexPatchSlice(slices[i].AsSpan());
        if (result[i].Position < previousPosition)
        {
          throw new Exception($"A slice position must be larger than the previous one. Previous:{previousPosition} Current: {result[i].Position} . {string.Join(',', slices)}");
        }
        previousPosition = result[i].Position;
      }

      if (result[0].Position != 0)
        throw new ArgumentException($"A slice array must start at zero. {string.Join(',', slices)}");
      if (result[result.Length - 1].Flags != ComplexPatchSliceFlags.None)
        throw new ArgumentException($"A slice arrays last entry can not contain any flags. {string.Join(',', slices)}");

      return result;
    }

    public static ImmutableContentSlice[] ParseAsImmutableContentSliceArray(string[] slices)
    {
      if (slices == null)
        throw new ArgumentNullException(nameof(slices));
      if (slices.Length < 2)
        throw new ArgumentException($"A slice array must contain at least two entries. {string.Join(',', slices)}");

      int previousPosition = -1;
      var result = new ImmutableContentSlice[slices.Length];
      for (int i = 0; i < slices.Length; ++i)
      {
        result[i] = ParseAsImmutableContentSlice(slices[i].AsSpan());
        if (result[i].Position < previousPosition)
        {
          throw new Exception($"A slice position must be larger than the previous one. Previous:{previousPosition} Current: {result[i].Position} . {string.Join(',', slices)}");
        }
        previousPosition = result[i].Position;
      }

      if (result[0].Position != 0)
        throw new ArgumentException($"A slice array must start at zero. {string.Join(',', slices)}");
      if (result[result.Length - 1].Flags != ContentSliceFlags.None)
        throw new ArgumentException($"A slice arrays last entry can not contain any flags. {string.Join(',', slices)}");

      return result;
    }


    public static ImmutableComplexPatchSlice ParseAsImmutableComplexPatchSlice(ReadOnlySpan<char> value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof(value));

      ComplexPatchSliceFlags flags = ComplexPatchSliceFlags.None;
      if (value.EndsWith(")"))
      {
        int index = value.LastIndexOf('(');
        if (index < 0)
          throw new Exception($"Invalid slice format, found ending ')' but no initial '(' ");

        flags = ParseAsImmutableComplexPatchSliceFlags(value.Slice(index + 1, value.Length - index - 2));

        value = value.Slice(0, index);
      }
      Int32 position = Int32.Parse(value);
      if (position < 0)
        throw new Exception("Slice value can not be negative");
      if (position > UInt16.MaxValue)
        throw new Exception($"Slice value can not be larger than {UInt16.MaxValue}");

      return new ImmutableComplexPatchSlice(UncheckedNumericCast.ToUInt16(position), flags);
    }

    public static ImmutableContentSlice ParseAsImmutableContentSlice(ReadOnlySpan<char> value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof(value));

      ContentSliceFlags flags = ContentSliceFlags.None;

      if (value.EndsWith(")"))
      {
        int index = value.LastIndexOf('(');
        if (index < 0)
          throw new Exception($"Invalid slice format, found ending ')' but no initial '(' ");

        flags = ParseAsImmutableContentSliceFlags(value.Slice(index + 1, value.Length - index - 2));

        value = value.Slice(0, index);
      }

      Int32 position = Int32.Parse(value);
      if (position < 0)
        throw new Exception("Slice value can not be negative");
      if (position > UInt16.MaxValue)
        throw new Exception($"Slice value can not be larger than {UInt16.MaxValue}");
      return new ImmutableContentSlice(UncheckedNumericCast.ToUInt16(position), flags);
    }

    public static ComplexPatchSliceFlags ParseAsImmutableComplexPatchSliceFlags(ReadOnlySpan<char> srcValue)
    {
      ComplexPatchSliceFlags flags = ComplexPatchSliceFlags.None;

      var value = srcValue;
      while (value.Length > 0)
      {
        int index = value.IndexOf('|');

        var subSlice = value.Slice(0, index >= 0 ? index : value.Length);

        ComplexPatchSliceFlags newFlag = ComplexPatchSliceFlags.None;

        if (MemoryExtensions.Equals(subSlice, "s", StringComparison.Ordinal))
        {
          newFlag = ComplexPatchSliceFlags.Scale;
        }
        else if (MemoryExtensions.Equals(subSlice, "t", StringComparison.Ordinal))
        {
          newFlag = ComplexPatchSliceFlags.Transparent;
        }
        else if (MemoryExtensions.Equals(subSlice, "g0", StringComparison.Ordinal))
        {
          newFlag = ComplexPatchSliceFlags.Group0;
        }
        else if (MemoryExtensions.Equals(subSlice, "g1", StringComparison.Ordinal))
        {
          newFlag = ComplexPatchSliceFlags.Group1;
        }
        else if (MemoryExtensions.Equals(subSlice, "g2", StringComparison.Ordinal))
        {
          newFlag = ComplexPatchSliceFlags.Group2;
        }
        else if (MemoryExtensions.Equals(subSlice, "g3", StringComparison.Ordinal))
        {
          newFlag = ComplexPatchSliceFlags.Group3;
        }
        else
          throw new Exception($"Unknown flag '{subSlice.ToString()}' found in '{srcValue.ToString()}'");

        if (flags.IsFlagged(newFlag))
          g_logger.Warn($"Flag '{subSlice.ToString()}' set multiple times in '{srcValue.ToString()}'");

        flags |= newFlag;

        value = value.Slice(index >= 0 ? index + 1 : value.Length);
      }
      return flags;
    }

    public static ContentSliceFlags ParseAsImmutableContentSliceFlags(ReadOnlySpan<char> srcValue)
    {
      ContentSliceFlags flags = ContentSliceFlags.None;

      var value = srcValue;
      while (value.Length > 0)
      {
        int index = value.IndexOf('|');

        var subSlice = value.Slice(0, index >= 0 ? index : value.Length);
        ContentSliceFlags newFlag = ContentSliceFlags.None;
        if (MemoryExtensions.Equals(subSlice, "c", StringComparison.Ordinal))
        {
          newFlag = ContentSliceFlags.Content;
        }
        else
          throw new Exception($"Unknown flag '{subSlice.ToString()}' found in '{srcValue.ToString()}'");

        if (flags.IsFlagged(newFlag))
          g_logger.Warn($"Flag '{subSlice.ToString()}' set multiple times in '{srcValue.ToString()}'");

        flags |= newFlag;

        value = value.Slice(index >= 0 ? index + 1 : value.Length);
      }
      return flags;
    }
  }
}
