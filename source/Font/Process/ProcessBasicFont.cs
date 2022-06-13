/****************************************************************************************************************************************************
 * Copyright 2021 NXP
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
using MB.Base.Exceptions;
using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Immutable;
using TexturePacker.Commands.Atlas;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.Process
{
  public readonly struct TraceInfo
  {
    public readonly string Command;
    public readonly string Source;
    public TraceInfo(string command, string source)
    {
      Command = command;
      Source = source;
    }

    public string Description => $"{Command} on {Source}";

  }

  public sealed partial class ProcessUtil
  {
    public static BitmapFont Tweak(BitmapFont src, in BitmapFontTweakConfig tweakConfig, TraceInfo traceInfo)
    {
      if (tweakConfig.BaseLinePx == 0 && tweakConfig.MeasureCharId == 0 && tweakConfig.SdfConfig == null)
        return src;

      UInt16 newLineSpacingPx = src.LineSpacingPx;
      UInt16 newBaseLinePx = src.BaseLinePx;
      UInt16 sdfSpread = src.SdfSpread;
      UInt16 sdfDesiredBaseLinePx = src.SdfDesiredBaseLinePx;

      if (tweakConfig.SdfConfig != null)
      {
        sdfSpread = tweakConfig.SdfConfig.Spread;
        sdfDesiredBaseLinePx = tweakConfig.SdfConfig.DesiredBaseLinePx;
      }

      // tweak the baseline
      Int32 yAdjustPx = 0;
      if (tweakConfig.BaseLinePx != 0)
      {
        newBaseLinePx = tweakConfig.BaseLinePx;
        yAdjustPx = tweakConfig.BaseLinePx - src.BaseLinePx;
        newLineSpacingPx = NumericCast.ToUInt16(newLineSpacingPx + yAdjustPx);
      }

      // tweak the linespacing
      if (tweakConfig.LineSpacingPx != 0)
      {
        newLineSpacingPx = tweakConfig.LineSpacingPx;
      }

      // Apply forced measurements
      if (tweakConfig.MeasureCharId != 0)
      {
        BitmapFontChar measureRecord = FindMeasureChar(src.Chars, tweakConfig.MeasureCharId);
        if (measureRecord.SrcTextureRectPx.Height != tweakConfig.MeasureHeightPx)
        {
          throw new Exception($"The measure char '{Convert.ToChar(tweakConfig.MeasureCharId)}' ({tweakConfig.MeasureCharId}) was {measureRecord.SrcTextureRectPx.Height}px and not {tweakConfig.MeasureHeightPx}px as required ({traceInfo.Description})");
        }
      }

      BitmapFontChar[] chars = Proces(src.Chars, yAdjustPx);

      return new BitmapFont(src.Name, src.Dpi, src.Size, newLineSpacingPx, newBaseLinePx, src.PaddingPx, src.TextureName, src.FontType,
                            sdfSpread, sdfDesiredBaseLinePx, chars, src.Kernings);
    }

    private static BitmapFontChar FindMeasureChar(ImmutableArray<BitmapFontChar> src, UInt32 findCharId)
    {
      for (int i = 0; i < src.Length; ++i)
      {
        if (src[i].Id == findCharId)
          return src[i];
      }
      throw new NotFoundException($"No record found for the measure char '{Convert.ToChar(findCharId)}' ({findCharId})");
    }

    private static BitmapFontChar[] Proces(ImmutableArray<BitmapFontChar> src, Int32 yAdjustPx)
    {
      var res = new BitmapFontChar[src.Length];

      PxPoint2 adjustPx = new PxPoint2(0, yAdjustPx);
      for (int i = 0; i < res.Length; ++i)
      {
        var record = src[i];
        PxPoint2 offsetPx = record.OffsetPx + adjustPx;
        res[i] = new BitmapFontChar(record.Id, record.SrcTextureRectPx, offsetPx, record.XAdvancePx);
      }
      return res;
    }
  }
}

//****************************************************************************************************************************************************

