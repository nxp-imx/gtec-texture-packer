/****************************************************************************************************************************************************
 * Copyright 2025 NXP
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
using System.Text.Json.Serialization;

namespace FslGraphics.Font.BF
{
  public sealed class SimpleRoot
  {
    [JsonPropertyName("Format")]
    public required string Format { get; set; }

    [JsonPropertyName("Version")]
    public int Version { get; set; }

    [JsonPropertyName("Name")]
    public required string Name { get; set; } = string.Empty;

    [JsonPropertyName("Size")]
    public UInt16 Size { get; set; }

    [JsonPropertyName("LineSpacingPx")]
    public UInt16 LineSpacingPx { get; set; }

    [JsonPropertyName("BaseLinePx")]
    public UInt16 BaseLinePx { get; set; }

    [JsonPropertyName("TextureName")]
    public required string TextureName { get; set; } = string.Empty;

    [JsonPropertyName("Chars")]
    public required List<JsonBitmapFontChar> Chars { get; set; }

    [JsonPropertyName("Kernings")]
    public required List<JsonFontKerning> Kernings { get; set; }
  }

  public readonly struct JsonBitmapFontChar
  {
    [JsonPropertyName("Id")]
    public UInt32 Id { get; }
    [JsonPropertyName("X")]
    public Int32 X { get; }
    [JsonPropertyName("Y")]
    public Int32 Y { get; }
    [JsonPropertyName("Width")]
    public Int32 Width { get; }
    [JsonPropertyName("Height")]
    public Int32 Height { get; }
    [JsonPropertyName("XOffsetPx")]
    public Int32 XOffsetPx { get; }
    [JsonPropertyName("YOffsetPx")]
    public Int32 YOffsetPx { get; }
    [JsonPropertyName("XAdvancePx")]
    public UInt16 XAdvancePx { get; }

    public JsonBitmapFontChar(UInt32 id, Int32 x, Int32 y, Int32 width, Int32 height, Int32 xOffsetPx, Int32 yOffsetPx, UInt16 xAdvancePx)
    {
      Id = id;
      X = x;
      Y = y;
      Width = width;
      Height = height;
      XOffsetPx = xOffsetPx;
      YOffsetPx = yOffsetPx;
      XAdvancePx = xAdvancePx;
    }
  }


  public readonly struct JsonFontKerning
  {
    [JsonPropertyName("First")]
    public UInt32 First { get; }

    [JsonPropertyName("Second")]
    public UInt32 Second { get; }

    [JsonPropertyName("AmountPx")]
    public Int32 AmountPx { get; }

    [JsonConstructor]
    public JsonFontKerning(uint first, uint second, int amountPx)
    {
      First = first;
      Second = second;
      AmountPx = amountPx;
    }
  }

}
