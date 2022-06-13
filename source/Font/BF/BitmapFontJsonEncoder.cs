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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.BF
{
  /// <summary>
  /// Write a binary bitmap font
  /// </summary>
  public class BitmapFontJsonEncoder
  {
    public readonly string DefaultExtension = "json";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want to keep this a object for future modifications")]
    public ByteList Encode(BitmapFont bitmapFont)
    {
      if (bitmapFont == null)
        throw new ArgumentNullException(nameof(bitmapFont));

      var jsonDict = ToJsonDict(bitmapFont);

      var options = new JsonSerializerOptions();
      options.WriteIndented = true;
      var textResult = JsonSerializer.Serialize(jsonDict, options);

      var dstBytes = new ByteList(textResult.Length);
      dstBytes.AddPureString(textResult);

      return dstBytes;
    }


    private static Dictionary<string, object> ToJsonDict(BitmapFont bitmapFont)
    {
      var result = new Dictionary<string, object>();
      result["Format"] = "BitmapFont";
      result["Version"] = 1;
      result["Name"] = bitmapFont.Name;
      result["Size"] = bitmapFont.Size;
      result["LineSpacingPx"] = bitmapFont.LineSpacingPx;
      result["BaseLinePx"] = bitmapFont.BaseLinePx;
      result["TextureName"] = bitmapFont.TextureName;
      result["Chars"] = BitmapFontCharsToJson(bitmapFont.Chars);
      result["Kernings"] = BitmapFontKerningsToJson(bitmapFont.Kernings);
      return result;
    }


    private static List<Dictionary<string, object>> BitmapFontCharsToJson(ImmutableArray<BitmapFontChar> entries)
    {
      var result = new List<Dictionary<string, object>>();
      foreach (var entry in entries)
        result.Add(BitmapFontCharToJson(entry));
      return result;
    }

    private static Dictionary<string, object> BitmapFontCharToJson(BitmapFontChar entry)
    {
      var result = new Dictionary<string, object>();
      result["Id"] = entry.Id;
      result["X"] = entry.SrcTextureRectPx.X;
      result["Y"] = entry.SrcTextureRectPx.Y;
      result["Width"] = entry.SrcTextureRectPx.Width;
      result["Height"] = entry.SrcTextureRectPx.Height;
      result["XOffsetPx"] = entry.OffsetPx.X;
      result["YOffsetPx"] = entry.OffsetPx.Y;
      result["XAdvancePx"] = entry.XAdvancePx;
      return result;
    }

    private static List<Dictionary<string, object>> BitmapFontKerningsToJson(ImmutableArray<BitmapFontKerning> entries)
    {
      var result = new List<Dictionary<string, object>>();
      foreach (var entry in entries)
        result.Add(BitmapFontKerningToJson(entry));
      return result;
    }

    private static Dictionary<string, object> BitmapFontKerningToJson(BitmapFontKerning entry)
    {
      var result = new Dictionary<string, object>();
      result["First"] = entry.First;
      result["Second"] = entry.Second;
      result["AmountPx"] = entry.AmountPx;
      return result;
    }
  }
}

//****************************************************************************************************************************************************
