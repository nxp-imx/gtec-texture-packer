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

namespace FslGraphics.Font.BF
{
  /// <summary>
  /// The kerning information is used to adjust the distance between certain characters,
  /// e.g. some characters should be placed closer to each other than others.
  /// </summary>
  public struct BitmapFontKerning : IEquatable<BitmapFontKerning>
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

    public BitmapFontKerning(UInt32 first, UInt32 second, Int32 amountPx)
    {
      First = first;
      Second = second;
      AmountPx = amountPx;
    }

    public static bool operator ==(BitmapFontKerning lhs, BitmapFontKerning rhs)
    {
      return lhs.First == rhs.First && lhs.Second == rhs.Second && lhs.AmountPx == rhs.AmountPx;
    }

    public static bool operator !=(BitmapFontKerning lhs, BitmapFontKerning rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is BitmapFontKerning) ? false : (this == (BitmapFontKerning)obj);
    }


    public override int GetHashCode()
    {
      return First.GetHashCode() ^ Second.GetHashCode() ^ AmountPx.GetHashCode();
    }


    public bool Equals(BitmapFontKerning other)
    {
      return this == other;
    }

    public override string ToString()
    {
      return $"First: {First} Second: {Second} AmountPx: {AmountPx}";
    }
  }
}

//****************************************************************************************************************************************************
