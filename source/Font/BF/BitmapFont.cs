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

using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Immutable;

//----------------------------------------------------------------------------------------------------------------------------------------------------


namespace FslGraphics.Font.BF
{
  public sealed class BitmapFont
  {
    /// <summary>
    /// This is the name of the true type font
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The dpi of the font.
    /// </summary>
    public readonly UInt16 Dpi;

    /// <summary>
    /// The size of the true type font.
    /// </summary>
    public readonly UInt16 Size;

    /// <summary>
    /// This is the distance in pixels between each line of text.
    /// </summary>
    public readonly UInt16 LineSpacingPx;

    /// <summary>
    /// The number of pixels from the absolute top of the line to the base of the characters.
    /// </summary>
    public readonly UInt16 BaseLinePx;

    /// <summary>
    /// The number of pixels that was added as padding around each character
    /// </summary>
    public readonly PxThicknessU16 PaddingPx;

    /// <summary>
    /// The name of the texture
    /// </summary>
    public readonly string TextureName;

    /// <summary>
    /// The name of the font type
    /// </summary>
    public readonly BitmapFontType FontType;

    /// <summary>
    /// The sdf spread (not defined if == 0)
    /// </summary>
    public readonly UInt16 SdfSpread;

    /// <summary>
    /// The sdf scaled baseline size (not defined if == 0), can be used to calculate the desired scale factor of the sdf font
    /// </summary>
    public readonly UInt16 SdfDesiredBaseLinePx;

    public readonly ImmutableArray<BitmapFontChar> Chars;

    public readonly ImmutableArray<BitmapFontKerning> Kernings;

    public BitmapFont(string name, UInt16 dpi, UInt16 size, UInt16 lineSpacingPx, UInt16 baseLinePx, PxThicknessU16 paddingPx, string textureName,
                      BitmapFontType fontType, UInt16 sdfSpread, UInt16 sdfDesiredBaseLinePx, BitmapFontChar[] chars, BitmapFontKerning[] kernings)
      : this(name, dpi, size, lineSpacingPx, baseLinePx, paddingPx, textureName, fontType, sdfSpread, sdfDesiredBaseLinePx, ImmutableArray.Create(chars), ImmutableArray.Create(kernings))
    {
    }

    public BitmapFont(string name, UInt16 dpi, UInt16 size, UInt16 lineSpacingPx, UInt16 baseLinePx, PxThicknessU16 paddingPx, string textureName,
                      BitmapFontType fontType, UInt16 sdfSpread, UInt16 sdfDesiredBaseLinePx, BitmapFontChar[] chars,
                      ImmutableArray<BitmapFontKerning> kernings)
      : this(name, dpi, size, lineSpacingPx, baseLinePx, paddingPx, textureName, fontType, sdfSpread, sdfDesiredBaseLinePx, ImmutableArray.Create(chars), kernings)
    {
    }

    public BitmapFont(string name, UInt16 dpi, UInt16 size, UInt16 lineSpacingPx, UInt16 baseLinePx, PxThicknessU16 paddingPx, string textureName,
                      BitmapFontType fontType, UInt16 sdfSpread, UInt16 sdfDesiredBaseLinePx, ImmutableArray<BitmapFontChar> chars,
                      ImmutableArray<BitmapFontKerning> kernings)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Dpi = dpi;
      Size = size;
      LineSpacingPx = lineSpacingPx;
      BaseLinePx = baseLinePx;
      PaddingPx = paddingPx;
      TextureName = textureName ?? throw new ArgumentNullException(nameof(textureName));
      FontType = fontType;
      SdfSpread = sdfSpread;
      SdfDesiredBaseLinePx = sdfDesiredBaseLinePx;
      Chars = chars;
      Kernings = kernings;
    }
  }
}

//****************************************************************************************************************************************************
