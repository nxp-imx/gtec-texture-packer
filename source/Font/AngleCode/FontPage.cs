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
using System.Diagnostics.CodeAnalysis;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.AngleCode
{
  /// <summary>
  /// This gives the name of a texture file. There is one for each page in the font.
  /// Based on: https://www.angelcode.com/products/bmfont/doc/file_format.html
  /// </summary>
  public struct FontPage : IEquatable<FontPage>
  {
    /// <summary>
    /// The page id.
    /// </summary>
    public int Id;

    /// <summary>
    /// The texture file name.
    /// </summary>
    public string File;

    public FontPage(int id, string file)
    {
      if (id < 0)
        throw new ArgumentOutOfRangeException(nameof(id));

      Id = id;
      File = file ?? throw new ArgumentNullException(nameof(file));
    }

    public bool IsValid => Id >= 0 && File != null;

    public static bool operator ==(FontPage lhs, FontPage rhs)
    {
      return lhs.Id == rhs.Id && lhs.File == rhs.File;
    }

    public static bool operator !=(FontPage lhs, FontPage rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals([NotNullWhen(true)] object? obj) => obj is FontPage objValue && (this == objValue);


    public override int GetHashCode() => Id.GetHashCode() ^ (File != null ? File.GetHashCode(StringComparison.Ordinal) : 0);


    public bool Equals(FontPage other) => this == other;


    public override string ToString() => $"Id: {Id} File: {File}";
  }

}

//****************************************************************************************************************************************************
