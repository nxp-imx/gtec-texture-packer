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

namespace FslGraphics.Font.Basic
{
  /// <summary>
  /// </summary>
  public struct BasicFontGlyphRange : IEquatable<BasicFontGlyphRange>
  {
    /// <summary>
    /// </summary>
    public int From;

    /// <summary>
    /// </summary>
    public int Length;

    /// <summary>
    /// </summary>
    public int Offset;


    public BasicFontGlyphRange(int from, int length, int offset)
    {
      if (from < 0)
        throw new ArgumentOutOfRangeException(nameof(from));
      if (length < 0)
        throw new ArgumentOutOfRangeException(nameof(length));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof(offset));

      From = from;
      Length = length;
      Offset = offset;
    }



    public static bool operator ==(BasicFontGlyphRange lhs, BasicFontGlyphRange rhs)
    {
      return lhs.From == rhs.From && lhs.Length == rhs.Length && lhs.Offset == rhs.Offset;
    }

    public static bool operator !=(BasicFontGlyphRange lhs, BasicFontGlyphRange rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is BasicFontGlyphRange) ? false : (this == (BasicFontGlyphRange)obj);
    }


    public override int GetHashCode()
    {
      return From.GetHashCode() ^ Length.GetHashCode() ^ Offset.GetHashCode();
    }


    public bool Equals(BasicFontGlyphRange other)
    {
      return this == other;
    }


    public override string ToString()
    {
      return $"From: {From} Length: {Length} Offset: {Offset}";
    }
  }

}

//****************************************************************************************************************************************************
