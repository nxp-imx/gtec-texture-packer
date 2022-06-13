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
using System.Collections.Generic;
using System.Diagnostics;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.AngleCode
{
  /// <summary>
  /// The kerning information is used to adjust the distance between certain characters,
  /// e.g. some characters should be placed closer to each other than others.
  /// </summary>
  public class AngleCodeFont
  {
    public readonly FontInfo Info;
    public readonly FontCommon Common;
    public readonly List<FontPage> Pages;
    public readonly List<FontChar> Chars;
    public readonly List<FontKerning> Kernings;

    public AngleCodeFont(FontInfo info, FontCommon common, List<FontPage> pages, List<FontChar> chars, List<FontKerning> kernings)
    {
      if (pages == null)
        throw new ArgumentNullException(nameof(pages));
      if (chars == null)
        throw new ArgumentNullException(nameof(chars));
      if (kernings == null)
        throw new ArgumentNullException(nameof(kernings));

      pages.Sort((lhs, rhs) => lhs.Id.CompareTo(rhs.Id));
      chars.Sort((lhs, rhs) => lhs.Id.CompareTo(rhs.Id));
      kernings.Sort((lhs, rhs) => lhs.First.CompareTo(rhs.First));

      ValidatePages(pages);
      ValidateChars(chars);

      Info = info;
      Common = common;
      Pages = pages;
      Chars = chars;
      Kernings = kernings;
    }

    private static void ValidatePages(List<FontPage> pages)
    {
      Debug.Assert(pages != null);

      int lastPage = -1;
      for (int i = 0; i < pages.Count; ++i)
      {
        var page = pages[i];
        if (page.Id < 0 || page.Id >= pages.Count)
          throw new Exception($"Page id {page.Id} is out of bounds");
        if (page.Id <= lastPage)
          throw new Exception($"Duplicated page id {page.Id} found");
        lastPage = page.Id;
      }
    }

    private static void ValidateChars(List<FontChar> chars)
    {
      Debug.Assert(chars != null);

      int lastChar = -1;
      for (int i = 0; i < chars.Count; ++i)
      {
        var entry = chars[i];
        if (entry.Id < 0)
          throw new Exception($"Char id {entry.Id} is out of bounds");
        if (entry.Id <= lastChar)
          throw new Exception($"Duplicated char id {entry.Id} found");
        if (entry.SrcTextureRectPx.X < 0)
          throw new Exception($"char.Id={entry.Id} X can not be negative ({entry.SrcTextureRectPx.X})");
        if (entry.SrcTextureRectPx.Y < 0)
          throw new Exception($"char.Id={entry.Id} Y can not be negative ({entry.SrcTextureRectPx.Y})");
        lastChar = entry.Id;
      }
    }
  }
}

//****************************************************************************************************************************************************
