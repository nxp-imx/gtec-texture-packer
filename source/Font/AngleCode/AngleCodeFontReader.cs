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
using System.Collections.Generic;

namespace FslGraphics.Font.AngleCode
{
  /// <summary>
  /// Decode the Angle Font Format  http://www.angelcode.com/products/bmfont/doc/file_format.html
  ///
  /// This is not a high performance implementation but it is simple.
  /// </summary>
  public static class AngleCodeFontReader
  {
    private static readonly IFormatProvider g_invariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    private enum ParseLineState
    {
      FindNextAttribute = 0,
      FindAttributeEnd = 1,
      FindValueStart = 2,
      FindValueEnd = 3,
      FindComplexValueEnd = 4,
    }



    public static AngleCodeFont Decode(string content)
    {
      if (content == null)
      {
        throw new ArgumentNullException(nameof(content));
      }
      if (!content.StartsWith("info ", StringComparison.Ordinal))
      {
        throw new Exception("Unsupported format");
      }

      content = content.Replace("\r", "", StringComparison.Ordinal);
      var lines = content.Split('\n');

      var info = ParseInfo(lines[0]);
      var common = ParseCommon(lines[1]);
      var (pages, afterPagesLineIndex) = ParsePages(lines, 2);
      var (chars, afterCharsLineIndex) = ParseChars(lines, afterPagesLineIndex);
      var (kernings, afterKerningsLineIndex) = ParseKernings(lines, afterCharsLineIndex);

      return new AngleCodeFont(info, common, pages, chars, kernings);
    }


    private static FontInfo ParseInfo(string line)
    {
      var attributeDict = ParseLine(line, "info");
      var face = GetAttributeValueAsStr(attributeDict, "face");
      var size = GetAttributeValueAsInt(attributeDict, "size");
      var bold = GetAttributeValueAsBool(attributeDict, "bold");
      var italic = GetAttributeValueAsBool(attributeDict, "italic");
      var charset = GetAttributeValueAsStr(attributeDict, "charset");
      var unicode = GetAttributeValueAsBool(attributeDict, "unicode");
      var stretchH = GetAttributeValueAsInt(attributeDict, "stretchH");
      var smooth = GetAttributeValueAsBool(attributeDict, "smooth");
      var aa = GetAttributeValueAsInt(attributeDict, "aa");
      var padding = GetAttributeValueAsPxThickness(attributeDict, "padding");
      var spacing = GetAttributeValueAsPxPoint2(attributeDict, "spacing");
      return new FontInfo(face, size, bold, italic, charset, unicode, stretchH, smooth, aa, padding, spacing);
    }


    private static FontCommon ParseCommon(string line)
    {
      var attributeDict = ParseLine(line, "common");
      var lineHeight = GetAttributeValueAsInt(attributeDict, "lineHeight");
      var baseLine = GetAttributeValueAsInt(attributeDict, "base");
      var scaleW = GetAttributeValueAsInt(attributeDict, "scaleW");
      var scaleH = GetAttributeValueAsInt(attributeDict, "scaleH");
      var pages = GetAttributeValueAsInt(attributeDict, "pages");
      var packed = GetAttributeValueAsBool(attributeDict, "packed");
      return new FontCommon(lineHeight, baseLine, scaleW, scaleH, pages, packed);
    }

    private static FontPage ParsePage(string line)
    {
      var attributeDict = ParseLine(line, "page");
      var id = GetAttributeValueAsInt(attributeDict, "id");
      var file = GetAttributeValueAsStr(attributeDict, "file");
      return new FontPage(id, file);
    }

    private static Tuple<List<FontPage>, int> ParsePages(string[] lines, int lineIndex)
    {
      var result = new List<FontPage>();
      while (lineIndex < lines.Length && lines[lineIndex].StartsWith("page ", StringComparison.Ordinal))
      {
        result.Add(ParsePage(lines[lineIndex]));
        lineIndex += 1;
      }
      return new Tuple<List<FontPage>, int>(result, lineIndex);
    }

