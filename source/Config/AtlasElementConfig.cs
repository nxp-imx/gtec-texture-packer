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

namespace TexturePacker.Config
{
  public readonly struct AtlasElementConfig : IEquatable<AtlasElementConfig>
  {
    /// <summary>
    /// The atlas default dpi for elements (that doesnt override it)
    /// </summary>
    public readonly UInt16 DefaultDpi;

    /// <summary>
    /// Extrude
    /// </summary>
    public readonly UInt16 Extrude;

    /// <summary>
    /// Trim transparent pixels (on/off)
    /// </summary>
    public readonly bool Trim;

    /// <summary>
    /// The number of transparent pixels to leave around the image
    /// </summary>
    public readonly UInt16 TrimMargin;

    /// <summary>
    /// The minimum tranparency value (0-255)
    /// All values below this is considered fully transparent (should usually be 1)
    /// </summary>
    public readonly UInt16 TransparencyThreshold;

    /// <summary>
    /// The space between elements (defaults to 2)
    /// This is used to prevent artifacts from neighbour elements.
    /// Can not be negative.
    /// </summary>
    public readonly UInt16 ShapePadding;

    /// <summary>
    /// The space between the elements and the border (defaults to 2)
    /// Adds this many transparent pixels to the border of the texture.
    /// Can not be negative.
    /// </summary>
    public readonly UInt16 BorderPadding;

    public AtlasElementConfig(UInt16 defaultDpi, UInt16 extrude, bool trim, UInt16 trimMargin, UInt16 transparencyThreshold = 1, UInt16 shapePadding = 2, UInt16 borderPadding = 2)
    {
      DefaultDpi = defaultDpi;
      Extrude = extrude;
      Trim = trim;
      TrimMargin = trimMargin;
      TransparencyThreshold = transparencyThreshold;
      ShapePadding = shapePadding;
      BorderPadding = borderPadding;
    }

    public bool IsValid => DefaultDpi != 0;

    public static bool operator ==(AtlasElementConfig lhs, AtlasElementConfig rhs)
      => lhs.DefaultDpi == rhs.DefaultDpi && lhs.Extrude == rhs.Extrude && lhs.Trim == rhs.Trim && lhs.TrimMargin == rhs.TrimMargin &&
         lhs.TransparencyThreshold == rhs.TransparencyThreshold && lhs.ShapePadding == rhs.ShapePadding && lhs.BorderPadding == rhs.BorderPadding;

    public static bool operator !=(AtlasElementConfig lhs, AtlasElementConfig rhs) => !(lhs == rhs);


    public override bool Equals(object obj)
    {
      return !(obj is AtlasElementConfig) ? false : (this == (AtlasElementConfig)obj);
    }


    public override int GetHashCode()
      => DefaultDpi.GetHashCode() ^ Extrude.GetHashCode() ^ Trim.GetHashCode() ^ TrimMargin.GetHashCode() ^ TransparencyThreshold.GetHashCode() ^
         ShapePadding.GetHashCode() ^ BorderPadding.GetHashCode();


    public bool Equals(AtlasElementConfig other) => this == other;

    public override string ToString()
      => $"DefaultDp: {DefaultDpi} Extrude: {Extrude} Trim: {Trim} TrimMargin: {TrimMargin} TransparencyThreshold: {TransparencyThreshold} ShapePadding: {ShapePadding} BorderPadding: {BorderPadding}";
  }
}
