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

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.AngleCode
{
  /// <summary>
  /// This holds information on how the font was generated.
  /// Based on: https://www.angelcode.com/products/bmfont/doc/file_format.html
  /// </summary>
  public struct FontInfo : IEquatable<FontInfo>
  {
    /// <summary>
    /// This is the name of the true type font.
    /// </summary>
    public string Face;

    /// <summary>
    /// The size of the true type font.
    /// </summary>
    public int Size;

    /// <summary>
    /// The font is bold
    /// </summary>
    public bool Bold;

    /// <summary>
    /// The font is italic
    /// </summary>
    public bool Italic;

    /// <summary>
    /// The name of the OEM charset used (when not unicode).
    /// </summary>
    public string Charset;

    /// <summary>
    /// Set to true if it is the unicode charset
    /// </summary>
    public bool Unicode;

    /// <summary>
    /// The font height stretch in percentage. 100% means no stretch
    /// </summary>
    public int StretchH;

    /// <summary>
    /// Set to true if smoothing was turned on.
    /// </summary>
    public bool Smooth;

    /// <summary>
    /// The supersampling level used. 1 means no supersampling was used.
    /// </summary>
    public int Aa;

    /// <summary>
    ///  padding for each character (up, right, down, left).
    /// </summary>
    public PxThickness PaddingPx;

    /// <summary>
    /// // The spacing for each character (horizontal, vertical).
    /// </summary>
    public PxPoint2 SpacingPx;

    public FontInfo(string face, int size, bool bold, bool italic, string charset, bool unicode, int stretchH, bool smooth, int aa,
                    PxThickness paddingPx, PxPoint2 spacingPx)
    {
      Face = face ?? throw new ArgumentNullException(nameof(face));
      Size = size;
      Bold = bold;
      Italic = italic;
      Charset = charset ?? throw new ArgumentNullException(nameof(charset));
      Unicode = unicode;
      StretchH = stretchH;
      Smooth = smooth;
      Aa = aa;
      PaddingPx = paddingPx;
      SpacingPx = spacingPx;
    }

    public bool IsValid
    {
      get
      {
        return Face != null && Charset != null;
      }
    }


    public static bool operator ==(FontInfo lhs, FontInfo rhs)
    {
      return lhs.Face == rhs.Face && lhs.Size == rhs.Size && lhs.Bold == rhs.Bold && lhs.Italic == rhs.Italic && lhs.Charset == rhs.Charset &&
             lhs.Unicode == rhs.Unicode && lhs.StretchH == rhs.StretchH && lhs.Smooth == rhs.Smooth && lhs.Aa == rhs.Aa &&
             lhs.PaddingPx == rhs.PaddingPx && lhs.SpacingPx == rhs.SpacingPx;
    }

    public static bool operator !=(FontInfo lhs, FontInfo rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is FontInfo) ? false : (this == (FontInfo)obj);
    }


    public override int GetHashCode()
    {
      return (Face != null ? Face.GetHashCode(StringComparison.Ordinal) : 0) ^ Size.GetHashCode() ^ Bold.GetHashCode() ^ Italic.GetHashCode() ^
             (Charset != null ? Charset.GetHashCode(StringComparison.Ordinal) : 0) ^ Unicode.GetHashCode() ^ StretchH.GetHashCode() ^ Smooth.GetHashCode() ^
             Aa.GetHashCode() ^ PaddingPx.GetHashCode() ^ SpacingPx.GetHashCode();
    }


    public bool Equals(FontInfo other)
    {
      return this == other;
    }


    public override string ToString()
    {
      return $"Face: {Face} Size: {Size} Bold: {Bold} Italic: {Italic} Charset: {Charset} Unicode: {Unicode} StretchH: {StretchH} Smooth: {Smooth} Aa: {Aa} PaddingPx: {PaddingPx} SpacingPx: {SpacingPx}";
    }
  }

}

//****************************************************************************************************************************************************
