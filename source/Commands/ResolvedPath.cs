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

namespace TexturePacker.Commands
{
  public class ResolvedParentPath
  {
    public readonly string AbsolutePath;
    public readonly string RelativeResolvedSourcePath;
    public readonly string RelativeUnresolvedSourcePath;
    public readonly string UnresolvedSourcePath;

    public ResolvedParentPath(string absolutePath, string unresolvedSourcePath, string relativeResolvedSourcePath, string relativeUnresolvedSourcePath)
    {
      AbsolutePath = absolutePath ?? throw new ArgumentNullException(nameof(absolutePath));
      UnresolvedSourcePath = unresolvedSourcePath ?? throw new ArgumentNullException(nameof(unresolvedSourcePath));
      RelativeResolvedSourcePath = relativeResolvedSourcePath ?? throw new ArgumentNullException(nameof(relativeResolvedSourcePath));
      RelativeUnresolvedSourcePath = relativeUnresolvedSourcePath ?? throw new ArgumentNullException(nameof(relativeUnresolvedSourcePath));

      AbsolutePath = IOUtil.NormalizePath(AbsolutePath);
      UnresolvedSourcePath = IOUtil.NormalizePath(UnresolvedSourcePath);
      RelativeResolvedSourcePath = IOUtil.NormalizePath(RelativeResolvedSourcePath);
      RelativeUnresolvedSourcePath = IOUtil.NormalizePath(RelativeUnresolvedSourcePath);
    }

    protected ResolvedParentPath(string absolutePath, string unresolvedSourcePath, ResolvedParentPath parentPath)
    {
      if (absolutePath == null)
        throw new ArgumentNullException(nameof(absolutePath));
      if (unresolvedSourcePath == null)
        throw new ArgumentNullException(nameof(unresolvedSourcePath));
      if (parentPath == null)
        throw new ArgumentNullException(nameof(parentPath));


      if (!IOUtil.PathStartsWithDir(absolutePath, parentPath.AbsolutePath))
        throw new ArgumentException($"absolutePath '{absolutePath}' did not belong to parent path '{parentPath.AbsolutePath}'");
      if (!IOUtil.PathStartsWithDir(unresolvedSourcePath, parentPath.UnresolvedSourcePath))
        throw new ArgumentException($"unresolvedSourcePath '{unresolvedSourcePath}' did not belong to parent path '{parentPath.UnresolvedSourcePath}'");


      AbsolutePath = IOUtil.NormalizePath(absolutePath);
      UnresolvedSourcePath = IOUtil.NormalizePath(unresolvedSourcePath);

      RelativeResolvedSourcePath = parentPath.AbsolutePath;
      RelativeResolvedSourcePath = IOUtil.RemoveStartDirectoryName(absolutePath, parentPath.AbsolutePath);
      RelativeUnresolvedSourcePath = IOUtil.RemoveStartDirectoryName(unresolvedSourcePath, parentPath.UnresolvedSourcePath);
    }
  }

  public sealed class ResolvedPath : ResolvedParentPath
  {
    public readonly ResolvedParentPath ParentPath;

    public ResolvedPath(string absolutePath, string unresolvedSourcePath, string relativeResolvedSourcePath, string relativeUnresolvedSourcePath)
      : base(absolutePath, unresolvedSourcePath, relativeResolvedSourcePath, relativeUnresolvedSourcePath)
    {
      ParentPath = new ResolvedParentPath(IOUtil.GetDirectoryName(absolutePath),
                                          IOUtil.GetDirectoryName(unresolvedSourcePath),
                                          IOUtil.GetDirectoryName(relativeResolvedSourcePath),
                                          IOUtil.GetDirectoryName(relativeUnresolvedSourcePath));
    }


    public ResolvedPath(string absolutePath, string unresolvedSourcePath, ResolvedParentPath parentPath)
      : base(absolutePath, unresolvedSourcePath, parentPath)
    {
      ParentPath = parentPath;

      // Validate
      {
        string combined = IOUtil.Combine(parentPath.AbsolutePath, RelativeResolvedSourcePath);
        if (combined != AbsolutePath)
          throw new Exception($"'{AbsolutePath}' != '{combined}'. Which is constructed from parent '{parentPath.AbsolutePath}' and '{RelativeResolvedSourcePath}'");

        if (!IOUtil.PathStartsWithDir(UnresolvedSourcePath, parentPath.UnresolvedSourcePath))
          throw new Exception($"UnresolvedSourcePath: '{UnresolvedSourcePath}' does not start with '{parentPath.UnresolvedSourcePath}' which is required");

        string combinedUnresolved = IOUtil.Combine(parentPath.UnresolvedSourcePath, RelativeUnresolvedSourcePath);
        if (combinedUnresolved != UnresolvedSourcePath)
          throw new Exception($"'{UnresolvedSourcePath}' != '{combinedUnresolved}'. Which is constructed from parent '{parentPath.UnresolvedSourcePath}' and '{RelativeUnresolvedSourcePath}'");
      }
    }

    internal static ResolvedPath Append(ResolvedPath path, string pathToAppend)
    {
      return new ResolvedPath(path.AbsolutePath + pathToAppend, path.UnresolvedSourcePath + pathToAppend, path.ParentPath);
    }

    public static ResolvedPath GetDirectoryName(ResolvedPath path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof(path));

      string absolutePath = IOUtil.GetDirectoryName(path.AbsolutePath);
      //string relativeResolvedSourcePath = IOUtil.GetDirectoryName(path.RelativeResolvedSourcePath);
      string unresolvedSourcePath = IOUtil.GetDirectoryName(path.UnresolvedSourcePath);

      if (!IOUtil.PathStartsWithDir(absolutePath, path.ParentPath.AbsolutePath))
        throw new NotSupportedException("GetDirectoryName");
      if (!IOUtil.PathStartsWithDir(unresolvedSourcePath, path.ParentPath.UnresolvedSourcePath))
        throw new NotSupportedException("GetDirectoryName");

      return new ResolvedPath(absolutePath, unresolvedSourcePath, path.ParentPath);
    }
  }
}
