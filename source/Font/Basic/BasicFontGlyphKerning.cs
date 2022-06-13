﻿/****************************************************************************************************************************************************
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

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.Basic
{
  /// <summary>
  /// </summary>
  public struct BasicFontGlyphKerning : IEquatable<BasicFontGlyphKerning>
  {
    /// <summary>
    /// The character id.
    /// </summary>
    public int Id;

    /// <summary>
    /// The offset in pixels
    /// </summary>
    public PxPoint2 OffsetPx;

    /// <summary>
    /// The layout width in pixels
    /// How much the current position should be advanced after drawing the character.
    /// </summary>
    public int LayoutWidthPx;

    public BasicFontGlyphKerning(int id, PxPoint2 offsetPx, int layoutWidthPx)
    {
      if (layoutWidthPx < 0)
        throw new ArgumentOutOfRangeException(nameof(layoutWidthPx));

      Id = id;
      OffsetPx = offsetPx;
      LayoutWidthPx = layoutWidthPx;
    }



    public static bool operator ==(BasicFontGlyphKerning lhs, BasicFontGlyphKerning rhs)
    {
      return lhs.Id == rhs.Id && lhs.OffsetPx == rhs.OffsetPx && lhs.LayoutWidthPx == rhs.LayoutWidthPx;
    }

    public static bool operator !=(BasicFontGlyphKerning lhs, BasicFontGlyphKerning rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is BasicFontGlyphKerning) ? false : (this == (BasicFontGlyphKerning)obj);
    }


    public override int GetHashCode()
    {
      return Id.GetHashCode() ^ OffsetPx.GetHashCode() ^ LayoutWidthPx.GetHashCode();
    }


    public bool Equals(BasicFontGlyphKerning other)
    {
      return this == other;
    }


    public override string ToString()
    {
      return $"Id: {Id} OffsetPx: {OffsetPx} LayoutWidthPx: {LayoutWidthPx}";
    }
  }

}

//****************************************************************************************************************************************************
