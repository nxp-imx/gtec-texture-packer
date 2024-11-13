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

namespace FslGraphics.Font.Basic
{
  public struct BasicFontHeader : IEquatable<BasicFontHeader>
  {
    /// <summary>
    /// the texture name
    /// </summary>
    public string PathName;

    /// <summary>
    /// The line spacing in pixels (>=0)
    /// </summary>
    public int LineSpacingPx;

    /// <summary>
    /// the baseline >= 0
    /// </summary>
    public int BaseLinePx;

    public PxSize2D MaxGlyphLeadingOverdrawArea;

    public BasicFontHeader(string pathName, int lineSpacingPx, int baseLinePx, PxSize2D maxGlyphLeadingOverdrawArea)
    {
      if (lineSpacingPx < 0)
        throw new ArgumentOutOfRangeException(nameof(lineSpacingPx));
      if (baseLinePx < 0)
        throw new ArgumentOutOfRangeException(nameof(baseLinePx));

      PathName = pathName ?? throw new ArgumentNullException(nameof(pathName));
      LineSpacingPx = lineSpacingPx;
      BaseLinePx = baseLinePx;
      MaxGlyphLeadingOverdrawArea = maxGlyphLeadingOverdrawArea;
    }


    public static bool operator ==(BasicFontHeader lhs, BasicFontHeader rhs)
      => lhs.PathName == rhs.PathName && lhs.LineSpacingPx == rhs.LineSpacingPx && lhs.BaseLinePx == rhs.BaseLinePx &&
         lhs.MaxGlyphLeadingOverdrawArea == rhs.MaxGlyphLeadingOverdrawArea;

    public static bool operator !=(BasicFontHeader lhs, BasicFontHeader rhs) => !(lhs == rhs);


    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BasicFontHeader objValue && (this == objValue);


    public override int GetHashCode()
    {
      return (PathName != null ? PathName.GetHashCode(StringComparison.Ordinal) : 0) ^ LineSpacingPx.GetHashCode() ^
              BaseLinePx.GetHashCode() ^ MaxGlyphLeadingOverdrawArea.GetHashCode();
    }


    public bool Equals(BasicFontHeader other) => this == other;

    public override string ToString() => $"PathName: {PathName} LineSpacingPx: {LineSpacingPx} BaseLinePx: {BaseLinePx} MaxGlyphLeadingOverdrawArea: {MaxGlyphLeadingOverdrawArea}";
  }

}

//****************************************************************************************************************************************************
