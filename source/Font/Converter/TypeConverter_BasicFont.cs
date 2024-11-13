#nullable enable
/****************************************************************************************************************************************************
 * Copyright 2020,2024 NXP
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

using FslGraphics.Font.Basic;
using FslGraphics.Font.BF;
using MB.Base;
using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using TexturePacker.Commands.Atlas;

//----------------------------------------------------------------------------------------------------------------------------------------------------


namespace FslGraphics.Font.Converter
{
  /// <summary>
  /// BitmapFont to BasicFont
  /// </summary>
  public sealed partial class TypeConverter
  {
    private enum BitmapFontExtractRangeState
    {
      FindBegin = 1,
      FindEnd = 2
    }

    /// <summary>
    /// WARNING: the BitmapFontChar.SrcTextureRectPx needs to be patched as its set to empty
    /// </summary>
    /// <param name="font"></param>
    /// <param name="fontType"></param>
    /// <param name="sdfConfig"></param>
    /// <param name="dpi"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static BitmapFont ToBitmapFont(BasicFont font, BitmapFontType fontType, BitmapFontSdfConfig? sdfConfig, UInt16 dpi)
    {
      if (font == null)
        throw new ArgumentNullException(nameof(font));

      ImmutableArray<BitmapFontChar> chars = ToBitmapFontChars(font);
      // There is no advanced kerning in this font type!
      var kernings = ImmutableArray<BitmapFontKerning>.Empty;

      UInt16 size = NumericCast.ToUInt16(font.Header.LineSpacingPx); // Use linespacing as we dont have the size
      UInt16 lineHeightPx = NumericCast.ToUInt16(font.Header.LineSpacingPx);
      UInt16 baseLinePx = NumericCast.ToUInt16(font.Header.BaseLinePx);
      PxThicknessU16 paddingPx = new PxThicknessU16(); // Empty as we dont have that information

      UInt16 sdfSpread = 0;
      UInt16 sdfDesiredBaseLinePx = 0;
      if (sdfConfig != null)
      {
        sdfSpread = sdfConfig.Spread;
        sdfDesiredBaseLinePx = sdfConfig.DesiredBaseLinePx;
      }

      return new BitmapFont(font.Name, dpi, size, lineHeightPx, baseLinePx, paddingPx, "Virtual", fontType, sdfSpread,
                            sdfDesiredBaseLinePx, chars, kernings);
    }


    private static ImmutableArray<BitmapFontChar> ToBitmapFontChars(BasicFont font)
    {
      int charCount = font.CountChars();
      if (charCount <= 0)
        return ImmutableArray<BitmapFontChar>.Empty;

      int glyphKerningIndex = 0;

      var builder = ImmutableArray.CreateBuilder<BitmapFontChar>(charCount);
      foreach (var range in font.Ranges)
      {
        int glyphEndIndex = range.From + range.Length;
        for (int glyphIndex = range.From; glyphIndex < glyphEndIndex; ++glyphIndex)
        {
          UInt32 id = NumericCast.ToUInt32(glyphIndex);
          PxRectangle srcTextureRectPx = PxRectangle.Empty; // not available in a basic font (We need to load the bitmap image to get that)
          PxPoint2 offsetPx = font.Kerning[glyphKerningIndex].OffsetPx;
          UInt16 xAdvancePx = NumericCast.ToUInt16(font.Kerning[glyphKerningIndex].LayoutWidthPx);
          builder.Add(new BitmapFontChar(id, srcTextureRectPx, offsetPx, xAdvancePx));
          ++glyphKerningIndex;
        }
      }
      return builder.MoveToImmutable();
    }

    public static BasicFont ToBasicFont(BitmapFont font, ImmutableArray<PxThickness> trimInfo, string atlasFontFolder)
    {
      if (font == null)
        throw new ArgumentNullException(nameof(font));
      if (font.Chars.Length != trimInfo.Length)
        throw new ArgumentException("trimInfo must contain a entry per character in the font");

      var name = font.Name;
      var header = ExtractBasicFontHeader(font, atlasFontFolder);
      var fontGlyphRanges = ExtractBasicFontRanges(font);
      var fontGlyphKernings = ExtractBasicFontKernings(font, trimInfo, fontGlyphRanges);
      return new BasicFont(name, header, fontGlyphRanges.ToArray(), fontGlyphKernings.ToArray());
    }

    private static BasicFontHeader ExtractBasicFontHeader(BitmapFont font, string atlasFontFolder)
    {
      return new BasicFontHeader(atlasFontFolder, font.LineSpacingPx, font.BaseLinePx, new PxSize2D(0, 0));
    }

    private static bool IsInRange(List<BasicFontGlyphRange> validRanges, int id)
    {
      foreach (var range in validRanges)
      {
        if (id >= range.From && id < (range.From + range.Length))
          return true;
      }
      return false;
    }

    private static List<BasicFontGlyphKerning> ExtractBasicFontKernings(BitmapFont font, ImmutableArray<PxThickness> trimInfo,
                                                                        List<BasicFontGlyphRange> validRanges)
    {
      Debug.Assert(font.Chars.Length == trimInfo.Length);

      var result = new List<BasicFontGlyphKerning>();
      for (int i = 0; i < font.Chars.Length; ++i)
      {
        var fontChar = font.Chars[i];
        var trimOffset = new PxPoint2(trimInfo[i].Left, trimInfo[i].Top);
        var id = UncheckedNumericCast.ToInt32(fontChar.Id);
        if (IsInRange(validRanges, id))
          result.Add(new BasicFontGlyphKerning(id, fontChar.OffsetPx - trimOffset, fontChar.XAdvancePx));
      }
      return result;
    }


    private static List<BasicFontGlyphRange> ExtractBasicFontRanges(BitmapFont font)
    {
      var result = new List<BasicFontGlyphRange>();
      var state = BitmapFontExtractRangeState.FindBegin;
      int rangeStartId = 0;
      int expectedRangeId = 0;

      int index = 0;
      while (index < font.Chars.Length)
      {
        var fontChar = font.Chars[index];
        switch (state)
        {
          case BitmapFontExtractRangeState.FindBegin:
            state = BitmapFontExtractRangeState.FindEnd;
            rangeStartId = UncheckedNumericCast.ToInt32(fontChar.Id);
            expectedRangeId = rangeStartId + 1;
            break;
          case BitmapFontExtractRangeState.FindEnd:
            {
              if (fontChar.Id != expectedRangeId)
              {
                state = BitmapFontExtractRangeState.FindBegin;
                AddBasicFontRange(result, new BasicFontGlyphRange(rangeStartId, expectedRangeId - rangeStartId, rangeStartId));
                --index;

              }
              else
              {
                ++expectedRangeId;
              }
            }
            break;
          default:
            break;
        }
        ++index;
      }
      if (state == BitmapFontExtractRangeState.FindEnd)
      {
        AddBasicFontRange(result, new BasicFontGlyphRange(rangeStartId, expectedRangeId - rangeStartId, rangeStartId));
      }
      return result;
    }

    private static void AddBasicFontRange(List<BasicFontGlyphRange> dstList, BasicFontGlyphRange newEntry)
    {
      if (newEntry.From < 32 && (newEntry.From + newEntry.Length) <= 32)
      {
        return;
      }
      dstList.Add(newEntry);
    }
  }
}

//****************************************************************************************************************************************************

