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

using FslGraphics.Font.BF;
using System;

namespace TexturePacker.Config
{
  public readonly struct AddBitmapFontConfig : IEquatable<AddBitmapFontConfig>
  {
    public readonly BitmapFontType FontType;
    public readonly OutputFontFormat OutputFormat;

    public AddBitmapFontConfig(BitmapFontType bitmapFontType, OutputFontFormat outputFormat)
    {
      FontType = bitmapFontType;
      OutputFormat = outputFormat;
    }

    public static bool operator ==(AddBitmapFontConfig lhs, AddBitmapFontConfig rhs)
      => lhs.FontType == rhs.FontType && lhs.OutputFormat == rhs.OutputFormat;

    public static bool operator !=(AddBitmapFontConfig lhs, AddBitmapFontConfig rhs) => !(lhs == rhs);


    public override bool Equals(object obj) => !(obj is AddBitmapFontConfig) ? false : (this == (AddBitmapFontConfig)obj);


    public override int GetHashCode() => FontType.GetHashCode() ^ OutputFormat.GetHashCode();


    public bool Equals(AddBitmapFontConfig other) => this == other;

    public override string ToString() => $"FontType: {FontType} OutputFormat: {OutputFormat}";
  }
}
