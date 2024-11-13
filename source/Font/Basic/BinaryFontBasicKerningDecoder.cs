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
using MB.Base.Bits;
using MB.Base.Compression;
using MB.Base.MathEx.Pixel;
using System;
using System.Collections.Immutable;
using System.Text;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.Basic
{
  /// <summary>
  /// Write a binary basic font kerning file.
  /// This is a very simple font format.
  /// </summary>
  public class BinaryFontBasicKerninDecoder : BinaryFontBasicKerning
  {
    public const UInt32 ExpectedVersion = 1;

    private readonly struct FBKHeader
    {
      public readonly UInt32 Magic;
      public readonly UInt32 Version;
      public readonly UInt32 Size;

      public FBKHeader(UInt32 magic, UInt32 version, UInt32 size)
      {
        Magic = magic;
        Version = version;
        Size = size;
      }
    }

    private readonly struct FBKDescription
    {
      public readonly Int32 LineSpacingPx;
      public readonly Int32 BaseLinePx;
      public readonly PxSize2D MaxGlyphLeadingOverdrawArea;

      public FBKDescription(Int32 lineSpacingPx, Int32 baseLinePx, PxSize2D maxGlyphLeadingOverdrawArea)
      {
        LineSpacingPx = lineSpacingPx;
        BaseLinePx = baseLinePx;
        MaxGlyphLeadingOverdrawArea = maxGlyphLeadingOverdrawArea;
      }
    }


    public static BasicFont Decode(ReadOnlySpan<byte> fontData)
    {
      // The file size is normally fairly small so this ought to be more than enough
      FBKHeader fbkHeader = ReadAndValidateHeader(ref fontData);

      ImmutableArray<BasicFontGlyphRange> ranges = ReadRanges(ref fontData);
      ImmutableArray<BasicFontGlyphKerning> kernings = ReadRangeGlyphKernings(ref fontData, ranges.AsSpan());

      var description = ReadDescription(ref fontData);
      string name = ReadString(ref fontData);
      string headerPathName = ReadString(ref fontData);

      var header = new BasicFontHeader(headerPathName, description.LineSpacingPx, description.BaseLinePx, description.MaxGlyphLeadingOverdrawArea);

      return new BasicFont(name, header, ranges, kernings);
    }


    private static FBKHeader ReadAndValidateHeader(ref ReadOnlySpan<byte> rFontData)
    {
      var magic = ByteSpanUtil.ReadUInt32LE(ref rFontData);
      var version = ByteSpanUtil.ReadUInt32LE(ref rFontData);
      var size = ByteSpanUtil.ReadUInt32LE(ref rFontData);
      var header = new FBKHeader(magic, version, size);

      if (header.Magic != HeaderMagic)
      {
        throw new Exception("File not of the expected type");
      }
      if (header.Version != ExpectedVersion)
      {
        throw new Exception($"Unsupported version {header.Version} expected {ExpectedVersion}");
      }
      if (header.Size > rFontData.Length)
      {
        throw new Exception("Span not of the expected size");
      }
      return header;
    }



    private static ImmutableArray<BasicFontGlyphRange> ReadRanges(ref ReadOnlySpan<byte> rFontData)
    {
      int length = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      if (length <= 0)
        return ImmutableArray<BasicFontGlyphRange>.Empty;

      var builder = ImmutableArray.CreateBuilder<BasicFontGlyphRange>(length);
      for (int i = 0; i < length; ++i)
      {
        builder.Add(ReadRange(ref rFontData));
      }
      return builder.MoveToImmutable();
    }

    private static BasicFontGlyphRange ReadRange(ref ReadOnlySpan<byte> rFontData)
    {
      var from = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      var length = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      int offset = 0; // offset is not written to the file -> so we can not read it
      return new BasicFontGlyphRange(from, length, offset);
    }



    private static ImmutableArray<BasicFontGlyphKerning> ReadRangeGlyphKernings(ref ReadOnlySpan<byte> rFontData, ReadOnlySpan<BasicFontGlyphRange> ranges)
    {
      int length = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      if (length <= 0)
        return ImmutableArray<BasicFontGlyphKerning>.Empty;

      var builder = ImmutableArray.CreateBuilder<BasicFontGlyphKerning>(length);
      int currentRangeIndex = 0;
      int rangeIndex = 0;
      for (int i = 0; i < length; ++i)
      {
        while (currentRangeIndex < ranges.Length && rangeIndex >= ranges[currentRangeIndex].Length)
        {
          rangeIndex = 0;
          ++currentRangeIndex;
        }
        builder.Add(ReadGlyphKerning(ref rFontData, ranges[currentRangeIndex].From + rangeIndex));
      }
      if (builder.Count != length)
        throw new Exception("Format error did not encounter the expected amount of glyphs");
      return builder.MoveToImmutable();
    }

    private static BasicFontGlyphKerning ReadGlyphKerning(ref ReadOnlySpan<byte> rFontData, int id)
    {
      var kerningOffsetXPx = ValueCompression.ReadSimpleInt32(ref rFontData);
      var kerningOffsetYPx = ValueCompression.ReadSimpleInt32(ref rFontData);
      var layoutWidthPx = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      return new BasicFontGlyphKerning(id, new PxPoint2(kerningOffsetXPx, kerningOffsetYPx), layoutWidthPx);
    }

    private static FBKDescription ReadDescription(ref ReadOnlySpan<byte> rFontData)
    {
      var lineSpacingPx = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      var baseLinePx = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      var maxGlyphLeadingOverdrawAreaWidth = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      var maxGlyphLeadingOverdrawAreaHeight = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      return new FBKDescription(lineSpacingPx, baseLinePx, new PxSize2D(maxGlyphLeadingOverdrawAreaWidth, maxGlyphLeadingOverdrawAreaHeight));
    }

    private static string ReadString(ref ReadOnlySpan<byte> rFontData)
    {
      int length = NumericCast.ToInt32(ValueCompression.ReadSimpleUInt32(ref rFontData));
      var value = Encoding.UTF8.GetString(rFontData.Slice(0, length));
      rFontData = rFontData.Slice(length);
      return value;
    }

  }
}

//****************************************************************************************************************************************************
