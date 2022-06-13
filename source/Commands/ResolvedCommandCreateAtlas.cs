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

using System;
using System.Collections.Immutable;
using TexturePacker.Commands.Atlas;
using TexturePacker.Config;

namespace TexturePacker.Commands
{
  public class ResolvedCommandCreateAtlas : ResolvedCommand
  {
    public readonly string Name;
    public readonly ResolvedPath SourcePath;
    public readonly AtlasConfig Config;

    public readonly ImmutableArray<ResolvedAtlasCommand> Commands;
    public readonly OutputAtlasFormat OutputFormat;

    public readonly ResolvedPath DstAtlasFilename;

    public ResolvedCommandCreateAtlas(string name, ResolvedPath sourcePath, AtlasConfig atlasConfig,
                                      ResolvedAtlasCommand[] commands, OutputAtlasFormat outputAtlasFormat,
                                      ResolvedPath dstAtlasFilename)
      : this(name, sourcePath, atlasConfig, ImmutableArray.Create(commands), outputAtlasFormat, dstAtlasFilename)
    {
    }

    public ResolvedCommandCreateAtlas(string name, ResolvedPath sourcePath, AtlasConfig atlasConfig,
                                      ImmutableArray<ResolvedAtlasCommand> commands, OutputAtlasFormat outputAtlasFormat,
                                      ResolvedPath dstAtlasFilename)
      : base(CommandId.CreateAtlas)
    {
      NameUtil.ValidatePathName(name);

      Name = name;
      SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));

      Config = atlasConfig;
      Commands = commands;
      OutputFormat = outputAtlasFormat;

      DstAtlasFilename = dstAtlasFilename;
    }
  }
}
