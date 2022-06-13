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
using TexturePacker.Config;

namespace TexturePacker.Commands.Atlas
{
  public class AtlasCommandAddImage : AtlasCommand
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly string Path;
    public readonly AtlasElementConfig ElementConfig;
    public readonly UInt16 Dpi;
    public readonly AddNineSlice AddNineSlice;
    public readonly AddComplexPatch AddComplexPatch;
    public readonly AddAnchor AddAnchor;

    /// <summary>
    ///
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="defaultDpi">the fallback that is used if Dpi is zero</param>
    /// <param name="dpi">if this is non zero then the image will be assigned this Dpi</param>
    public AtlasCommandAddImage(AtlasElementConfig elementConfig, string imagePath, UInt16 dpi, AddNineSlice addNineSlice, AddComplexPatch addComplexPatch, AddAnchor addAnchor)
      : base(AtlasCommandId.AddImage)
    {
      if (!elementConfig.IsValid)
        throw new ArgumentException("invalid ElementConfig", nameof(elementConfig));
      if (addNineSlice != null && addComplexPatch != null)
        throw new ArgumentException("must supply either a nineSlice or a complexPatch not both");

      NameUtil.ValidatePathName(imagePath);

      Path = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
      ElementConfig = elementConfig;
      Dpi = dpi;
      AddNineSlice = addNineSlice;
      AddComplexPatch = addComplexPatch;
      AddAnchor = addAnchor;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public override ResolvedAtlasCommand Resolve(in AtlasCommandResolveInfo info)
    {
      var resolvedPath = info.SrcPathResolver.Combine(info.SourcePath, Path);

      g_logger.Trace("- Added image file '{0}'", resolvedPath.AbsolutePath);

      // Analyze the filename
      var filenameInfo = Input.FileNameUtil.AnalyzeFilename(resolvedPath.AbsolutePath, false);
      bool isPatch = filenameInfo.IsPatch;

      UInt16 dpi = Dpi;
      if (filenameInfo.DpiOverride != 0u)
      {
        if (Dpi != 0u && filenameInfo.DpiOverride != Dpi)
          throw new NotSupportedException($"Image name '{resolvedPath.AbsolutePath}' dpi of {filenameInfo.DpiOverride} does not match the set dpi {Dpi}");

        dpi = filenameInfo.DpiOverride;
      }


      // Check for license files
      info.CopyManager.ResolveLicenseFiles(IOUtil.GetDirectoryName(resolvedPath.AbsolutePath), info.DstAtlasPath.AbsolutePath);

      //var resolvedAtlasPath = new ResolvedPath(filenameInfo.NormalizedName, resolvedPath.RelativeResolvedSourcePath, resolvedPath.UnresolvedSourcePath, resolvedPath.ParentPath);

      var resolvedAtlasPath = new ResolvedPath(filenameInfo.NormalizedName, resolvedPath.UnresolvedSourcePath, resolvedPath.ParentPath);

      return new ResolvedAtlasCommandAddImage(new ResolvedImageFile(ElementConfig, resolvedPath, resolvedAtlasPath, isPatch, dpi, AddNineSlice,
                                                                    AddComplexPatch, AddAnchor));
    }
  }
}
