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

using MB.Base;
using MB.Base.MathEx;
using MB.Base.MathEx.Pixel;
using MB.Graphics2.Patch.Advanced;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TexturePacker.Atlas;

namespace TexturePacker
{
  /// <summary>
  /// This is unfortunately needed as imagesharp chokes on zero sized width or heights
  /// </summary>
  public sealed class SafeImage : IDisposable
  {
    public enum ImageFormat
    {
      Png,
      Jpg
    }

    //private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    private bool m_isDisposed;
    private Image<Rgba32> m_image;
    public readonly PxSize2D Size;

    private static readonly Rgba32 PatchFillColor = new Rgba32(0, 0, 0, 255);

    /// <summary>
    /// Workaround imagesharp being annoying by not allowing zero sized images.
    /// </summary>
    public SafeImage()
      : this(new PxSize2D())
    {
    }

    public SafeImage(PxSize2D size)
    {
      if (size.Width > 0 && size.Height > 0)
        m_image = new Image<Rgba32>(size.Width, size.Height);
      else
        m_image = null;
      Size = size;
    }

    public SafeImage(Image<Rgba32> image)
      : this(image, SafeGetSize(image))
    {
    }

    public SafeImage(Image<Rgba32> image, PxSize2D size)
    {
      if (image == null)
        throw new ArgumentNullException(nameof(image));

      Size = size;
      m_image = (size.Width > 0 && size.Height > 0) ? image : null;
    }

    public void Dispose()
    {
      if (m_isDisposed)
        return;
      m_isDisposed = true;
      if (m_image != null)
      {
        m_image.Dispose();
        m_image = null;
      }
    }

    public SafeImage CloneCrop(PxThicknessU cropTrimPx)
    {
      return CloneCrop(new PxRectangle((Int32)cropTrimPx.Left, (Int32)cropTrimPx.Top, Size.Width - (Int32)cropTrimPx.SumX, Size.Height - (Int32)cropTrimPx.SumY));
    }


    public SafeImage CloneCrop(PxRectangle cropRectPx)
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      if (cropRectPx.Left < 0 || cropRectPx.Top < 0 || cropRectPx.Right > Size.Width || cropRectPx.Bottom > Size.Height)
        throw new ArgumentException($"cropRectPx {cropRectPx} is out of bounds", nameof(cropRectPx));

      if (m_image != null && cropRectPx.Width > 0 && cropRectPx.Height > 0)
      {
        var croppedImage = m_image.Clone(i => i.Crop(ToRectangle(cropRectPx)));
        return new SafeImage(croppedImage, cropRectPx.Size);
      }
      return new SafeImage();
    }


