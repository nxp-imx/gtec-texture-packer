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

using FslGraphics.Font.BF;
using NLog;
using System;
using System.Collections.Immutable;
using System.IO;
using TexturePacker.Config;

namespace TexturePacker.Commands.Atlas
{
  public class BitmapFontSdfConfig
  {
    /// <summary>
    /// The spread
    /// </summary>
    public readonly UInt16 Spread;
    /// <summary>
    /// If this is zero then no scaling should is desired
    /// </summary>
    public readonly UInt16 DesiredBaseLinePx;

    public BitmapFontSdfConfig(UInt16 spread, UInt16 desiredBaseLinePx)
    {
      if (spread < 1)
      {
        throw new ArgumentException("Must be >= 1u", nameof(spread));
      }
      Spread = spread;
      DesiredBaseLinePx = desiredBaseLinePx;
    }
  }

  public readonly struct BitmapFontTweakConfig
  {
    /// <summary>
    /// The baseline that we will modify the font to have
    /// </summary>
    public readonly UInt16 BaseLinePx;

    /// <summary>
    /// The linespacing that will be used.
    /// </summary>
    public readonly UInt16 LineSpacingPx;

    /// <summary>
    /// The char that will be measured (if != 0)
    /// </summary>
    public readonly UInt32 MeasureCharId;

    /// <summary>
    /// The margin that will be added to the measured char, so linespacing will become "MeasureHeightPx + ForceMarginPx.SumY" the rest of
    /// the font will be realigned according to those rules.
    /// If MeasureChar == 0 this is ignored.
    /// </summary>
    public readonly UInt16 MeasureHeightPx;

    public readonly BitmapFontSdfConfig? SdfConfig;

    public BitmapFontTweakConfig(UInt16 baseLinePx, UInt16 lineSpacingPx, UInt32 measureCharId, UInt16 measureHeightPx, BitmapFontSdfConfig? sdfConfig)
    {
      BaseLinePx = baseLinePx;
      LineSpacingPx = lineSpacingPx;
      MeasureCharId = measureCharId;
      MeasureHeightPx = measureHeightPx;
      SdfConfig = sdfConfig;
    }
  }

  public class AtlasCommandAddBitmapFont : AtlasCommand
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly AtlasElementConfig ElementConfig;
    public readonly string FilePath;
    public readonly string? Name;  // can be null
    public readonly BitmapFontType Type;
    public readonly ImmutableHashSet<OutputFontFormat> OutputFontFormats;
    public readonly BitmapFontTweakConfig TweakConfig;

    public AtlasCommandAddBitmapFont(AtlasElementConfig elementConfig, string filePath, string? name, BitmapFontType type,
                                     ImmutableHashSet<OutputFontFormat> outputFontFormats, BitmapFontTweakConfig tweakConfig)
      : base(AtlasCommandId.AddBitmapFont)
    {
      if (!elementConfig.IsValid)
        throw new ArgumentException("invalid ElementConfig", nameof(elementConfig));
      NameUtil.ValidatePathName(filePath);
      if (name != null)
        NameUtil.IsValidVariableName(name);

      if (outputFontFormats == null)
        throw new ArgumentNullException(nameof(outputFontFormats));
      if (outputFontFormats.Count <= 0)
        g_logger.Warn("Created without any output format");

      ElementConfig = elementConfig;
      FilePath = filePath;
      Name = name;
      Type = type;
      OutputFontFormats = outputFontFormats;
      TweakConfig = tweakConfig;
    }

    public override ResolvedAtlasCommand Resolve(in AtlasCommandResolveInfo info)
    {
      ResolvedPath resolvedFilePath = info.SrcPathResolver.Combine(info.SourcePath, FilePath);

      string relativeFontAtlasPath = AtlasPathManager.CreateRelativeFontAtlasPath(resolvedFilePath, Name);

      string orgFontName = Name == null ? Path.GetFileNameWithoutExtension(FilePath) : Name;
      ResolvedPath dstFontFilenameWithoutExtension = ResolvedPath.Append(info.DstAtlasFilenameWithoutExtension, $"_{orgFontName}");


      // Check for license files
      var fontDir = IOUtil.GetDirectoryName(resolvedFilePath.AbsolutePath);
      info.CopyManager.ResolveLicenseFiles(fontDir, info.DstAtlasPath.AbsolutePath);

      return new ResolvedAtlasCommandAddBitmapFont(ElementConfig, resolvedFilePath, Type, TweakConfig, OutputFontFormats, relativeFontAtlasPath,
                                                   dstFontFilenameWithoutExtension);
    }
  }
}