    private static FontChar ParseChar(string line)
    {
      var attributeDict = ParseLine(line, "char");
      var id = GetAttributeValueAsInt(attributeDict, "id");
      var x = GetAttributeValueAsInt(attributeDict, "x");
      var y = GetAttributeValueAsInt(attributeDict, "y");
      var width = GetAttributeValueAsInt(attributeDict, "width");
      var height = GetAttributeValueAsInt(attributeDict, "height");
      var xoffset = GetAttributeValueAsInt(attributeDict, "xoffset");
      var yoffset = GetAttributeValueAsInt(attributeDict, "yoffset");
      var xadvance = GetAttributeValueAsInt(attributeDict, "xadvance");
      var page = GetAttributeValueAsInt(attributeDict, "page");
      var chnl = GetAttributeValueAsInt(attributeDict, "chnl");
      return new FontChar(id, new PxRectangle(x, y, width, height), new PxPoint2(xoffset, yoffset), xadvance, page, chnl);
    }

    private static Tuple<List<FontChar>, int> ParseChars(string[] lines, int lineIndex)
    {
      var attributeDict = ParseLine(lines[lineIndex], "chars");
      var charCount = GetAttributeValueAsInt(attributeDict, "count");
      ++lineIndex;

      var result = new List<FontChar>();
      while (lineIndex < lines.Length && lines[lineIndex].StartsWith("char ", StringComparison.Ordinal))
      {
        result.Add(ParseChar(lines[lineIndex]));
        ++lineIndex;
      }

      if (result.Count != charCount)
        throw new Exception($"File did not contain the expected amount of chars. Expected: {charCount} Found: {result.Count}");

      return Tuple.Create(result, lineIndex);
    }


    private static FontKerning ParseKerning(string line)
    {
      var attributeDict = ParseLine(line, "kerning");
      var first = GetAttributeValueAsUInt32(attributeDict, "first");
      var second = GetAttributeValueAsUInt32(attributeDict, "second");
      var amount = GetAttributeValueAsInt32(attributeDict, "amount");
      return new FontKerning(first, second, amount);
    }

    private static Tuple<List<FontKerning>, int> ParseKernings(string[] lines, int lineIndex)
    {
      var result = new List<FontKerning>();
      if (lines[lineIndex].StartsWith("kernings" + ' ', StringComparison.Ordinal))
      {
        var attributeDict = ParseLine(lines[lineIndex], "kernings");
        var kerningCount = GetAttributeValueAsInt(attributeDict, "count");
        ++lineIndex;

        while (lineIndex < lines.Length && lines[lineIndex].StartsWith("kerning ", StringComparison.Ordinal))
        {
          result.Add(ParseKerning(lines[lineIndex]));
          ++lineIndex;
        }

        if (result.Count != kerningCount)
          throw new Exception($"File did not contain the expected amount of chars. Expected: {kerningCount} Found: {result.Count}");
      }
      return new Tuple<List<FontKerning>, int>(result, lineIndex);
    }


    private static AngleEntryAttribute GetAttribute(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      if (attributeDict.TryGetValue(name, out AngleEntryAttribute result))
      {
        return result;
      }
      throw new Exception($"Attribute not found '{name}'");
    }

    private static string GetAttributeValueAsStr(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      return GetAttribute(attributeDict, name).Value;
    }

    private static int GetAttributeValueAsInt(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var str = GetAttributeValueAsStr(attributeDict, name);
      return int.Parse(str, g_invariantCulture);
    }

    private static Int32 GetAttributeValueAsInt32(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var str = GetAttributeValueAsStr(attributeDict, name);
      return Int32.Parse(str, g_invariantCulture);
    }

    private static UInt32 GetAttributeValueAsUInt32(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var str = GetAttributeValueAsStr(attributeDict, name);
      return UInt32.Parse(str, g_invariantCulture);
    }

    private static bool GetAttributeValueAsBool(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var value = GetAttributeValueAsInt(attributeDict, name);
      return value != 0;
    }

    private static int[] GetAttributeValueAsIntArray(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var strArray = GetAttributeValueAsStr(attributeDict, name);
      var entries = strArray.Split(',');
      var result = new int[entries.Length];
      for (int i = 0; i < result.Length; ++i)
      {
        result[i] = int.Parse(entries[i], g_invariantCulture);
      }
      return result;
    }

    private static PxThickness GetAttributeValueAsPxThickness(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var strArray = GetAttributeValueAsStr(attributeDict, name);
      var entries = strArray.Split(',');
      if (entries.Length != 4)
        throw new Exception($"PxThickness should contain four entries not {entries.Length} '{strArray}'");
      int left = int.Parse(entries[0], g_invariantCulture);
      int top = int.Parse(entries[1], g_invariantCulture);
      int right = int.Parse(entries[2], g_invariantCulture);
      int bottom = int.Parse(entries[3], g_invariantCulture);
      return new PxThickness(left, top, right, bottom);
    }

