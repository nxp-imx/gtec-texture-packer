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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FslGraphics.Font.MsdfAtlas
{
  public class Root
  {
    [JsonPropertyName("atlas")]
    public Atlas Atlas { get; set; }

    [JsonPropertyName("metrics")]
    public Metrics Metrics { get; set; }

    [JsonPropertyName("glyphs")]
    public required List<Glyph> Glyphs { get; set; }

    [JsonPropertyName("kerning")]
    public required List<Kerning> Kerning { get; set; }
  }

  /// <summary>
  /// Atlas section includes the settings used to generate the atlas, including its type and dimensions
  /// </summary>
  public readonly struct Atlas
  {
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    /// This appears to correspond to the supplied px to the pxrange parameter or if no
    /// pxrange was supplied this will be equal to the calculated pxrange.
    /// </summary>
    [JsonPropertyName("distanceRange")]
    public double DistanceRange { get; }

    [JsonPropertyName("distanceRangeMiddle")]
    public double DistanceRangeMiddle { get; }

    /// <summary>
    /// Represents the font size in pixels per em
    /// </summary>
    [JsonPropertyName("size")]
    public double Size { get; }

    /// <summary>
    /// Texture width
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; }

    /// <summary>
    /// Texture height
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; }

    [JsonPropertyName("yOrigin")]
    public string YOrigin { get; }

    [JsonConstructor]
    public Atlas(string type, double distanceRange, double distanceRangeMiddle, double size, int width, int height, string yOrigin)
    {
      Type = type;
      DistanceRange = distanceRange;
      DistanceRangeMiddle = distanceRangeMiddle;
      Size = size;
      Width = width;
      Height = height;
      YOrigin = yOrigin;
    }
  }

  /// <summary>
  /// Metrics section contains useful font metric values retrieved from the font.
  /// All values are in em's.
  /// </summary>
  public readonly struct Metrics
  {
    [JsonPropertyName("emSize")]
    public double EmSize { get; }

    /// <summary>
    ///  The vertical distance between baselines of consecutive lines.
    /// </summary>
    [JsonPropertyName("lineHeight")]
    public double LineHeight { get; }

    /// <summary>
    /// The height above the baseline where the tallest glyph extends.
    /// </summary>
    [JsonPropertyName("ascender")]
    public double Ascender { get; }

    /// <summary>
    /// The depth below the baseline where the lowest glyph extends.
    /// </summary>
    [JsonPropertyName("descender")]
    public double Descender { get; }

    [JsonPropertyName("underlineY")]
    public double UnderlineY { get; }

    [JsonPropertyName("underlineThickness")]
    public double UnderlineThickness { get; }

    [JsonConstructor]
    public Metrics(double emSize, double lineHeight, double ascender, double descender, double underlineY, double underlineThickness)
    {
      EmSize = emSize;
      LineHeight = lineHeight;
      Ascender = ascender;
      Descender = descender;
      UnderlineY = underlineY;
      UnderlineThickness = underlineThickness;
    }
  }

  public readonly struct Bounds
  {
    [JsonPropertyName("left")]
    public double Left { get; }

    [JsonPropertyName("bottom")]
    public double Bottom { get; }

    [JsonPropertyName("right")]
    public double Right { get; }

    [JsonPropertyName("top")]
    public double Top { get; }

    [JsonConstructor]
    public Bounds(double left, double bottom, double right, double top)
    {
      Left = left;
      Bottom = bottom;
      Right = right;
      Top = top;
    }
  }


  /// <summary>
  /// Glyphs is an array of individual glyphs identified by Unicode character index (unicode) or glyph index (index),
  /// depending on whether character set or glyph set mode is used.
  /// </summary>
  public class Glyph
  {
    [JsonPropertyName("unicode")]
    public int Unicode { get; set; }

    /// <summary>
    /// Advance is the horizontal advance in em's.
    /// </summary>
    [JsonPropertyName("advance")]
    public double Advance { get; set; }

    /// <summary>
    /// PlaneBounds represents the glyph quad's bounds in em's relative to the baseline and horizontal cursor position.
    ///                   Cursor 0
    ///                   |
    ///                 *-|--*
    ///                 | |  |
    ///                 | |  |
    ///  baseline 0   ----|----
    ///                 | |  |
    ///                 *-|--*
    /// </summary>
    [JsonPropertyName("planeBounds")]
    public Bounds PlaneBounds { get; set; }

    /// <summary>
    /// AtlasBounds represents the glyph's bounds in the atlas in pixels.
    /// </summary>
    [JsonPropertyName("atlasBounds")]
    public Bounds AtlasBounds { get; set; }
  }

  /// <summary>
  /// Kerning lists all kerning pairs and their advance adjustment.
  /// Which needs to be added to the base advance of the first glyph in the pair
  /// </summary>
  public readonly struct Kerning
  {
    [JsonPropertyName("unicode1")]
    public int Unicode1 { get; }

    [JsonPropertyName("unicode2")]
    public int Unicode2 { get; }

    [JsonPropertyName("advance")]
    public double Advance { get; }

    [JsonConstructor]
    public Kerning(int unicode1, int unicode2, double advance)
    {
      Unicode1 = unicode1;
      Unicode2 = unicode2;
      Advance = advance;
    }
  }
}
