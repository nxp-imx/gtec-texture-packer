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

namespace TexturePacker.License
{
  public sealed class BasicLicenseInfo
  {
    //private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly string Origin;
    public readonly string License;
    public readonly string? Url;
    public readonly string? Comment;


    //public readonly ImmutableArray<string> Files;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "we want a string here")]
    public BasicLicenseInfo(string origin, string license, string? url, string? comment)
    {
      Origin = origin ?? throw new ArgumentNullException(nameof(origin));
      License = license ?? throw new ArgumentNullException(nameof(license));
      Url = url;
      Comment = comment;

      Origin = Origin.Trim();
      License = License.Trim();
      Url = TrimOptionalValue(Url);
      Comment = TrimOptionalValue(Comment);

      if (origin.Length < 1)
        throw new ArgumentException("origin must be at least one character");
      if (license.Length < 1)
        throw new ArgumentException("license must be at least one character");
      if (url != null && !(url.StartsWith("http://", StringComparison.Ordinal) || url.StartsWith("https://", StringComparison.Ordinal)))
        throw new ArgumentException("url must start with 'http://' or 'https://'");
      if (comment != null && comment.Length < 1)
        throw new ArgumentException("comment must be at least one character");
      if (origin.Contains(';', StringComparison.Ordinal))
        throw new NotSupportedException($"Origin '{Origin}' can not contain ';'");
      if (license.Contains(';', StringComparison.Ordinal))
        throw new NotSupportedException($"License '{License}' can not contain ';'");
    }

    private static string? TrimOptionalValue(string? optionalValue)
    {
      if (optionalValue == null)
        return null;
      var trimmed = optionalValue.Trim();
      if (trimmed.Length == 0)
        return null;
      return trimmed;
    }

    public bool IsConsideredEqual(BasicLicenseInfo value)
    {
      return value != null && Origin == value.Origin && License == value.License && Url == value.Url && Comment == value.Comment;
    }

    public static int Compare(BasicLicenseInfo lhs, BasicLicenseInfo rhs)
    {
      if (lhs == null)
        return (rhs == null ? 1 : -1);
      if (rhs == null)
        return 1;

      int res = string.Compare(lhs.Origin, rhs.Origin, StringComparison.Ordinal);
      if (res != 0)
        return res;
      res = string.Compare(lhs.License, rhs.License, StringComparison.Ordinal);
      if (res != 0)
        return res;
      res = string.Compare(lhs.Url, rhs.Url, StringComparison.Ordinal);
      if (res != 0)
        return res;
      return string.Compare(lhs.Comment, rhs.Comment, StringComparison.Ordinal);
    }
  }
}
