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
  /// The kerning information is used to adjust the distance between certain characters,
  /// e.g. some characters should be placed closer to each other than others.
  /// Based on: https://www.angelcode.com/products/bmfont/doc/file_format.html
  /// </summary>
  public struct FontKerning : IEquatable<FontKerning>
  {
    /// <summary>
    /// </summary>
    public UInt32 First;

    /// <summary>
    /// </summary>
    public UInt32 Second;

    /// <summary>
    /// </summary>
    public Int32 AmountPx;

    public FontKerning(UInt32 first, UInt32 second, Int32 amount)
    {
      First = first;
      Second = second;
      AmountPx = amount;
    }


    public static bool operator ==(FontKerning lhs, FontKerning rhs)
    {
      return lhs.First == rhs.First && lhs.Second == rhs.Second && lhs.AmountPx == rhs.AmountPx;
    }

    public static bool operator !=(FontKerning lhs, FontKerning rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is FontKerning) ? false : (this == (FontKerning)obj);
    }


    public override int GetHashCode()
    {
      return First.GetHashCode() ^ Second.GetHashCode() ^ AmountPx.GetHashCode();
    }


    public bool Equals(FontKerning other)
    {
      return this == other;
    }

    public override string ToString()
    {
      return $"First: {First} Second: {Second} Amount: {AmountPx}";
    }
  }

}

//****************************************************************************************************************************************************
