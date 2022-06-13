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
  /// <summary>
  /// Settings for the CreateAtlas command
  /// </summary>
  public readonly struct CreateAtlasConfig : IEquatable<CreateAtlasConfig>
  {
    public readonly OutputAtlasFormat OutputFormat;
    //public readonly UInt16 DefaultDpi;
    public readonly AtlasConfig Atlas;

    /// <summary>
    /// Settings for AddBitmapFont commands
    /// </summary>
    public readonly AddBitmapFontConfig AddBitmapFont;


    public CreateAtlasConfig(OutputAtlasFormat outputFormat, AtlasConfig atlas, AddBitmapFontConfig addBitmapFontConfig)
    {
      OutputFormat = outputFormat;
      Atlas = atlas;
      AddBitmapFont = addBitmapFontConfig;
    }
    //public CreateAtlasConfig(OutputAtlasFormat outputFormat, UInt16 defaultDp, AtlasConfig atlas, AddBitmapFontConfig addBitmapFontConfig)
    //{
    //  OutputFormat = outputFormat;
    //  DefaultDp = defaultDpi;
    //  Atlas = atlas;
    //  AddBitmapFont = addBitmapFontConfig;
    //}

    public static bool operator ==(CreateAtlasConfig lhs, CreateAtlasConfig rhs)
      => lhs.OutputFormat == rhs.OutputFormat && lhs.Atlas == rhs.Atlas && lhs.AddBitmapFont == rhs.AddBitmapFont;

    public static bool operator !=(CreateAtlasConfig lhs, CreateAtlasConfig rhs) => !(lhs == rhs);


    public override bool Equals(object obj)
    {
      return !(obj is CreateAtlasConfig) ? false : (this == (CreateAtlasConfig)obj);
    }


    public override int GetHashCode() => OutputFormat.GetHashCode() ^ Atlas.GetHashCode() ^ AddBitmapFont.GetHashCode();


    public bool Equals(CreateAtlasConfig other) => this == other;

    //public override string ToString()
    //{
    //  return $"OutputFormat: {OutputFormat} Atlas: {Atlas} AddBitmapFont: {AddBitmapFont}";
    //}
  }
}