    private static PxRectangle GetAttributeValueAsPxRectangle(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var strArray = GetAttributeValueAsStr(attributeDict, name);
      var entries = strArray.Split(',');
      if (entries.Length != 4)
        throw new Exception($"PxRectangle should contain four entries not {entries.Length} '{strArray}'");
      int left = int.Parse(entries[0], g_invariantCulture);
      int top = int.Parse(entries[1], g_invariantCulture);
      int width = int.Parse(entries[2], g_invariantCulture);
      int height = int.Parse(entries[3], g_invariantCulture);
      if (width < 0)
        throw new Exception($"PxRectangle width should not be negative ({width})");
      if (height < 0)
        throw new Exception($"PxRectangle height should not be negative ({height})");
      return new PxRectangle(left, top, width, height);
    }

    private static PxPoint2 GetAttributeValueAsPxPoint2(Dictionary<string, AngleEntryAttribute> attributeDict, string name)
    {
      var strArray = GetAttributeValueAsStr(attributeDict, name);
      var entries = strArray.Split(',');
      if (entries.Length != 2)
        throw new Exception($"PxPoint2 should contain two entries not {entries.Length} '{strArray}'");
      int x = int.Parse(entries[0], g_invariantCulture);
      int y = int.Parse(entries[1], g_invariantCulture);
      return new PxPoint2(x, y);
    }


    /// <summary>
    /// Basic line parsing
    /// </summary>
    /// <param name="line"></param>
    /// <param name="lineHeader"></param>
    /// <returns></returns>
    private static Dictionary<string, AngleEntryAttribute> ParseLine(string line, string lineHeader)
    {
      if (!line.StartsWith(lineHeader + ' ', StringComparison.Ordinal))
        throw new Exception($"Unexpected line start '{line}' expected '{lineHeader}'");

      int currentIndex = lineHeader.Length + 1;
      var state = ParseLineState.FindNextAttribute;

      int attributeStartIndex = 0;
      int attributeEndIndex = 0;
      int attributeValueStartIndex = 0;

      var result = new Dictionary<string, AngleEntryAttribute>();

      while (currentIndex < line.Length)
      {
        var ch = line[currentIndex];
        switch (state)
        {
          case ParseLineState.FindNextAttribute:
            if (!IsWhiteSpace(ch))
            {
              attributeStartIndex = currentIndex;
              state = ParseLineState.FindAttributeEnd;
            }
            break;
          case ParseLineState.FindAttributeEnd:
            if (ch == '=')
            {
              attributeEndIndex = currentIndex - 1;
              state = ParseLineState.FindValueStart;
            }
            else if (IsWhiteSpace(ch))
            {
              throw new Exception("Found unexpected whitespace");
            }
            break;
          case ParseLineState.FindValueStart:
            if (ch == '"')
            {
              state = ParseLineState.FindComplexValueEnd;
              attributeValueStartIndex = currentIndex + 1;
            }
            else
            {
              state = ParseLineState.FindValueEnd;
              attributeValueStartIndex = currentIndex;
            }
            break;
          case ParseLineState.FindValueEnd:
            if (IsWhiteSpace(ch))
            {
              state = ParseLineState.FindNextAttribute;
              var name = line.Substring(attributeStartIndex, (attributeEndIndex + 1) - attributeStartIndex);
              var value = line.Substring(attributeValueStartIndex, currentIndex - attributeValueStartIndex);
              result[name] = new AngleEntryAttribute(name, value);
            }
            break;
          case ParseLineState.FindComplexValueEnd:
            if (ch == '"')
            {
              state = ParseLineState.FindNextAttribute;
              var name = line.Substring(attributeStartIndex, (attributeEndIndex + 1) - attributeStartIndex);
              var value = line.Substring(attributeValueStartIndex, currentIndex - attributeValueStartIndex);
              result[name] = new AngleEntryAttribute(name, value);
            }
            break;
          default:
            break;
        }
        ++currentIndex;
      }
      if (state == ParseLineState.FindValueEnd)
      {
        var name = line.Substring(attributeStartIndex, (attributeEndIndex + 1) - attributeStartIndex);
        var value = line.Substring(attributeValueStartIndex);
        result[name] = new AngleEntryAttribute(name, value);
      }
      else if (state != ParseLineState.FindNextAttribute)
      {
        throw new Exception($"Failed to parse line '{line}'");
      }
      return result;
    }

    private static bool IsWhiteSpace(char ch)
    {
      return ch == ' ' || ch == '\t';
    }
  }
}
