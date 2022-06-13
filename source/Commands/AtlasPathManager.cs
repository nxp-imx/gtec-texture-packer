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
using System.IO;

namespace TexturePacker.Commands
{
  public static class AtlasPathManager
  {
    public static string CreateRelativeAtlasPath(ResolvedPath parentFolderPath, string absFilePath)
    {
      if (parentFolderPath == null)
        throw new ArgumentNullException(nameof(parentFolderPath));
      if (absFilePath == null)
        throw new ArgumentNullException(nameof(absFilePath));
      if (!IOUtil.PathStartsWithDir(absFilePath, parentFolderPath.AbsolutePath))
        throw new ArgumentException($"Path '{absFilePath}' did not start with '{parentFolderPath.AbsolutePath}/' as expected", nameof(absFilePath));

      var atlasPathSpan = absFilePath.AsSpan().Slice(parentFolderPath.AbsolutePath.Length + 1);
      var atlasPath = Path.Combine(Path.GetDirectoryName(atlasPathSpan).ToString(), Path.GetFileNameWithoutExtension(atlasPathSpan).ToString());
      return IOUtil.NormalizePath(atlasPath);
    }

    public static string CreateRelativeFontAtlasPath(ResolvedPath fontFilePath, string overrideName)
    {
      if (fontFilePath == null)
        throw new ArgumentNullException(nameof(fontFilePath));

      var fontPath = overrideName == null ? Path.GetFileNameWithoutExtension(fontFilePath.UnresolvedSourcePath) : overrideName;
      return IOUtil.NormalizePath(fontPath);
    }
  }
}
