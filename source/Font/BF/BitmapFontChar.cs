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
using System.Diagnostics.CodeAnalysis;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.BF
{
  /// <summary>
  /// This describes on character in the font. There is one for each included character in the font.
  /// </summary>
  public struct BitmapFontChar : IEquatable<BitmapFontChar>
  {
    /// <summary>
    /// The character id.
    /// </summary>
    public UInt32 Id;

    /// <summary>
    /// The rectangle of the character image in the texture.
    /// </summary>
    public PxRectangle SrcTextureRectPx;

    /// <summary>
    /// How much the current position should be offset when copying the image from the texture to the screen.
    /// </summary>
    public PxPoint2 OffsetPx;

    /// <summary>
    /// How much the current position should be advanced after drawing the character.
    /// </summary>
    public UInt16 XAdvancePx;

    public BitmapFontChar(UInt32 id, PxRectangle srcTextureRectPx, PxPoint2 offsetPx, UInt16 xAdvancePx)
    {
      Id = id;
      SrcTextureRectPx = srcTextureRectPx;
      OffsetPx = offsetPx;
      XAdvancePx = xAdvancePx;
    }

    public static bool operator ==(BitmapFontChar lhs, BitmapFontChar rhs)
      => lhs.Id == rhs.Id && lhs.SrcTextureRectPx == rhs.SrcTextureRectPx && lhs.OffsetPx == rhs.OffsetPx && lhs.XAdvancePx == rhs.XAdvancePx;

    public static bool operator !=(BitmapFontChar lhs, BitmapFontChar rhs) => !(lhs == rhs);


    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BitmapFontChar objValue && (this == objValue);


    public override int GetHashCode() => HashCode.Combine(Id, SrcTextureRectPx, OffsetPx, XAdvancePx);

    public bool Equals(BitmapFontChar other) => this == other;

    public override string ToString() => $"Id: {Id} SrcTextureRectPx: {SrcTextureRectPx} OffsetPx: {OffsetPx} XAdvancePx: {XAdvancePx}";
  }

}

//****************************************************************************************************************************************************
