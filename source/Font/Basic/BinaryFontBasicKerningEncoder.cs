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

using MB.Base.Container;
using System;
using System.Collections.Immutable;
using System.Diagnostics;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.Basic
{
  /// <summary>
  /// Write a binary basic font kerning file.
  /// This is a very simple font format.
  /// </summary>
  public class BinaryFontBasicKerningEncoder
  {
    public readonly string DefaultExtension = "fbk";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want to keep this a object for future modifications")]
    public ByteList Encode(BasicFont basicFont)
    {
      if (basicFont == null)
        throw new ArgumentNullException(nameof(basicFont));

      // The file size is normally fairly small so this ought to be more than enough
      var dstBuffer = new ByteList(4096 * 4);
      var offsetSize = WriteHeader(dstBuffer);
      var sizeOfHeader = dstBuffer.Count;

      WriteRanges(dstBuffer, basicFont.Ranges);
      WriteRangeGlyphKernings(dstBuffer, basicFont.Kerning);
      WriteDescription(dstBuffer, basicFont);

      dstBuffer.AddString(basicFont.Name);
      dstBuffer.AddString(basicFont.Header.PathName);

      // Write the number of bytes that were written to the extended header
      Debug.Assert(sizeOfHeader >= 0 && sizeOfHeader < dstBuffer.Count);
      var bytesWritten = (UInt32)(dstBuffer.Count - sizeOfHeader);
      dstBuffer.SetUInt32(offsetSize, bytesWritten);
      return dstBuffer;
    }


    private static int WriteHeader(ByteList dstBuffer)
    {
      Debug.Assert(dstBuffer != null);

      dstBuffer.AddUInt32(0x004B4246);
      dstBuffer.AddUInt32(1);
      var offset = dstBuffer.Count;
      dstBuffer.AddUInt32(0);
      return offset;
    }

    private static void WriteRanges(ByteList dstBuffer, ImmutableArray<BasicFontGlyphRange> ranges)
    {
      dstBuffer.AddEncodedUInt32((UInt32)ranges.Length);
      for (int i = 0; i < ranges.Length; ++i)
      {
        WriteRange(dstBuffer, ranges[i]);
      }
    }

    private static void WriteRange(ByteList dstBuffer, BasicFontGlyphRange range)
    {
      Debug.Assert(range.From >= 0);
      Debug.Assert(range.Length >= 0);
      dstBuffer.AddEncodedUInt32((UInt32)range.From);
      dstBuffer.AddEncodedUInt32((UInt32)range.Length);
    }

    private static void WriteRangeGlyphKernings(ByteList dstBuffer, ImmutableArray<BasicFontGlyphKerning> kerning)
    {
      dstBuffer.AddEncodedUInt32((UInt32)kerning.Length);
      for (int i = 0; i < kerning.Length; ++i)
      {
        WriteGlyphKerning(dstBuffer, kerning[i]);
      }
    }

    private static void WriteGlyphKerning(ByteList dstBuffer, BasicFontGlyphKerning kerning)
    {
      dstBuffer.AddEncodedInt32(kerning.OffsetPx.X);
      dstBuffer.AddEncodedInt32(kerning.OffsetPx.Y);
      Debug.Assert(kerning.LayoutWidthPx >= 0);
      dstBuffer.AddEncodedUInt32((UInt32)kerning.LayoutWidthPx);
    }

    private static void WriteDescription(ByteList dstBuffer, BasicFont basicFont)
    {
      Debug.Assert(basicFont.Header.LineSpacingPx >= 0);
      Debug.Assert(basicFont.Header.BaseLinePx >= 0);

      dstBuffer.AddEncodedUInt32((UInt32)basicFont.Header.LineSpacingPx);
      dstBuffer.AddEncodedUInt32((UInt32)basicFont.Header.BaseLinePx);
      dstBuffer.AddEncodedUInt32((UInt32)basicFont.Header.MaxGlyphLeadingOverdrawArea.Width);
      dstBuffer.AddEncodedUInt32((UInt32)basicFont.Header.MaxGlyphLeadingOverdrawArea.Height);
    }
  }
}

//****************************************************************************************************************************************************
