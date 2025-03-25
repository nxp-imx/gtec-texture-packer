#nullable enable
/****************************************************************************************************************************************************
 * Copyright 2025 NXP
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
using FslGraphics.Font.MsdfAtlas;
using MB.Base;
using MB.Base.MathEx;
using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TexturePacker.Commands.Atlas;

//----------------------------------------------------------------------------------------------------------------------------------------------------


namespace FslGraphics.Font.Converter
{
  /// <summary>
  /// BitmapFont to BasicFont
  /// </summary>
  public sealed partial class TypeConverter
  {
    private enum InternalFontType
    {
      SoftMask = 0,
      SDF = 1,
      MSDF = 2,
      MTSDF = 3
    }

    private enum InternalFontOrigin
    {
      Top = 0,
      Bottom = 1
    }

    private sealed class PixelConverter
    {
      public readonly InternalFontType FontType;
      private readonly InternalFontOrigin m_fontOrigin;
      private readonly double m_emSize;
      private readonly PxSize1DD m_preciseFontSizePxf;

      public readonly PxSize1D FontSizePx;
      public readonly PxSize2D TextureSizePx;
      public readonly PxSize1D LineHeightPx;
      public readonly PxSize1D BaseLinePx;

      public readonly float DistanceRange;

      public PixelConverter(Root font)
      {
        FontType = ParseFontType(font.Atlas.Type);
        m_fontOrigin = ParseOrigin(font.Atlas.YOrigin);
        m_emSize = font.Metrics.EmSize;
        m_preciseFontSizePxf = PxSize1DD.Create(font.Atlas.Size);

        FontSizePx = PxSize1D.Create(NumericCast.ToInt32(m_preciseFontSizePxf.RawValue));
        TextureSizePx = new PxSize2D(font.Atlas.Width, font.Atlas.Height);

        LineHeightPx = EmToPxSize1D(m_preciseFontSizePxf, font.Metrics.LineHeight, m_emSize);

        double ascender = m_fontOrigin == InternalFontOrigin.Bottom ? font.Metrics.Ascender : -font.Metrics.Ascender;
        BaseLinePx = EmToPxSize1D(m_preciseFontSizePxf, ascender, m_emSize);
        DistanceRange = (float)font.Atlas.DistanceRange;
      }

      public PxRectangle AtlasBoundsToPxRectangle(Bounds value)
      {
        PxValue leftPx = ToTextureXPositionPxValue(value.Left);
        PxValue topPx = ToTextureYPositionPxValue(value.Top);
        PxValue rightPx = ToTextureXPositionPxValue(value.Right);
        PxValue bottomPx = ToTextureYPositionPxValue(value.Bottom);
        return PxRectangle.FromLeftTopRightBottom(leftPx.Value, topPx.Value, rightPx.Value, bottomPx.Value);
      }

      public PxValue ToTextureXPositionPxValue(double value)
      {
        return PixelsToPxValue(value);
      }


      /// <summary>
      /// Help convert a texture y-position to pixel based value
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      /// <exception cref="NotSupportedException"></exception>
      public PxValue ToTextureYPositionPxValue(double value)
      {
        var valuePx = PixelsToPxValue(value);
        switch (m_fontOrigin)
        {
          case InternalFontOrigin.Top:
            return valuePx;
          case InternalFontOrigin.Bottom:
            return new PxValue(TextureSizePx.Height - 1) - valuePx;
        }
        throw new NotSupportedException($"Unsupported origin: {m_fontOrigin}");
      }

      private PxValue PixelsToPxValue(double value)
      {
        //return new PxValue(MathUtil.RoundToInt32(value));
        return new PxValue((Int32)Math.Floor(value));
      }

      public PxSize1D EmToPxSize1D(double value)
      {
        return EmToPxSize1D(m_preciseFontSizePxf, value, m_emSize);
      }

      public PxValue EmToPxValue(double value)
      {
        return EmToPxValue(m_preciseFontSizePxf, value, m_emSize);
      }

      public PxPoint2 CalcOffsetPx(Bounds value)
      {
        var leftPx = EmToPxValue(value.Left);
        var topPx = EmToPxValue(value.Top);
        var rightPx = EmToPxValue(value.Right);
        var bottomPx = EmToPxValue(value.Bottom);
        switch (m_fontOrigin)
        {
          case InternalFontOrigin.Top:
            return new PxPoint2(leftPx.Value, BaseLinePx.RawValue + topPx.Value);
          case InternalFontOrigin.Bottom:
            return new PxPoint2(leftPx.Value, BaseLinePx.RawValue - topPx.Value);
        }
        throw new NotSupportedException($"Unsupported origin: {m_fontOrigin}");
      }


      private static PxSize1D EmToPxSize1D(PxSize1DD fontSizePxf, double value, double emSize)
      {
        return PxSize1D.Create(MathUtil.RoundToInt32(fontSizePxf.RawValue * value * emSize));
      }

      private static PxValue EmToPxValue(PxSize1DD fontSizePxf, double value, double emSize)
      {
        return new PxValue(MathUtil.RoundToInt32(fontSizePxf.RawValue * value * emSize));
      }
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
    public static BitmapFont ToBitmapFont(Root font, BitmapFontType fontType, BitmapFontSdfConfig? sdfConfig, UInt16 dpi, string fontName)
    {
      if (font == null)
        throw new ArgumentNullException(nameof(font));

      PixelConverter converter = new PixelConverter(font);
      var foundFontType = ToBitmapFontType(converter.FontType);
      if (fontType != foundFontType)
      {
        throw new NotSupportedException($"The expected font type {fontType} did not match, the found font type {foundFontType}");
      }

      ImmutableArray<BitmapFontChar> chars = ToBitmapFontChars(converter, font.Glyphs);
      ImmutableArray<BitmapFontKerning> kernings = ToBitmapFontKerning(converter, font.Kerning);

      UInt16 size = NumericCast.ToUInt16(converter.FontSizePx.RawValue);
      UInt16 lineHeightPx = NumericCast.ToUInt16(converter.LineHeightPx.RawValue);
      UInt16 baseLinePx = NumericCast.ToUInt16(converter.BaseLinePx.RawValue);

      PxThicknessU16 paddingPx = new PxThicknessU16(); // Empty as we dont have that information

      float sdfDistanceRange = Math.Max(converter.DistanceRange, 0.0f);
      UInt16 sdfDesiredBaseLinePx = 0;
      if (sdfConfig != null)
      {
        if(sdfConfig.DistanceRange > 0.0f)
        {
          // Override the distance range supplied by the font
          sdfDistanceRange = sdfConfig.DistanceRange;
        }
        sdfDesiredBaseLinePx = sdfConfig.DesiredBaseLinePx;
      }
      return new BitmapFont(fontName, dpi, size, lineHeightPx, baseLinePx, paddingPx, $"{fontName}.png", fontType, sdfDistanceRange,
                            sdfDesiredBaseLinePx, chars, kernings);
    }


    private static ImmutableArray<BitmapFontChar> ToBitmapFontChars(PixelConverter converter, List<Glyph> glyphs)
    {
      int charCount = glyphs.Count;
      if (charCount <= 0)
        return ImmutableArray<BitmapFontChar>.Empty;

      var builder = ImmutableArray.CreateBuilder<BitmapFontChar>(charCount);
      foreach (Glyph entry in glyphs)
      {
        PxRectangle srcTextureRectPx = converter.AtlasBoundsToPxRectangle(entry.AtlasBounds);
        PxPoint2 offsetPx = converter.CalcOffsetPx(entry.PlaneBounds);
        UInt16 xAdvancePx = NumericCast.ToUInt16(converter.EmToPxValue(entry.Advance).Value);
        builder.Add(new BitmapFontChar(NumericCast.ToUInt32(entry.Unicode), srcTextureRectPx, offsetPx, xAdvancePx));
      }
      return builder.MoveToImmutable();
    }


    private static ImmutableArray<BitmapFontKerning> ToBitmapFontKerning(PixelConverter converter, List<Kerning> kerning)
    {
      int kerningCount = kerning.Count;
      if (kerningCount <= 0)
        return ImmutableArray<BitmapFontKerning>.Empty;

      var builder = ImmutableArray.CreateBuilder<BitmapFontKerning>(kerningCount);
      foreach (Kerning entry in kerning)
      {
        UInt32 first = NumericCast.ToUInt32(entry.Unicode1);
        UInt32 second = NumericCast.ToUInt32(entry.Unicode2);
        Int32 amountPx = converter.EmToPxValue(entry.Advance).Value;
        builder.Add(new BitmapFontKerning(first, second, amountPx));
      }
      return builder.MoveToImmutable();
    }


    private static BitmapFontType ToBitmapFontType(InternalFontType value)
    {
      switch (value)
      {
        case InternalFontType.SoftMask:
          return BitmapFontType.Bitmap;
        case InternalFontType.SDF:
          return BitmapFontType.SDF;
        case InternalFontType.MSDF:
          return BitmapFontType.MSDF;
        case InternalFontType.MTSDF:
          return BitmapFontType.MTSDF;
      }
      throw new NotSupportedException($"font type: {value}");
    }


    private static InternalFontType ParseFontType(string value)
    {
      switch (value)
      {
        case "softmask":
          return InternalFontType.SoftMask;
        case "sdf":
          return InternalFontType.SDF;
        case "msdf":
          return InternalFontType.MSDF;
        case "mtsdf":
          return InternalFontType.MTSDF;
      }
      throw new NotSupportedException($"Unknown type: {value}");
    }


    private static InternalFontOrigin ParseOrigin(string value)
    {
      switch (value)
      {
        case "top":
          return InternalFontOrigin.Top;
        case "bottom":
          return InternalFontOrigin.Bottom;
      }
      throw new NotSupportedException($"Unknown origin: {value}");
    }
  }
}

//****************************************************************************************************************************************************

