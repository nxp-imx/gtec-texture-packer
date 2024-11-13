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
using System.Collections.Immutable;
using TexturePacker.Config;

namespace TexturePacker.Commands
{
  public class ResolvedImageFile
  {
    private static readonly AddAnchor g_emptyAddAnchor = new AddAnchor(ImmutableArray.Create(Array.Empty<PxPoint2>()));

    public readonly AtlasElementConfig ElementConfig;
    public readonly ResolvedPath Path;
    // The filename that will be used to define the atlas name
    public readonly ResolvedPath AtlasPath;
    public readonly bool IsPatch;
    public readonly UInt16 Dpi;
    public readonly AddNineSlice? AddNineSlice;
    public readonly AddComplexPatch? AddComplexPatch;
    public readonly AddAnchor AddAnchor;

    public ResolvedImageFile(AtlasElementConfig elementConfig, ResolvedPath path, ResolvedPath atlasPath, bool isPatch, UInt16 dpi,
                             AddNineSlice? addNineSlice, AddComplexPatch? addComplexPatch, AddAnchor? addAnchor)
    {
      if (!elementConfig.IsValid)
        throw new ArgumentException("invalid ElementConfig", nameof(elementConfig));
      if (dpi == 0)
        throw new ArgumentException("can not be zero", nameof(dpi));
      Path = path ?? throw new ArgumentNullException(nameof(path));
      AtlasPath = atlasPath ?? throw new ArgumentNullException(nameof(atlasPath));
      IsPatch = isPatch;
      Dpi = dpi;
      ElementConfig = elementConfig;
      AddNineSlice = addNineSlice;
      AddComplexPatch = addComplexPatch;
      AddAnchor = addAnchor != null ? addAnchor : g_emptyAddAnchor;

      if (isPatch && AddNineSlice != null)
      {
        throw new ArgumentException($"'{path.AbsolutePath}' can not be marked as a Patch and have a AddNineSlice section");
      }
      if (isPatch && AddComplexPatch != null)
      {
        throw new ArgumentException($"'{path.AbsolutePath}' can not be marked as a Patch and have a AddComplexPatch section");
      }
      if (AddNineSlice != null && AddComplexPatch != null)
      {
        throw new ArgumentException($"'{path.AbsolutePath}' can not have both a AddNineSlice and AddComplexPatch");
      }
    }

    /// <summary>
    /// Extract the meta data used for validation that ensures that all meta data information is the same for atlas flavors.
    /// </summary>
    /// <returns></returns>
    public ResolvedImageMetaData GetMetaData()
    {
      return new ResolvedImageMetaData(IsPatch, AddNineSlice != null, AddAnchor.Points.Length, AddComplexPatch);
    }
  }
}
