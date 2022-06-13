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
using MB.Encoder.TextureAtlas.BTA;
using MB.Graphics2.TextureAtlas.Basic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.BF
{
  /// <summary>
  /// Write a binary bitmap font
  /// </summary>
  public class BitmapFontEncoderCSharp
  {
    public readonly string DefaultExtension = "cs";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want to keep this a object for future modifications")]
    public ByteList Encode(BitmapFont bitmapFont, string fontClassName, string companyName, string namespaceName, UInt32 creationYear)
    {
      if (bitmapFont == null)
        throw new ArgumentNullException(nameof(bitmapFont));

      // The file size is normally fairly small so this ought to be more than enough
      //var dstBuffer = new ByteList(4096 * 4);

      //int offsetSize = WriteHeader(dstBuffer);
      //var sizeOfHeader = dstBuffer.Count;

      //WriteContent(dstBuffer, bitmapFont);

      //// Write the number of bytes that were written to the extended header
      //Debug.Assert(sizeOfHeader >= 0 && sizeOfHeader < dstBuffer.Count);
      //var bytesWritten = UncheckedNumericCast.ToUInt32(dstBuffer.Count - sizeOfHeader);
      //dstBuffer.SetUInt32(offsetSize, bytesWritten);
      //return dstBuffer;

      using (var stringWriter = new System.IO.StringWriter())
      {
        using (var writer = new IndentedTextWriter(stringWriter, "  "))
        {
          CSharpUtil.AddHeader(writer, creationYear, companyName);
          AddUsings(writer);
          CSharpUtil.AddNamespaceBegin(writer, namespaceName);
          CSharpUtil.AddStaticClassBegin(writer, fontClassName);
          AddFontContent(writer, bitmapFont);
          AddFont(writer);
          CSharpUtil.AddClassEnd(writer);
          CSharpUtil.AddNamespaceEnd(writer);
          CSharpUtil.AddFooter(writer);
        }

        stringWriter.Flush();
        return new ByteList(System.Text.Encoding.UTF8.GetBytes(stringWriter.ToString()));
      }
    }

    private static void AddUsings(IndentedTextWriter writer)
    {
      writer.WriteLine("using MB.Base.MathEx.Pixel;");
      writer.WriteLine("using MB.Graphics2.Font;");
      writer.WriteLine("using System;");
      writer.WriteLineNoTabs("");
    }

    private static void AddFontContent(IndentedTextWriter writer, BitmapFont bitmapFont)
    {
      writer.WriteLine($"public const string Name = \"{bitmapFont.Name}\";");
      writer.WriteLine($"public const UInt16 Dpi = {bitmapFont.Dpi};");
      writer.WriteLine($"public const UInt16 Size = {bitmapFont.Size};");
      writer.WriteLine($"public const UInt16 LineSpacingPx = {bitmapFont.LineSpacingPx};");
      writer.WriteLine($"public const UInt16 BaseLinePx = {bitmapFont.BaseLinePx};");
      writer.WriteLine($"public const BitmapFontType Type = BitmapFontType.{bitmapFont.FontType};");
      writer.WriteLine($"public const string TextureName = \"{bitmapFont.TextureName}\";");
      writer.WriteLineNoTabs("");

      AddChars(writer, bitmapFont.Chars);
      AddKernings(writer, bitmapFont.Kernings);
    }


    private static void AddChars(IndentedTextWriter writer, ImmutableArray<BitmapFontChar> chars)
    {
      // Sort the chars by id to ensure we always write the data in the same order
      var sortedChars = new List<BitmapFontChar>(chars);
      sortedChars.Sort((lhs, rhs) => lhs.Id.CompareTo(rhs.Id));

      {
        writer.WriteLine($"public static readonly BitmapFontChar[] Chars = new BitmapFontChar[{sortedChars.Count}]");
        writer.WriteLine($"{{");
        ++writer.Indent;

        for (int i = 0; i < sortedChars.Count; ++i)
        {
          var ch = sortedChars[i];
          writer.WriteLine($"new BitmapFontChar({ch.Id}, new PxRectangle({ch.SrcTextureRectPx.Left}, {ch.SrcTextureRectPx.Top}, {ch.SrcTextureRectPx.Width}, {ch.SrcTextureRectPx.Height}), new PxPoint2({ch.OffsetPx.X}, {ch.OffsetPx.Y}), {ch.XAdvancePx}),");
        }

        --writer.Indent;
        writer.WriteLine($"}};");
        writer.WriteLineNoTabs("");
      }
    }


    private static void AddKernings(IndentedTextWriter writer, ImmutableArray<BitmapFontKerning> kernings)
    {
      // Sort the kernings by first to ensure we always write the data in the same order
      var sortedKernings = new List<BitmapFontKerning>(kernings);
      sortedKernings.Sort((lhs, rhs) => lhs.First.CompareTo(rhs.First));

      {
        writer.WriteLine($"public static readonly BitmapFontKerning[] Kernings = new BitmapFontKerning[{sortedKernings.Count}]");
        writer.WriteLine($"{{");
        ++writer.Indent;

        for (int i = 0; i < sortedKernings.Count; ++i)
        {
          var kern = sortedKernings[i];
          writer.WriteLine($"new BitmapFontKerning({kern.First}, {kern.Second}, {kern.AmountPx}),");
        }

        --writer.Indent;
        writer.WriteLine($"}};");
        writer.WriteLineNoTabs("");
      }
    }

    private static void AddFont(IndentedTextWriter writer)
    {
      writer.WriteLine($"public static readonly BitmapFont Font = new BitmapFont(Name, Dpi, Size, LineSpacingPx, BaseLinePx, Type, TextureName, Chars, Kernings);");
      writer.WriteLineNoTabs("");
    }

  }
}

//****************************************************************************************************************************************************
