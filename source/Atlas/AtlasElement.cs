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

namespace TexturePacker.Atlas
{
  public class AtlasElement : IDisposable
  {
    public readonly string SourcePath;
    public readonly SafeImage SourceImage;
    /// <summary>
    /// The original untrimmed/uncropped size.
    /// Beware this might be larger than the currently sourceImage (since the sourceImage is cropped)
    /// </summary>
    public readonly PxSize2D OriginalSizePx;
    public readonly TrimInfo SourceTrimInfo;
    public readonly PxRectangle CroppedRectanglePx;
    public readonly UInt16 Dpi;
    public readonly AtlasImageType ImageType;

    /// <summary>
    /// Will be null for non-patch elements
    /// </summary>
    public readonly AtlasElementPatchInfo? PatchInfo;

    public readonly ImmutableArray<PxPoint2> AnchorPoints;

    private bool m_isDisposed;

    /// <summary>
    /// Create a basic atlas element
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="sourceImage"></param>
    /// <param name="sourceTrimInfo"></param>
    /// <param name="dpi"></param>
    /// <param name="patchInfo"></param>
    /// <param name="anchorPoints"></param>
    public AtlasElement(string sourcePath, SafeImage sourceImage, TrimInfo sourceTrimInfo, UInt16 dpi, AtlasImageType imageType,
                        AtlasElementPatchInfo? patchInfo, ImmutableArray<PxPoint2> anchorPoints)
    {
      SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
      SourceImage = sourceImage ?? throw new ArgumentNullException(nameof(sourceImage));

      SourcePath = IOUtil.NormalizePath(SourcePath);
      if (SourcePath.StartsWith('/'))
        throw new ArgumentException($"Can not start with '/' was '{sourcePath}'", nameof(sourcePath));

      if (sourceTrimInfo.RectanglePx.Left < 0 || sourceTrimInfo.RectanglePx.Top < 0 || sourceTrimInfo.RectanglePx.Right > sourceImage.Size.Width ||
          sourceTrimInfo.RectanglePx.Bottom > sourceImage.Size.Height)
      {
        throw new ArgumentOutOfRangeException(nameof(sourceTrimInfo), "sourceTrimInfo.RectanglePx");
      }

      SourceTrimInfo = sourceTrimInfo;
      OriginalSizePx = sourceTrimInfo.RectanglePx.Size + sourceTrimInfo.MarginPx.Sum;
      CroppedRectanglePx = new PxRectangle(0, 0, sourceImage.Size.Width, sourceImage.Size.Height);

      Dpi = dpi;
      ImageType = imageType;
      PatchInfo = patchInfo;
      AnchorPoints = anchorPoints;
    }

    public void Dispose()
    {
      if (m_isDisposed)
        return;
      m_isDisposed = true;
      Dispose(true);
      // Suppress finalization.
      GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      SourceImage.Dispose();
    }
  }
}
