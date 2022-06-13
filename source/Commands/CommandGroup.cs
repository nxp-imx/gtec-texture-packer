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

using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TexturePacker.Config;

namespace TexturePacker.Commands
{
  public class CommandGroup
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly string CompanyName;
    public readonly string NamespaceName;
    public readonly UInt32 CreationYear;
    public readonly List<Command> CommandList;
    public readonly PathResolver PathResolver;
    public readonly TexturePackerConfig Config;
    public readonly string SrcPath;
    public readonly string DstPath;

    public CommandGroup(string companyName, string namespaceName, UInt32 creationYear, List<Command> commandList, PathResolver pathResolver, 
                        TexturePackerConfig config, string srcPath, string dstPath)
    {
      CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
      NamespaceName = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
      CreationYear = creationYear;
      CommandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
      PathResolver = pathResolver;
      Config = config;
      SrcPath = srcPath ?? throw new ArgumentNullException(nameof(srcPath));
      DstPath = dstPath ?? throw new ArgumentNullException(nameof(dstPath));

      if (!System.IO.Path.IsPathRooted(srcPath))
        throw new NotSupportedException($"srcPath must be be rooted '{srcPath}'");
      if (!System.IO.Path.IsPathRooted(dstPath))
        throw new NotSupportedException($"dstPath must be be rooted '{dstPath}'");
    }

    public ResolvedCommandGroup Resolve(ImmutableHashSet<string> imageExtensions, bool disableLicenseFiles)
    {
      g_logger.Trace("Resolve");

      var copyManager = new CopyManager(Config.License, disableLicenseFiles);
      var info = new CommandResolveInfo(copyManager, PathResolver, imageExtensions, DstPath);

      var resolvedCommands = new List<ResolvedCommand>(CommandList.Count);
      foreach (var command in CommandList)
      {
        var actionCommandGroup = command as ActionCommandGroup;
        if (actionCommandGroup != null)
        {
          resolvedCommands.AddRange(actionCommandGroup.Resolve(info));
        }
        else
        {
          var actionCommand = command as ActionCommand;
          if (actionCommand != null)
            resolvedCommands.Add(actionCommand.Resolve(info));
        }
      }
      ResolvedCommandCopyFiles copyCommand = copyManager.TryBuildCopyCommand();
      copyManager.ClearCommands();
      if (copyCommand != null)
      {
        resolvedCommands.Add(copyCommand);
      }
      return new ResolvedCommandGroup(CompanyName, NamespaceName, CreationYear, ImmutableArray.Create(resolvedCommands.ToArray()));
    }
  }
}
