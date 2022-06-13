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

using NLog;
using System;
using System.IO;

namespace TexturePacker.Input
{
  readonly struct FileNameInfo
  {
    public readonly string NormalizedName;
    public readonly bool IsPatch;
    public readonly UInt16 DpiOverride;

    public FileNameInfo(string normalizedName, bool isPatch, UInt16 dpiOverride)
    {
      NormalizedName = normalizedName ?? throw new ArgumentNullException(nameof(normalizedName));
      IsPatch = isPatch;
      DpiOverride = dpiOverride;
    }
  }

  static class FileNameUtil
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    private const string DpiPostFix = "dpi";
    private const string PatchExtension = ".9.png";
    private const string PureNinePatchExtension = ".9";


    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <returns>If zero no valid dpi was encoded in the name</returns>
    public static UInt16 TryDecodeNameDpi(string name)
    {
      UInt16 defaultDpi = 0;
      if (name.EndsWith(DpiPostFix, StringComparison.Ordinal))
      {
        int index = name.LastIndexOf('_');
        var valueSpan = name.AsSpan().Slice(index + 1, name.Length - (index + 1) - DpiPostFix.Length);
        if (!UInt16.TryParse(valueSpan, out defaultDpi))
        {
          return 0;
        }
      }
      return Math.Max(defaultDpi, (UInt16)0u);
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <returns>If zero no valid dpi was encoded in the name</returns>
    public static UInt16 TryDecodeNameDpi(ReadOnlySpan<char> name)
    {
      UInt16 defaultDpi = 0;
      if (name.EndsWith(DpiPostFix))
      {
        int index = name.LastIndexOf('_');
        var valueSpan = name.Slice(index + 1, name.Length - (index + 1) - DpiPostFix.Length);
        if (!UInt16.TryParse(valueSpan, out defaultDpi))
        {
          return 0;
        }
      }
      return Math.Max(defaultDpi, (UInt16)0u);
    }


    public static FileNameInfo AnalyzeFilename(string filename, bool keepDpiInFilename)
    {
      var directoryName = Path.GetDirectoryName(filename.AsSpan());
      var normalizedFilenameSpan = Path.GetFileNameWithoutExtension(filename.AsSpan());
      bool isPatch = false;
      // Check if this could be a patch image by checking if the image extension is valid
      if (filename.EndsWith(PatchExtension, StringComparison.Ordinal))
      {
        g_logger.Trace($"- Image is marked as a patch by the extension '{PatchExtension}'");
        isPatch = true;
        if (normalizedFilenameSpan.EndsWith(PureNinePatchExtension, StringComparison.Ordinal))
        {
          normalizedFilenameSpan = normalizedFilenameSpan.Slice(0, normalizedFilenameSpan.Length - PureNinePatchExtension.Length);
        }
      }

      // Check if the filename contains a dpi setting
      UInt16 dpiOverride = AnalyzeDpiTag(keepDpiInFilename, ref normalizedFilenameSpan);

      var normalizedFullPath = IOUtil.Combine(directoryName.ToString(), normalizedFilenameSpan.ToString());
      return new FileNameInfo(normalizedFullPath, isPatch, dpiOverride);
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="keepDpiInFilename"></param>
    /// <returns></returns>
    public static FileNameInfo AnalyzeName(string name, bool keepDpiInFilename)
    {
      var nameSpan = name.AsSpan();
      UInt16 dpiOverride = AnalyzeDpiTag(keepDpiInFilename, ref nameSpan);
      return new FileNameInfo(nameSpan.ToString(), false, dpiOverride);
    }

    private static UInt16 AnalyzeDpiTag(bool keepDpiInFilename, ref ReadOnlySpan<char> normalizedFilenameSpan)
    {
      UInt16 dpiOverride = 0;
      if (normalizedFilenameSpan.EndsWith(DpiPostFix, StringComparison.Ordinal))
      {
        int index = normalizedFilenameSpan.LastIndexOf('_');
        var valueSpan = normalizedFilenameSpan.Slice(index + 1, normalizedFilenameSpan.Length - (index + 1) - DpiPostFix.Length);
        if (!UInt16.TryParse(valueSpan, out dpiOverride))
        {
          dpiOverride = 0;
        }
        else
        {
          g_logger.Trace($"- Image dpi set by filename to '{dpiOverride}'");
          if (!keepDpiInFilename)
          {
            normalizedFilenameSpan = normalizedFilenameSpan.Slice(0, index);
          }
        }
      }

      return dpiOverride;
    }
  }
}
