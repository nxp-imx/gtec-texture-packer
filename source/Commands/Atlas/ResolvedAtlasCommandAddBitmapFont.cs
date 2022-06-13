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
using TexturePacker.Config;

namespace TexturePacker.Commands.Atlas
{
  public class ResolvedAtlasCommandAddBitmapFont : ResolvedAtlasCommand
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly AtlasElementConfig ElementConfig;
    public readonly ResolvedPath FilePath;
    public readonly BitmapFontType Type;
    public readonly BitmapFontTweakConfig TweakConfig;
    public readonly ImmutableHashSet<OutputFontFormat> OutputFontFormats;
    public readonly string RelativeFontAtlasPath;
    public readonly ResolvedPath DstFilename;

    public ResolvedAtlasCommandAddBitmapFont(AtlasElementConfig elementConfig, ResolvedPath filePath, BitmapFontType type, BitmapFontTweakConfig tweakConfig,
                                             ImmutableHashSet<OutputFontFormat> outputFontFormats, string relativeFontAtlasPath, ResolvedPath dstFilename)
      : base(AtlasCommandId.AddBitmapFont)
    {
      if (!elementConfig.IsValid)
        throw new ArgumentException("invalid ElementConfig", nameof(elementConfig));
      if (outputFontFormats != null && outputFontFormats.Count <= 0)
        g_logger.Trace("Created without any output format");

      ElementConfig = elementConfig;
      FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
      Type = type;
      TweakConfig = tweakConfig;
      OutputFontFormats = outputFontFormats ?? throw new ArgumentNullException(nameof(outputFontFormats));
      RelativeFontAtlasPath = relativeFontAtlasPath ?? throw new ArgumentNullException(nameof(relativeFontAtlasPath));
      DstFilename = dstFilename ?? throw new ArgumentNullException(nameof(dstFilename));
    }
  }
}
