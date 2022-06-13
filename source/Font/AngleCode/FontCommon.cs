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

using System;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.AngleCode
{
  /// <summary>
  /// This holds information common to all characters.
  /// Based on: https://www.angelcode.com/products/bmfont/doc/file_format.html
  /// </summary>
  public struct FontCommon : IEquatable<FontCommon>
  {
    /// <summary>
    /// This is the distance in pixels between each line of text.
    /// </summary>
    public int LineHeightPx;

    /// <summary>
    /// The number of pixels from the absolute top of the line to the base of the characters.
    /// </summary>
    public int BaseLinePx;

    /// <summary>
    /// The width of the texture, normally used to scale the x pos of the character image.
    /// </summary>
    public int ScaleW;

    /// <summary>
    /// The height of the texture, normally used to scale the y pos of the character image.
    /// </summary>
    public int ScaleH;

    /// <summary>
    /// The number of texture pages included in the font.
    /// </summary>
    public int Pages;

    /// <summary>
    /// Set to true if the monochrome characters have been packed into each of the texture channels. In this case alphaChnl describes what
    /// is stored in each channel.
    /// </summary>
    public bool Packed;

    public FontCommon(int lineHeightPx, int baseLinePx, int scaleW, int scaleH, int pages, bool packed)
    {
      LineHeightPx = lineHeightPx;
      BaseLinePx = baseLinePx;
      ScaleW = scaleW;
      ScaleH = scaleH;
      Pages = pages;
      Packed = packed;
    }


    public static bool operator ==(FontCommon lhs, FontCommon rhs)
    {
      return lhs.LineHeightPx == rhs.LineHeightPx && lhs.BaseLinePx == rhs.BaseLinePx && lhs.ScaleW == rhs.ScaleW && lhs.ScaleH == rhs.ScaleH &&
             lhs.Pages == rhs.Pages && lhs.Packed == rhs.Packed;
    }

    public static bool operator !=(FontCommon lhs, FontCommon rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is FontCommon) ? false : (this == (FontCommon)obj);
    }


    public override int GetHashCode()
    {
      return LineHeightPx.GetHashCode() ^ BaseLinePx.GetHashCode() ^ ScaleW.GetHashCode() ^ ScaleH.GetHashCode() ^ Pages.GetHashCode() ^
             Packed.GetHashCode();
    }


    public bool Equals(FontCommon other)
    {
      return this == other;
    }


    public override string ToString()
    {
      return $"LineHeightPx: {LineHeightPx} BasePx: {BaseLinePx} ScaleW: {ScaleW} ScaleH: {ScaleH} Pages: {Pages} Packed: {Packed}";
    }
  }

}

//****************************************************************************************************************************************************
