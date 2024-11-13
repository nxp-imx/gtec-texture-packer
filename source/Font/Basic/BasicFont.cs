#nullable enable
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
using System.Collections.Immutable;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.Basic
{
  /// <summary>
  /// This describes on character in the font. There is one for each included character in the font.
  /// </summary>
  public class BasicFont
  {
    /// <summary>
    /// The texture name
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The basic font header
    /// </summary>
    public readonly BasicFontHeader Header;

    /// <summary>
    /// Ranges
    /// </summary>
    public readonly ImmutableArray<BasicFontGlyphRange> Ranges;

    /// <summary>
    /// The basic font kerning
    /// </summary>
    public readonly ImmutableArray<BasicFontGlyphKerning> Kerning;

    public BasicFont(string name, BasicFontHeader header, BasicFontGlyphRange[] ranges, BasicFontGlyphKerning[] kerning)
      : this(name, header, ImmutableArray.Create(ranges), ImmutableArray.Create(kerning))
    {
    }

    public BasicFont(string name, BasicFontHeader header, ImmutableArray<BasicFontGlyphRange> ranges, ImmutableArray<BasicFontGlyphKerning> kerning)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Header = header;
      Ranges = ranges;
      Kerning = kerning;
      ValidateBasicFont();
    }

    public int CountChars()
    {
      int total = 0;
      foreach (var range in Ranges)
      {
        total += range.Length;
      }
      return total;
    }

    private void ValidateBasicFont()
    {
      foreach (var entry in Kerning)
      {
        if (!IsKnownGlyph(entry.Id))
          throw new NotSupportedException($"Found kerning for id {entry.Id} not mentioned in any range");
      }
    }

    private bool IsKnownGlyph(int id)
    {
      foreach (var range in Ranges)
      {
        if (id >= range.From && id < (range.From + range.Length))
          return true;
      }
      return false;
    }
  }

}

//****************************************************************************************************************************************************
