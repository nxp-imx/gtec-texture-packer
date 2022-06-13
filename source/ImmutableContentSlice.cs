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
using System.Runtime.CompilerServices;

namespace TexturePacker
{
  /// <summary>
  /// Represents a slice
  /// </summary>
  [Serializable]
  public readonly struct ImmutableContentSlice : IEquatable<ImmutableContentSlice>
  {
    public readonly UInt16 Position;
    public readonly ContentSliceFlags Flags;

    //------------------------------------------------------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableContentSlice(UInt16 position, ContentSliceFlags flags)
    {
      Position = position;
      Flags = flags;
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Check if a span is equal to another span
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ImmutableContentSlice lhs, ImmutableContentSlice rhs) => lhs.Position == rhs.Position && lhs.Flags == rhs.Flags;

    //------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Check if a span is not equal to another span
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ImmutableContentSlice lhs, ImmutableContentSlice rhs) => !(lhs == rhs);

    //------------------------------------------------------------------------------------------------------------------------------------------------

    public override bool Equals(object obj)
    {
      return !(obj is ImmutableContentSlice) ? false : (this == (ImmutableContentSlice)obj);
    }


    //------------------------------------------------------------------------------------------------------------------------------------------------

    public override int GetHashCode() => Position.GetHashCode();

    //------------------------------------------------------------------------------------------------------------------------------------------------

    public override string ToString() => $"({Position}:{Flags})";

    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region IEquatable<ImmutableContentSlice> Members
    //------------------------------------------------------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ImmutableContentSlice other) => Position == other.Position && Flags == other.Flags;

    //------------------------------------------------------------------------------------------------------------------------------------------------
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
  }
}
