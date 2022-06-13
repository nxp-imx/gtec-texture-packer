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
using TexturePacker.Input;

namespace TexturePacker.Commands
{
  public sealed class CommandCreateAtlas : ActionCommand
  {

    private static readonly IFormatProvider g_invariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    public const string AtlasSrcPathVariable = "Atlas.SrcPath";
    public const string AtlasDstPathVariable = "Atlas.DstPath";
    public const string AtlasDefaultDpVariable = "Atlas.DefaultDpi";

    private static readonly string PathAtlasSrcPathVariable = $"${{{AtlasSrcPathVariable}}}";
    private static readonly string PathAtlasDstPathVariable = $"${{{AtlasDstPathVariable}}}";

    public readonly string AtlasFileSourceDirectoryPath;
    public readonly string Name;
    public readonly string SmartName;
    public readonly string SourcePath;
    public readonly string DstPath;
    public readonly string LicenseFile;
    public readonly AtlasConfig Config;

    public readonly ImmutableArray<AtlasCommand> Commands;

    public readonly OutputAtlasFormat OutputFormat;

    public CommandCreateAtlas(string atlasFileSourceDirectoryPath, string name, string sourcePath, string licenseFile, AtlasConfig atlasConfig,
                              AtlasCommand[] commands, OutputAtlasFormat outputAtlasFormat)
      : this(atlasFileSourceDirectoryPath, name, sourcePath, licenseFile, atlasConfig, ImmutableArray.Create(commands), outputAtlasFormat)
    {
    }

    public CommandCreateAtlas(string atlasFileSourceDirectoryPath, string name, string sourcePath, string licenseFile, AtlasConfig atlasConfig,
                              ImmutableArray<AtlasCommand> commands, OutputAtlasFormat outputAtlasFormat)
      : base(CommandId.CreateAtlas)
    {
      if (licenseFile != null)
        NameUtil.ValidatePathName(licenseFile);
      if (sourcePath == null)
        throw new ArgumentNullException(nameof(sourcePath));

      if (sourcePath.Length == 0)
      {
        sourcePath = PathAtlasSrcPathVariable;
      }
      else
      {
        if (sourcePath.Contains(PathAtlasSrcPathVariable, StringComparison.Ordinal))
          throw new NotSupportedException($"SourcePath can not contain '{PathAtlasSrcPathVariable}");
        if (sourcePath.Contains(PathAtlasDstPathVariable, StringComparison.Ordinal))
          throw new NotSupportedException($"SourcePath can not contain '{PathAtlasDstPathVariable}");
        NameUtil.ValidatePathName(sourcePath);
        sourcePath = $"{PathAtlasSrcPathVariable}/{sourcePath}";
      }
      DstPath = $"{PathAtlasDstPathVariable}";

      AtlasFileSourceDirectoryPath = atlasFileSourceDirectoryPath ?? throw new ArgumentNullException(nameof(atlasFileSourceDirectoryPath));

      var smartNameInfo = FileNameUtil.AnalyzeName(name, false);

      Name = name;
      SmartName = smartNameInfo.DpiOverride == 0 ? name : $"{smartNameInfo.NormalizedName}_${{Atlas.DefaultDpi}}dpi";
      SourcePath = sourcePath;
      LicenseFile = licenseFile; // Can be null
      Config = atlasConfig;
      Commands = commands;
      OutputFormat = outputAtlasFormat;
    }

    public override ResolvedCommand Resolve(in CommandResolveInfo info)
    {
      string strDpi = Config.Element.DefaultDpi.ToString(g_invariantCulture);
      var srcPathResolver = new PathResolver(info.PathResolver);
      srcPathResolver.AddComplexVariable(AtlasSrcPathVariable, AtlasFileSourceDirectoryPath);
      srcPathResolver.AddComplexVariable(AtlasDefaultDpVariable, strDpi);

      var dstPathResolver = new PathResolver(info.PathResolver);
      dstPathResolver.AddComplexVariable(AtlasDstPathVariable, info.DstPath);
      dstPathResolver.AddComplexVariable(AtlasDefaultDpVariable, strDpi);


      var resolvedSourcePath = srcPathResolver.ResolvePath(SourcePath);
      var resolvedDstPath = dstPathResolver.ResolvePath(DstPath);
      var resolvedDstAtlasPath = dstPathResolver.Combine(resolvedDstPath, SmartName);

      if (LicenseFile != null)
      {
        var dstPath = IOUtil.GetDirectoryName(resolvedDstAtlasPath.AbsolutePath);
        var resolvedLicensePath = srcPathResolver.ResolvePath(IOUtil.Combine(SourcePath, LicenseFile));
        var dstLicenseFile = IOUtil.Combine(dstPath, System.IO.Path.GetFileName(LicenseFile));
        info.CopyManager.AddMasterLicenseFile(resolvedLicensePath.AbsolutePath, dstLicenseFile);
      }

      var resolvedCommands = new ResolvedAtlasCommand[Commands.Length];
      var atlasResolveInfo = new AtlasCommandResolveInfo(info.CopyManager, srcPathResolver, dstPathResolver, info.ImageExtensions, resolvedSourcePath,
                                                         resolvedDstPath, resolvedDstAtlasPath);
      for (int i = 0; i < resolvedCommands.Length; ++i)
      {
        resolvedCommands[i] = Commands[i].Resolve(atlasResolveInfo);
      }

      return new ResolvedCommandCreateAtlas(Name, resolvedSourcePath, Config, resolvedCommands, OutputFormat, resolvedDstAtlasPath);
    }
  }
}
