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

using FslGraphics.Font.AngleCode;
using FslGraphics.Font.BF;
using MB.Base;
using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TexturePacker.Commands.Atlas;

//----------------------------------------------------------------------------------------------------------------------------------------------------


namespace FslGraphics.Font.Converter
{
  public sealed partial class TypeConverter
  {
    #region AngleCodeFont to BitmapFont

    public static BitmapFont ToBitmapFont(AngleCodeFont font, BitmapFontType fontType, BitmapFontSdfConfig sdfConfig, UInt16 dpi)
    {
      if (font == null)
        throw new ArgumentNullException(nameof(font));

      if (font.Pages.Count != 1)
        throw new Exception($"BitmapFont only support one page, not {font.Pages.Count} pages");
      if (font.Common.Packed)
        throw new Exception("BitmapFont does not support font packing (common.Packed=true)");

      var chars = ToBitmapFontChars(font.Chars);
      var kernings = ToBitmapFontKernings(font.Kernings);

      UInt16 size = NumericCast.ToUInt16(font.Info.Size);
      UInt16 lineHeightPx = NumericCast.ToUInt16(font.Common.LineHeightPx);
      UInt16 baseLinePx = NumericCast.ToUInt16(font.Common.BaseLinePx);
      PxThicknessU16 paddingPx = PxTypeConverter.ToPxThicknessU16(font.Info.PaddingPx);

      UInt16 sdfSpread = 0;
      UInt16 sdfDesiredBaseLinePx = 0;
      if (sdfConfig != null)
      {
        sdfSpread = sdfConfig.Spread;
        sdfDesiredBaseLinePx = sdfConfig.DesiredBaseLinePx;
      }

      return new BitmapFont(font.Info.Face, dpi, size, lineHeightPx, baseLinePx, paddingPx, font.Pages[0].File, fontType, sdfSpread,
                            sdfDesiredBaseLinePx, chars, kernings);
    }

    private static BitmapFontChar[] ToBitmapFontChars(List<FontChar> entries)
    {
      Debug.Assert(entries != null);

      var result = new BitmapFontChar[entries.Count];
      for (int i = 0; i < result.Length; ++i)
      {
        var entry = entries[i];
        result[i] = new BitmapFontChar(UncheckedNumericCast.ToUInt32(entry.Id), entry.SrcTextureRectPx, entry.OffsetPx,
                                       UncheckedNumericCast.ToUInt16(entry.XAdvancePx));
      }
      return result;
    }

    private static BitmapFontKerning[] ToBitmapFontKernings(List<FontKerning> entries)
    {
      var result = new BitmapFontKerning[entries.Count];
      for (int i = 0; i < result.Length; ++i)
      {
        var entry = entries[i];
        result[i] = new BitmapFontKerning(entry.First, entry.Second, entry.AmountPx);
      }
      return result;
    }

    #endregion
  }
}

//****************************************************************************************************************************************************
