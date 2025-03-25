/****************************************************************************************************************************************************
 * Copyright 2025 NXP
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

using FslGraphics.Font.Basic;
using System;

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font
{
  static class FontFileFormatUtil
  {
    /// <summary>
    /// This is a really dumb but easy way to guess the font file format.
    /// But its good enough for now.
    /// </summary>
    /// <returns></returns>
    public static FontFileFormat GuessFontFormatFromFilename(string filename)
    {
      if (filename.EndsWith(".fnt", StringComparison.InvariantCultureIgnoreCase))
      {
        return FontFileFormat.AngleCode;
      }
      if (filename.EndsWith($".{BinaryFontBasicKerning.DefaultFileExtension}", StringComparison.InvariantCultureIgnoreCase))
      {
        return FontFileFormat.Basic;
      }
      if (filename.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
      {
        return FontFileFormat.MsdfAtlas;
      }
      throw new NotSupportedException($"Could not guess font file format for: {filename}");
    }
  }
}

//****************************************************************************************************************************************************
