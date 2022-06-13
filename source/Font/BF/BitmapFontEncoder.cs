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

using MB.Base;
using MB.Base.Container;
using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.BF
{
  /// <summary>
  /// Write a binary bitmap font
  /// </summary>
  public class BitmapFontEncoder
  {
    public readonly string DefaultExtension = "nbf";
    private const UInt32 CurrentVersion = 3;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want to keep this a object for future modifications")]
    public ByteList Encode(BitmapFont bitmapFont)
    {
      if (bitmapFont == null)
        throw new ArgumentNullException(nameof(bitmapFont));

      // The file size is normally fairly small so this ought to be more than enough
      var dstBuffer = new ByteList(4096 * 4);

      int offsetSize = WriteHeader(dstBuffer);
      var sizeOfHeader = dstBuffer.Count;

      WriteContent(dstBuffer, bitmapFont);

      // Write the number of bytes that were written to the extended header
      Debug.Assert(sizeOfHeader >= 0 && sizeOfHeader < dstBuffer.Count);
      var bytesWritten = UncheckedNumericCast.ToUInt32(dstBuffer.Count - sizeOfHeader);
      dstBuffer.SetUInt32(offsetSize, bytesWritten);
      return dstBuffer;
    }

    private static int WriteHeader(ByteList dstBuffer)
    {
      Debug.Assert(dstBuffer != null);

      dstBuffer.AddUInt32(0x004E4246);
      dstBuffer.AddUInt32(CurrentVersion);
      var offset = dstBuffer.Count;
      dstBuffer.AddUInt32(0);
      return offset;
    }

    private static void WriteContent(ByteList dstBuffer, BitmapFont bitmapFont)
    {
      Debug.Assert(dstBuffer != null);
      Debug.Assert(bitmapFont != null);

      dstBuffer.AddString(bitmapFont.Name);
      dstBuffer.AddEncodedUInt16(bitmapFont.Dpi);
      dstBuffer.AddEncodedUInt16(bitmapFont.Size);
      dstBuffer.AddEncodedUInt16(bitmapFont.LineSpacingPx);
      dstBuffer.AddEncodedUInt16(bitmapFont.BaseLinePx);
      dstBuffer.AddEncodedUInt16(bitmapFont.PaddingPx.Left);          // new in V3
      dstBuffer.AddEncodedUInt16(bitmapFont.PaddingPx.Top);           // new in V3
      dstBuffer.AddEncodedUInt16(bitmapFont.PaddingPx.Right);         // new in V3
      dstBuffer.AddEncodedUInt16(bitmapFont.PaddingPx.Bottom);        // new in V3
      dstBuffer.AddEncodedUInt16(bitmapFont.SdfSpread);               // new in V3
      dstBuffer.AddEncodedUInt16(bitmapFont.SdfDesiredBaseLinePx);    // new in V3
      dstBuffer.AddString(bitmapFont.TextureName);
      dstBuffer.AddEncodedUInt32((UInt32)bitmapFont.FontType);

      AddChars(dstBuffer, bitmapFont.Chars);
      AddKernings(dstBuffer, bitmapFont.Kernings);
    }

    private static void AddChars(ByteList dstBuffer, ImmutableArray<BitmapFontChar> chars)
    {
      // Sort the chars by id to ensure we always write the data in the same order
      var sortedChars = new List<BitmapFontChar>(chars);
      sortedChars.Sort((lhs, rhs) => lhs.Id.CompareTo(rhs.Id));

      dstBuffer.AddEncodedUInt32(UncheckedNumericCast.ToUInt32(sortedChars.Count));
      for (int i = 0; i < sortedChars.Count; ++i)
      {
        AddChar(dstBuffer, sortedChars[i]);
      }
    }

    private static void AddChar(ByteList dstBuffer, BitmapFontChar bitmapFontChar)
    {
      dstBuffer.AddEncodedUInt32(bitmapFontChar.Id);
      dstBuffer.AddEncodedPxRectangleU(PxUncheckedTypeConverter.ToPxRectangleU(bitmapFontChar.SrcTextureRectPx));
      dstBuffer.AddEncodedPxPoint2(bitmapFontChar.OffsetPx);
      dstBuffer.AddEncodedUInt16(bitmapFontChar.XAdvancePx);
    }

    private static void AddKernings(ByteList dstBuffer, ImmutableArray<BitmapFontKerning> kernings)
    {
      // Sort the kernings by first to ensure we always write the data in the same order
      var sortedKernings = new List<BitmapFontKerning>(kernings);
      sortedKernings.Sort((lhs, rhs) => lhs.First.CompareTo(rhs.First));

      dstBuffer.AddEncodedUInt32(UncheckedNumericCast.ToUInt32(sortedKernings.Count));
      for (int i = 0; i < sortedKernings.Count; ++i)
      {
        AddKerning(dstBuffer, sortedKernings[i]);
      }
    }

    private static void AddKerning(ByteList dstBuffer, BitmapFontKerning bitmapFontKerning)
    {
      dstBuffer.AddEncodedUInt32(bitmapFontKerning.First);
      dstBuffer.AddEncodedUInt32(bitmapFontKerning.Second);
      dstBuffer.AddEncodedInt32(bitmapFontKerning.AmountPx);
    }
  }
}

//****************************************************************************************************************************************************