    /// <summary>
    /// Calc a trim rectangle
    /// </summary>
    /// <param name="alpahThreshold"></param>
    /// <param name="trimMargin"></param>
    /// <returns></returns>
    public PxRectangle CalcTrimmedImageRect(int alpahThreshold, int trimMargin)
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      return m_image != null ? CalcTrimmedImageRect(m_image, alpahThreshold, trimMargin) : new PxRectangle();
    }

    public AtlasElementPatchInfo TryProcessPatchInfo()
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      if (Size.Width < 3 || Size.Height < 3)
        return null;

      return m_image != null ? ProcessPatchInfo(m_image) : null;
    }



    /// <summary>
    /// Draw the given image to this image
    /// </summary>
    /// <param name="location"></param>
    /// <param name="srcImage"></param>
    public void DrawImage(PxPoint2 dstPositionPx, SafeImage srcImage)
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      if (srcImage == null)
        throw new ArgumentNullException(nameof(srcImage));

      if (dstPositionPx.X < 0 || dstPositionPx.Y < 0)
        throw new ArgumentException($"out of bounds", nameof(dstPositionPx));

      if ((dstPositionPx.X + srcImage.Size.Width) > Size.Width || (dstPositionPx.Y + srcImage.Size.Height) > Size.Height)
        throw new ArgumentException($"drawing a image at that position would be out of bounds", nameof(dstPositionPx));

      if (srcImage.Size.Width <= 0 || srcImage.Size.Height <= 0)
        return; // Nothing to draw

      if (m_image == null)
      {
        // This should never occur due to earlier checks
        throw new Exception("Can not draw to zero sized image");
      }

      var location = new Point(dstPositionPx.X, dstPositionPx.Y);
      try
      {
        Debug.Assert(srcImage.m_image != null);
        m_image.Mutate(img => img.DrawImage(srcImage.m_image, location, 1.0f));
      }
      catch (Exception)
      {
        throw;
      }
    }

    public void Extrude(PxRectangle rectanglePx, int extrude)
    {
      if (extrude < 0)
        throw new ArgumentException($"{nameof(extrude)} must be positive");
      if (extrude > m_image.Width && extrude > m_image.Height)
        throw new ArgumentException($"{nameof(extrude)} must fit inside the image");
      if (!(rectanglePx.Left >= extrude && rectanglePx.Right <= (m_image.Width - extrude) && rectanglePx.Top >= extrude && rectanglePx.Bottom <= (m_image.Height - extrude)))
      {
        throw new ArgumentException($"{nameof(rectanglePx)} and {nameof(extrude)} must fit inside the image");
      }
      if (extrude == 0)
        return;

      var span = m_image.GetLegacyPixelSpan();
      var srcSpanRangeX = new SpanRange(rectanglePx.Left, rectanglePx.Width);
      var srcSpanRangeY = new SpanRange(rectanglePx.Top, rectanglePx.Height);
      int stride = m_image.Width;
      // Top
      ExtrudeY(span, stride, rectanglePx.Top - extrude, srcSpanRangeX, rectanglePx.Top, extrude);
      // Bottom
      ExtrudeY(span, stride, rectanglePx.Bottom, srcSpanRangeX, rectanglePx.Bottom - 1, extrude);
      // Left
      ExtrudeX(span, stride, rectanglePx.Left - extrude, rectanglePx.Left, srcSpanRangeY, extrude);
      // Right
      ExtrudeX(span, stride, rectanglePx.Right, rectanglePx.Right - 1, srcSpanRangeY, extrude);

      var colorTL = span[(rectanglePx.Top * stride) + rectanglePx.Left];
      var colorTR = span[(rectanglePx.Top * stride) + rectanglePx.Right - 1];
      var colorBL = span[((rectanglePx.Bottom - 1) * stride) + rectanglePx.Left];
      var colorBR = span[((rectanglePx.Bottom - 1) * stride) + rectanglePx.Right - 1];

      Fill(span, stride, new PxRectangle(rectanglePx.TopLeft - new PxPoint2(extrude, extrude), new PxPoint2(extrude, extrude)), colorTL);
      Fill(span, stride, new PxRectangle(rectanglePx.TopRight - new PxPoint2(0, extrude), new PxPoint2(extrude, extrude)), colorTR);
      Fill(span, stride, new PxRectangle(rectanglePx.BottomLeft - new PxPoint2(extrude, 0), new PxPoint2(extrude, extrude)), colorBL);
      Fill(span, stride, new PxRectangle(rectanglePx.BottomRight, new PxPoint2(extrude, extrude)), colorBR);
    }

    private static void Fill(Span<Rgba32> span, int stride, PxRectangle dstFillRectPx, Rgba32 color)
    {
      int dstOffset = (dstFillRectPx.Top * stride) + dstFillRectPx.Left;
      int dstOffsetEnd = (dstFillRectPx.Bottom * stride);
      while (dstOffset < dstOffsetEnd)
      {
        for (int x = 0; x < dstFillRectPx.Width; ++x)
        {
          span[dstOffset + x] = color;
        }
        dstOffset += stride;
      }
    }

    private static void ExtrudeX(Span<Rgba32> span, int stride, int dstX, int srcX, SpanRange srcY, int extrude)
    {
      int srcOffset = (srcY.Start * stride) + srcX;
      int dstOffset = (srcY.Start * stride) + dstX;
      int dstOffsetEnd = dstOffset + (stride * srcY.Length);
      while (dstOffset < dstOffsetEnd)
      {
        for (int x = 0; x < extrude; ++x)
        {
          span[dstOffset + x] = span[srcOffset];
        }
        srcOffset += stride;
        dstOffset += stride;
      }
    }

    private static void ExtrudeY(Span<Rgba32> span, int stride, int dstY, SpanRange srcX, int srcY, int extrude)
    {
      int srcOffset = (srcY * stride) + srcX.Start;
      int dstOffset = (stride * dstY) + srcX.Start;
      int dstOffsetEnd = dstOffset + (stride * extrude);
      while (dstOffset < dstOffsetEnd)
      {
        for (int x = 0; x < srcX.Length; ++x)
        {
          span[dstOffset + x] = span[srcOffset + x];
        }
        dstOffset += stride;
      }
    }

    public void Premultiply()
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));

      //var memoryGroup = m_image.GetPixelMemoryGroup<Rgba32>();
      //for (int memIndex = 0; memIndex < memoryGroup.Count; ++memIndex)
      //{
      //var span = memoryGroup[memIndex].Span;

      var span = m_image.GetLegacyPixelSpan();
      for (int i = 0; i < span.Length; ++i)
      {
        var color = span[i].ToVector4();
        color.X *= color.W;
        color.Y *= color.W;
        color.Z *= color.W;
        span[i] = new Rgba32(color);
        //}
      }
    }


    public byte[] ToByteArray()
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      return m_image != null ? System.Runtime.InteropServices.MemoryMarshal.AsBytes(m_image.GetLegacyPixelSpan()).ToArray() : Array.Empty<byte>();
    }

    public void Save(string dstFilename)
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      if (dstFilename == null)
        throw new ArgumentNullException(nameof(dstFilename));
      if (m_image == null)
        throw new NotSupportedException("Can not save a zero sized image");

      m_image.Save(dstFilename);
    }


    public void Save(System.IO.Stream dstStream, ImageFormat format)
    {
      if (m_isDisposed)
        throw new ObjectDisposedException(nameof(SafeImage));
      if (dstStream == null)
        throw new ArgumentNullException(nameof(dstStream));
      if (m_image == null)
        throw new NotSupportedException("Can not save a zero sized image");

      m_image.Save(dstStream, ToIImageFormat(format));
    }

    private static IImageEncoder ToIImageFormat(ImageFormat format)
    {
      switch (format)
      {
        case ImageFormat.Png:
          return new SixLabors.ImageSharp.Formats.Png.PngEncoder();
        case ImageFormat.Jpg:
          return new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
        default:
          throw new NotSupportedException($"Unsupported format {format}");
      }
    }

    public static bool IsDuplicate(SafeImage srcImage1, SafeImage srcImage2)
    {
      if (srcImage1 == srcImage2)
        return true;
      if (srcImage1 == null || srcImage2 == null)
        return false;

      if (srcImage1.Size != srcImage2.Size)
        return false;

      // Empty bitmaps are considered duplicates
      if (srcImage1.Size.Width <= 0 || srcImage1.Size.Height <= 0)
        return true;

      // Finally check all pixels
      Debug.Assert(srcImage1.m_image != null);
      Debug.Assert(srcImage2.m_image != null);

      var srcSpan1 = srcImage1.m_image.GetLegacyPixelSpan();
      var srcSpan2 = srcImage2.m_image.GetLegacyPixelSpan();
      if (srcSpan1.Length != srcSpan2.Length)
        return false;

      for (int i = 0; i < srcSpan1.Length; ++i)
      {
        if (srcSpan1[i] != srcSpan2[i])
          return false;
      }
      return true;
    }


    private static PxSize2D SafeGetSize(Image<Rgba32> image)
    {
      return image != null ? new PxSize2D(image.Width, image.Height) : new PxSize2D();
    }


    private static bool IsColumnTransparent(Span<Rgba32> pixelSpan, int x, int width, int height, int alpahThreshold)
    {
      for (int y = 0; y < height; ++y)
      {
        if (pixelSpan[x + (y * width)].A >= alpahThreshold)
          return false;
      }
      return true;
    }


    private static bool IsRowTransparent(Span<Rgba32> pixelRowSpan, int alpahThreshold)
    {
      for (int x = 0; x < pixelRowSpan.Length; ++x)
      {
        if (pixelRowSpan[x].A >= alpahThreshold)
          return false;
      }
      return true;
    }

    public bool IsOpaque(in PxRectangle srcRectPx)
    {
      if (m_image != null)
      {
        int left = srcRectPx.Left;
        int top = srcRectPx.Top;
        int bottom = srcRectPx.Bottom;
        int right = srcRectPx.Right;
        for (int y = top; y < bottom; ++y)
        {
          Span<Rgba32> row = m_image.GetPixelRowSpan(y);
          for (int x = left; x < right; ++x)
          {
            if (row[x].A != 0xFF)
            {
              return false;
            }
          }
        }
      }
      return true;
    }

    private static int IndexOfFirstNonTransparentColumn(Image<Rgba32> image, int alpahThreshold, int startIndex = 0)
    {
      Debug.Assert(image != null);
      Debug.Assert(image.Width >= 0);
      Debug.Assert(startIndex >= 0);
      Debug.Assert(startIndex <= image.Width);

      Span<Rgba32> pixelSpan = image.GetLegacyPixelSpan();
      int width = image.Width;
      int height = image.Height;
      for (int x = startIndex; x < width; ++x)
      {
        if (!IsColumnTransparent(pixelSpan, x, width, height, alpahThreshold))
          return x;
      }
      return -1;
    }

    private static int LastIndexOfFirstNonTransparentColumn(Image<Rgba32> image, int alpahThreshold)
    {
      if (image.Width < 1)
        return -1;
      return LastIndexOfFirstNonTransparentColumn(image, alpahThreshold, image.Width - 1);
    }

    private static int LastIndexOfFirstNonTransparentColumn(Image<Rgba32> image, int alpahThreshold, int startIndex)
    {
      Debug.Assert(image != null);
      Debug.Assert(image.Width >= 0);
      Debug.Assert(startIndex >= 0);
      Debug.Assert(startIndex <= image.Width);

      Span<Rgba32> pixelSpan = image.GetLegacyPixelSpan();
      int width = image.Width;
      int height = image.Height;
      for (int x = startIndex; x >= 0; --x)
      {
        if (!IsColumnTransparent(pixelSpan, x, width, height, alpahThreshold))
          return x;
      }
      return -1;
    }

    private static int IndexOfFirstNonTransparentRow(Image<Rgba32> image, int alpahThreshold, int startIndex = 0)
    {
      Debug.Assert(image != null);
      Debug.Assert(image.Height >= 0);
      Debug.Assert(startIndex >= 0);
      Debug.Assert(startIndex <= image.Height);

      for (int y = startIndex; y < image.Height; ++y)
      {
        if (!IsRowTransparent(image.GetPixelRowSpan(y), alpahThreshold))
          return y;
      }
      return -1;
    }

    private static int LastIndexOfFirstNonTransparentRow(Image<Rgba32> image, int alpahThreshold)
    {
      if (image.Height < 1)
        return -1;
      return LastIndexOfFirstNonTransparentRow(image, alpahThreshold, image.Height - 1);
    }

    private static int LastIndexOfFirstNonTransparentRow(Image<Rgba32> image, int alpahThreshold, int startIndex)
    {
      Debug.Assert(image != null);
      Debug.Assert(image.Height >= 0);
      Debug.Assert(startIndex >= 0);
      Debug.Assert(startIndex <= image.Height);

      for (int y = startIndex; y >= 0; --y)
      {
        if (!IsRowTransparent(image.GetPixelRowSpan(y), alpahThreshold))
          return y;
      }
      return -1;
    }

    private static PxRectangle CalcTrimmedImageRect(Image<Rgba32> image, int alpahThreshold, int trimMargin)
    {
      Debug.Assert(trimMargin >= 0);
      int topIndex = IndexOfFirstNonTransparentRow(image, alpahThreshold);
      if (topIndex < 0)
        return new PxRectangle();

      int bottomIndex = LastIndexOfFirstNonTransparentRow(image, alpahThreshold);
      Debug.Assert(bottomIndex >= topIndex);
      int leftIndex = IndexOfFirstNonTransparentColumn(image, alpahThreshold);
      Debug.Assert(leftIndex >= 0);
      int rightIndex = LastIndexOfFirstNonTransparentColumn(image, alpahThreshold);
      Debug.Assert(rightIndex >= leftIndex);

      Debug.Assert(rightIndex < image.Width);
      Debug.Assert(bottomIndex < image.Height);
      ++rightIndex;
      ++bottomIndex;

      // apply trim margin
      leftIndex = Math.Max(leftIndex - trimMargin, 0);
      topIndex = Math.Max(topIndex - trimMargin, 0);
      rightIndex = Math.Min(rightIndex + trimMargin, image.Width);
      bottomIndex = Math.Min(bottomIndex + trimMargin, image.Height);

      return PxRectangle.FromLeftTopRightBottom(leftIndex, topIndex, rightIndex, bottomIndex);
    }

    private static Rectangle ToRectangle(PxRectangle value)
    {
      return new Rectangle(value.X, value.Y, value.Width, value.Height);
    }

    private static AtlasElementPatchInfo ProcessPatchInfo(Image<Rgba32> image)
    {
      Debug.Assert(image.Width >= 3 && image.Height >= 3);

      var topSpan = image.GetPixelRowSpan(0);
      var bottomSpan = image.GetPixelRowSpan(image.Height - 1);
      topSpan = topSpan.Slice(1, topSpan.Length - 2);
      bottomSpan = bottomSpan.Slice(1, bottomSpan.Length - 2);

      var scaleSpansX = FindFilledPixelSpans(topSpan);
      var contentSpanX = FindFilledPixelSpans(bottomSpan);

      Span<Rgba32> fullImageSpan = image.GetLegacyPixelSpan();
      int spanStride = image.Width;
      var scaleSpansY = FindFilledPixelRowSpans(fullImageSpan, 0, 1, image.Height - 1, spanStride);
      var contentSpanY = FindFilledPixelRowSpans(fullImageSpan, image.Width - 1, 1, image.Height - 1, spanStride);

      UInt32 imageWidth = UncheckedNumericCast.ToUInt32(image.Width - 2);
      UInt32 imageHeight = UncheckedNumericCast.ToUInt32(image.Height - 2);
      PxSize2D sizePx = PxUncheckedTypeConverter.ToPxSize2D(new PxExtent2D(imageWidth, imageHeight));
      ImmutableComplexPatch patch = ImmutablePatchHelper.CreateTransparentComplexPatch(scaleSpansX, scaleSpansY, contentSpanX, contentSpanY, sizePx);
      Debug.Assert(contentSpanX.Count < 1 || contentSpanX[contentSpanX.Count - 1].End <= sizePx.Width);
      Debug.Assert(contentSpanY.Count < 1 || contentSpanY[contentSpanY.Count - 1].End <= sizePx.Height);
      Debug.Assert(patch.ContentSpans.CountX == contentSpanX.Count);
      Debug.Assert(patch.ContentSpans.CountY == contentSpanY.Count);
      Debug.Assert(patch.ContentSpans.CountX < 1 || patch.ContentSpans.AsSpanX()[patch.ContentSpans.CountX - 1].End <= sizePx.Width);
      Debug.Assert(patch.ContentSpans.CountY < 1 || patch.ContentSpans.AsSpanY()[patch.ContentSpans.CountY - 1].End <= sizePx.Height);
      return new AtlasElementPatchInfo(patch, true, false);
    }

    private static List<SpanRangeU> FindFilledPixelRowSpans(Span<Rgba32> fullImageSpan, int x, int startY, int lengthY, int stride)
    {
      var foundSpans = new List<SpanRangeU>();
      bool inSpan = false;
      int spanStartIndex = 0;
      for (int i = startY; i < lengthY; ++i)
      {
        bool isFilledPixel = (fullImageSpan[x + (i * stride)] == PatchFillColor);
        if (!inSpan)
        {
          if (isFilledPixel)
          {
            inSpan = true;
            spanStartIndex = i;
          }
        }
        else if (!isFilledPixel)
        {
          // Span end
          inSpan = false;
          foundSpans.Add(new SpanRangeU((UInt32)(spanStartIndex - startY), (UInt32)(i - spanStartIndex)));
        }
      }

      // Close any open spans
      if (inSpan)
      {
        foundSpans.Add(new SpanRangeU((UInt32)(spanStartIndex - startY), (UInt32)(lengthY - spanStartIndex)));
      }
      return foundSpans;
    }

    private static List<SpanRangeU> FindFilledPixelSpans(Span<Rgba32> span)
    {
      var foundSpans = new List<SpanRangeU>();
      bool inSpan = false;
      int spanStartIndex = 0;
      for (int i = 0; i < span.Length; ++i)
      {
        bool isFilledPixel = (span[i] == PatchFillColor);
        if (!inSpan)
        {
          if (isFilledPixel)
          {
            inSpan = true;
            spanStartIndex = i;
          }
        }
        else if (!isFilledPixel)
        {
          // Span end
          inSpan = false;
          foundSpans.Add(new SpanRangeU((UInt32)spanStartIndex, (UInt32)(i - spanStartIndex)));
        }
      }

      // Close any open spans
      if (inSpan)
      {
        foundSpans.Add(new SpanRangeU((UInt32)spanStartIndex, (UInt32)(span.Length - spanStartIndex)));
      }
      return foundSpans;
    }
  }

  internal static class LegacyImage
  {
    public static Span<Rgba32> GetLegacyPixelSpan(this Image<Rgba32> image)
    {
      if (image == null)
        throw new ArgumentNullException(nameof(image));

      //return image.GetPixelSpan<Rgba32>();
      var memoryGroup = image.GetPixelMemoryGroup<Rgba32>();
      if (memoryGroup.Count != 1)
        throw new NotSupportedException("we expect one memory group");

      return memoryGroup[0].Span;
    }
  }
}
