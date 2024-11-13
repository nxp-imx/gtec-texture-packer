/****************************************************************************************************************************************************
 * Copyright 2020, 2024 NXP
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

using MB.Graphics2.Patch.Advanced;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TexturePacker.Commands
{
  [DoNotUseDefaultConstruction]
  public readonly struct ResolvedImageMetaData : IEquatable<ResolvedImageMetaData>
  {
    public readonly bool IsPatch;
    public readonly bool WasNineSlice;
    public readonly int AnchorPoints;
    public readonly string PatchInfo;

    public ResolvedImageMetaData(bool isPatch, bool wasNineSlice, int anchorPoints, AddComplexPatch? addComplexPatch)
    {
      IsPatch = isPatch;
      WasNineSlice = wasNineSlice;
      AnchorPoints = anchorPoints;
      PatchInfo = BuildPatchInfo(addComplexPatch);
    }

    public static bool operator ==(ResolvedImageMetaData lhs, ResolvedImageMetaData rhs)
      => lhs.IsPatch == rhs.IsPatch && lhs.WasNineSlice == rhs.WasNineSlice && lhs.AnchorPoints == rhs.AnchorPoints &&
         lhs.PatchInfo == rhs.PatchInfo;

    public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is ResolvedImageMetaData objValue && (this == objValue);

    public readonly override int GetHashCode()
      => HashCode.Combine(IsPatch, WasNineSlice, AnchorPoints);

    public readonly bool Equals(ResolvedImageMetaData other) => this == other;

    public static bool operator !=(ResolvedImageMetaData lhs, ResolvedImageMetaData rhs) => !(lhs == rhs);

    public readonly override string ToString() => $"IsPatch:{IsPatch} WasNineSlice:{WasNineSlice} AnchorPoints:{AnchorPoints} PatchInfo:{PatchInfo}";


    public static string BuildPatchInfo(AddComplexPatch? value)
    {
      if (value == null)
        return "{}";

      return Encode(value.Patch);
    }

    public static string Encode(in ImmutableComplexPatch patch) => $"{{ {Encode(patch.Slices)}, {Encode(patch.ContentSpans)} }}";

    public static string Encode(in ImmutableComplexPatchSlices slices) => $"{{ {Encode(slices.AsSpanX())}, {Encode(slices.AsSpanY())} }}";

    private static string Encode(ReadOnlySpan<ImmutableComplexPatchSlice> span)
    {
      var entries = new ComplexPatchSliceFlags[span.Length];
      for (int i = 0; i < span.Length; ++i)
      {
        entries[i] = span[i].Flags;
      }
      return $"{{ {string.Join(",", entries)} }}";
    }

    private static string Encode(in ImmutablePatchContentSpans value) => $"{{ {Encode(value.AsSpanX())}, {Encode(value.AsSpanY())} }}";

    private static string Encode(ReadOnlySpan<ImmutableContentSpan> span) => $"{span.Length}";
  }
}
