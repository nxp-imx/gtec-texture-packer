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
using MB.Base.Container;
using MB.Base.MathEx;
using MB.Base.MathEx.Pixel;
using MB.Graphics2.Patch.Advanced;
using MB.Graphics2.TextureAtlas.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TexturePacker.Atlas;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace TexturePacker
{
  public static class ImmutablePatchHelper
  {
    public static ImmutableComplexPatch CreateTransparentComplexPatch(List<SpanRangeU> scaleSpansX, List<SpanRangeU> scaleSpansY,
                                                                      List<SpanRangeU> contentSpanX, List<SpanRangeU> contentSpanY,
                                                                      PxSize2D imageSize)
    {
      var slicesX = ToSlices(scaleSpansX, imageSize.Width);
      var slicesY = ToSlices(scaleSpansY, imageSize.Height);
      var spansX = ToSpans(contentSpanX, imageSize.Width);
      var spansY = ToSpans(contentSpanY, imageSize.Height);

      return ImmutableComplexPatchUtil.CreateTransparentComplexPatch(slicesX, slicesY, spansX, spansY, PatchFlags.None);
    }

    //public static ImmutableComplexPatch CreateComplexPatch(List<SpanRangeU> scaleSpansX, List<SpanRangeU> scaleSpansY,
    //                                                               PxThicknessU contentMarginPx, PxSize2D imageSize)
    //{
    //  if (scaleSpansX == null)
    //    throw new ArgumentNullException(nameof(scaleSpansX));
    //  if (scaleSpansY == null)
    //    throw new ArgumentNullException(nameof(scaleSpansY));
    //  var slicesX = ToSlices(scaleSpansX, imageSize.Width);
    //  var slicesY = ToSlices(scaleSpansY, imageSize.Height);

    //  // Generate the content span corrosponding to the content margin
    //  var spans = new ImmutableContentSpan[1 + 1];
    //  var spansX = spans.AsSpan(0, 1);
    //  var spansY = spans.AsSpan(1, 1);
    //  spansX[0] = ImmutableContentSpan.FromNearFar(NumericCast.ToUInt16(contentMarginPx.Left),
    //                                              NumericCast.ToUInt16(imageSize.Width - contentMarginPx.Right));
    //  spansY[0] = ImmutableContentSpan.FromNearFar(NumericCast.ToUInt16(contentMarginPx.Top),
    //                                              NumericCast.ToUInt16(imageSize.Height - contentMarginPx.Bottom));

    //  return ImmutablePatchUtil.CreateComplexPatch(slicesX, slicesY, spansX, spansY, PatchFlags.None);
    //}


    public static ImmutableComplexPatch CreateTransparentComplexPatch(ImmutableComplexPatchSlice[] sliceArrayX, ImmutableComplexPatchSlice[] sliceArrayY,
                                                                      ImmutableContentSlice[] contentSliceArrayX, ImmutableContentSlice[] contentSliceArrayY,
                                                                      ComplexPatchFlags flags)
    {
      if (sliceArrayX == null)
        throw new ArgumentNullException(nameof(sliceArrayX));
      if (sliceArrayY == null)
        throw new ArgumentNullException(nameof(sliceArrayY));
      if (contentSliceArrayX == null)
        throw new ArgumentNullException(nameof(contentSliceArrayX));
      if (contentSliceArrayY == null)
        throw new ArgumentNullException(nameof(contentSliceArrayY));
      if (sliceArrayX[sliceArrayX.Length - 1].Position != contentSliceArrayX[contentSliceArrayX.Length - 1].Position)
        throw new Exception($"The slice array content slice array must end at the same position. '{ToString(sliceArrayX)}' did not end at the same position as '{ToString(contentSliceArrayX)}'");
      if (sliceArrayY[sliceArrayY.Length - 1].Position != contentSliceArrayY[contentSliceArrayY.Length - 1].Position)
        throw new Exception($"The slice array content slice array must end at the same position. '{ToString(sliceArrayY)}' did not end at the same position as '{ToString(contentSliceArrayY)}'");

      PatchFlags patchFlags = ToPatchFlags(flags);

      ReadOnlySpan<ImmutableContentSpan> contentSpanArrayX = ToImmutableContentSpan(contentSliceArrayX);
      ReadOnlySpan<ImmutableContentSpan> contentSpanArrayY = ToImmutableContentSpan(contentSliceArrayY);

      return ImmutableComplexPatchUtil.CreateTransparentComplexPatch(sliceArrayX, sliceArrayY, contentSpanArrayX, contentSpanArrayY, patchFlags);
    }


    private static ReadOnlySpan<ImmutableContentSpan> ToImmutableContentSpan(ImmutableContentSlice[] slices)
    {
      Debug.Assert(slices != null);
      Debug.Assert(slices.Length > 1);

      var span = new ImmutableContentSpan[slices.Length - 1];
      int dstIndex = 0;
      for (int i = 0; i < slices.Length - 1; ++i)
      {
        ref readonly var slice = ref slices[i];
        if (slice.Flags.IsFlagged(ContentSliceFlags.Content))
        {
          span[dstIndex] = new ImmutableContentSpan(slice.Position, NumericCast.ToUInt16(slices[i + 1].Position - slice.Position));
          ++dstIndex;
        }
      }
      return span.AsSpan(0, dstIndex);
    }

    private static PatchFlags ToPatchFlags(ComplexPatchFlags flags)
    {
      PatchFlags result = PatchFlags.None;
      if (flags.IsFlagged(ComplexPatchFlags.MirrorX))
      {
        result |= PatchFlags.MirrorX;
        flags &= ~ComplexPatchFlags.MirrorX;
      }
      if (flags.IsFlagged(ComplexPatchFlags.MirrorY))
      {
        result |= PatchFlags.MirrorY;
        flags &= ~ComplexPatchFlags.MirrorY;
      }
      if (flags != ComplexPatchFlags.None)
        throw new Exception($"Failed to convert flag: {flags}");
      return result;
    }

    private static string ToString(ImmutableComplexPatchSlice[] sliceArray)
    {
      return string.Join(',', sliceArray);
    }

    private static string ToString(ImmutableContentSlice[] contentSliceArray)
    {
      return string.Join(',', contentSliceArray);
    }

    private static ReadOnlySpan<ImmutableComplexPatchSlice> ToSlices(List<SpanRangeU> spans, int size)
    {
      if (spans == null)
        throw new ArgumentNullException(nameof(spans));
      if (spans.Count == 0)
        return new ReadOnlySpan<ImmutableComplexPatchSlice>();

      long lastPosition = -1;

      var result = new List<ImmutableComplexPatchSlice>(spans.Count);
      if (spans[0].Start != 0)
        result.Add(new ImmutableComplexPatchSlice(0, ComplexPatchSliceFlags.None));

      for (int i = 0; i < spans.Count - 1; ++i)
      {
        if (spans[i].Start < lastPosition)
          throw new Exception("internal error, invalid span");

        result.Add(new ImmutableComplexPatchSlice(NumericCast.ToUInt16(spans[i].Start), ComplexPatchSliceFlags.Scale));
        if (spans[i].End < spans[i + 1].Start)
          result.Add(new ImmutableComplexPatchSlice(NumericCast.ToUInt16(spans[i].End), ComplexPatchSliceFlags.None));

        lastPosition = spans[i].End;
      }
      // Add the last span entry
      result.Add(new ImmutableComplexPatchSlice(NumericCast.ToUInt16(spans[spans.Count - 1].Start), ComplexPatchSliceFlags.Scale));
      result.Add(new ImmutableComplexPatchSlice(NumericCast.ToUInt16(spans[spans.Count - 1].End), ComplexPatchSliceFlags.None));
      // Add a terminator if needed
      if (spans[spans.Count - 1].End < size)
        result.Add(new ImmutableComplexPatchSlice(NumericCast.ToUInt16(size), ComplexPatchSliceFlags.None));

      return result.ToArray();
    }

    private static ReadOnlySpan<ImmutableContentSpan> ToSpans(List<SpanRangeU> spans, int size)
    {
      if (spans == null)
        throw new ArgumentNullException(nameof(spans));
      if (spans.Count == 0)
        return new ReadOnlySpan<ImmutableContentSpan>();

      var result = new List<ImmutableContentSpan>(spans.Count);
      long lastPosition = -1;

      for (int i = 0; i < spans.Count; ++i)
      {
        if (spans[i].Start < lastPosition)
          throw new Exception("internal error, invalid span");

        if (spans[i].Start >= size || spans[i].End > size)
          throw new Exception("invalid span");

        result.Add(new ImmutableContentSpan(NumericCast.ToUInt16(spans[i].Start), NumericCast.ToUInt16(spans[i].Length)));

        lastPosition = spans[i].End;
      }

      return result.ToArray();
    }

    /// <summary>
    /// Return true if this complex patch can be simplified to a nineslice
    /// </summary>
    /// <param name="patch"></param>
    /// <returns></returns>
    public static bool IsNineSlicePatch(in ImmutableComplexPatch patch)
    {
      var slicesX = patch.Slices.AsSpanX();
      var slicesY = patch.Slices.AsSpanY();
      var spansX = patch.ContentSpans.AsSpanX();
      var spansY = patch.ContentSpans.AsSpanY();
      return (slicesX.Length == 4 && slicesY.Length == 4 &&
             (slicesX[0].Position == 0 && !slicesX[0].Flags.IsFlagged(ComplexPatchSliceFlags.Scale) &&
              slicesX[1].Flags.IsFlagged(ComplexPatchSliceFlags.Scale) && !slicesX[2].Flags.IsFlagged(ComplexPatchSliceFlags.Scale) &&
             !slicesX[3].Flags.IsFlagged(ComplexPatchSliceFlags.Scale)) &&
             (slicesY[0].Position == 0 && !slicesY[0].Flags.IsFlagged(ComplexPatchSliceFlags.Scale) &&
              slicesY[1].Flags.IsFlagged(ComplexPatchSliceFlags.Scale) && !slicesY[2].Flags.IsFlagged(ComplexPatchSliceFlags.Scale) &&
             !slicesY[3].Flags.IsFlagged(ComplexPatchSliceFlags.Scale))) &&
             (spansX.Length == 1 && spansY.Length == 1);
    }

    public static AtlasNineSliceInfo ProcessComplexPatchAsNineSlice(AtlasElementPatchInfo patchInfo, PxSize2D imageSize, string debugName)
    {
      if (patchInfo == null)
        throw new ArgumentNullException(nameof(patchInfo));
      Debug.Assert(ImmutablePatchHelper.IsNineSlicePatch(patchInfo.ComplexPatch));

      var complexPatch = patchInfo.ComplexPatch;

      UInt32 imageWidthPx = UncheckedNumericCast.ToUInt32(imageSize.Width);
      UInt32 imageHeightPx = UncheckedNumericCast.ToUInt32(imageSize.Height);

      ReadOnlySpan<ImmutableComplexPatchSlice> slicesX = complexPatch.Slices.AsSpanX();
      ReadOnlySpan<ImmutableComplexPatchSlice> slicesY = complexPatch.Slices.AsSpanY();
      ReadOnlySpan<ImmutableContentSpan> spansX = complexPatch.ContentSpans.AsSpanX();
      ReadOnlySpan<ImmutableContentSpan> spansY = complexPatch.ContentSpans.AsSpanY();

      Debug.Assert(spansX.Length == 1);
      Debug.Assert(spansY.Length == 1);

      if ((slicesX[0].Position != 0 || (slicesX[1].Position <= 0 || slicesX[1].Position > imageWidthPx) ||
           (slicesX[2].Position <= slicesX[1].Position || slicesX[2].Position > imageWidthPx) || slicesX[3].Position != imageWidthPx) ||
          (slicesY[0].Position != 0 || (slicesY[1].Position <= 0 || slicesY[1].Position > imageHeightPx) ||
           (slicesY[2].Position <= slicesY[1].Position || slicesY[2].Position > imageHeightPx) || slicesY[3].Position != imageHeightPx))
      {
        throw new Exception($"internal patch error {debugName}. Width {imageWidthPx} Height {imageHeightPx} SpansX: {string.Join(',', slicesX.ToArray())} SpansY: {string.Join(',', slicesY.ToArray())}");
      }
      if (!patchInfo.AllowContentMarginToExceedImageBoundary && (spansX[0].End > imageWidthPx || spansY[0].End > imageHeightPx))
      {
        throw new Exception($"internal patch error {debugName}. Width {imageWidthPx} Height {imageHeightPx} SpansX: {string.Join(',', spansX.ToArray())} SpansY: {string.Join(',', spansY.ToArray())}");
      }

      UInt32 leftPx = slicesX[1].Position;
      UInt32 topPx = slicesY[1].Position;
      UInt32 rightPx = NumericCast.ToUInt32(slicesX[3].Position - slicesX[2].Position);
      UInt32 bottomPx = NumericCast.ToUInt32(slicesY[3].Position - slicesY[2].Position);
      var nineSlice = new PxThicknessU(leftPx, topPx, rightPx, bottomPx);

      UInt32 contentLPx = spansX[0].Start;
      UInt32 contentTPx = spansY[0].Start;
      UInt32 contentRPx = NumericCast.ToUInt32(imageSize.Width - spansX[0].End);
      UInt32 contentBPx = NumericCast.ToUInt32(imageSize.Height - spansY[0].End);
      var contentMarginPx = new PxThicknessU(contentLPx, contentTPx, contentRPx, contentBPx);

      // Extract nine slice flags from the patch
      AtlasNineSliceFlags flags = ExtractFlags(patchInfo.ComplexPatch);

      return new AtlasNineSliceInfo(nineSlice, contentMarginPx, flags);
    }

    private static AtlasNineSliceFlags ExtractFlags(in ImmutableComplexPatch complexPatch)
    {
      Debug.Assert(complexPatch.Slices.CountX == 4);
      Debug.Assert(complexPatch.Slices.CountY == 4);
      Debug.Assert(complexPatch.GridFlags.Count == 9);

      return IsTransparent(complexPatch.GridFlags[0], AtlasNineSliceFlags.Slice0Transparent) |
             IsTransparent(complexPatch.GridFlags[1], AtlasNineSliceFlags.Slice1Transparent) |
             IsTransparent(complexPatch.GridFlags[2], AtlasNineSliceFlags.Slice2Transparent) |
             IsTransparent(complexPatch.GridFlags[3], AtlasNineSliceFlags.Slice3Transparent) |
             IsTransparent(complexPatch.GridFlags[4], AtlasNineSliceFlags.Slice4Transparent) |
             IsTransparent(complexPatch.GridFlags[5], AtlasNineSliceFlags.Slice5Transparent) |
             IsTransparent(complexPatch.GridFlags[6], AtlasNineSliceFlags.Slice6Transparent) |
             IsTransparent(complexPatch.GridFlags[7], AtlasNineSliceFlags.Slice7Transparent) |
             IsTransparent(complexPatch.GridFlags[8], AtlasNineSliceFlags.Slice8Transparent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AtlasNineSliceFlags IsTransparent(ComplexPatchGridFlags srcFlags, AtlasNineSliceFlags flag)
    {
      return ComplexPatchGridFlagsExt.IsFlagged(srcFlags, ComplexPatchGridFlags.Transparent) ? flag : AtlasNineSliceFlags.None;
    }



  }
}

//****************************************************************************************************************************************************
