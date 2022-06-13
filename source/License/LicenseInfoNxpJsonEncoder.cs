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
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TexturePacker.License
{
  // Unfortunately we are unable to use 'System.Text.Json' at this point in time as you are unable to control the serialization ordering of keys.
  // This means two writes of the same data will not necessarily produce the same file.
  // This is useless for files that are put into version control!
  //
  // So we use a custom json writer
  public sealed class LicenseInfoNxpJsonEncoder
  {
    //private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "We want to keep this a object for future modifications")]
    public string Encode(ComplexLicenseInfo license)
    {
      if (license == null)
      {
        throw new ArgumentNullException(nameof(license));
      }
      if (license.Comment != null || license.Licenses.Length > 1)
      {
        return DoEncode(license);
      }
      else if (license.Licenses.Length == 1)
      {
        return Encode(license.Licenses[0]);
      }
      throw new NotSupportedException("invalid empty license");
    }

    private static string DoEncode(ComplexLicenseInfo info)
    {
      var serializedLines = JsonSerialize(info);
      return string.Join("\n", serializedLines) + "\n";
    }

    private static string Encode(BasicLicenseInfo info)
    {
      var nxpLicense = TypeConverter.ToLicenseInfoNxpJson(info);
      var serializedLines = new List<string>();
      JsonSerialize(serializedLines, nxpLicense);
      return string.Join("\n", serializedLines) + "\n";
    }

    private static List<string> JsonSerialize(ComplexLicenseInfo info)
    {
      int licenseIndent = 3;
      var sortedLicenses = SortLicences(info.Licenses);

      // While this is not a-z sorted it will be the same order everytime (and it matches the order of most existing files)
      var dstLines = new List<string>();
      dstLines.Add("{");
      dstLines.Add($"  \"{LicenseInfoNxpJsonValues.KeyComplexLicense}\": {{");
      if (info.Comment != null)
        dstLines.Add($"    \"{LicenseInfoNxpJsonValues.KeyComplexComment}\": \"{ToJsonString(info.Comment)}\",");
      dstLines.Add($"    \"{LicenseInfoNxpJsonValues.KeyComplexLicenses}\": [");
      {
        for (int i = 0; i < sortedLicenses.Length - 1; ++i)
        {
          var nxpLicense = TypeConverter.ToLicenseInfoNxpJson(sortedLicenses[i]);
          JsonSerialize(dstLines, nxpLicense, licenseIndent);
          dstLines[dstLines.Count - 1] += ",";
        }
        if (sortedLicenses.Length > 0)
        {
          var nxpLicense = TypeConverter.ToLicenseInfoNxpJson(sortedLicenses[sortedLicenses.Length - 1]);
          JsonSerialize(dstLines, nxpLicense, licenseIndent);
        }
      }
      dstLines.Add($"    ]");
      dstLines.Add($"  }}");
      dstLines.Add("}");
      return dstLines;
    }

    private static ImmutableArray<BasicLicenseInfo> SortLicences(ImmutableArray<BasicLicenseInfo> licenes)
    {
      return licenes.Sort(BasicLicenseInfo.Compare);
    }

    private static void JsonSerialize(List<string> dstLines, in LicenseInfoNxpJson info, int indent = 0, int indentSize = 2)
    {
      var indentSpaces = new string(' ', indent * indentSize);

      // While this is not a-z sorted it will be the same order everytime (and it matches the order of most existing files)
      dstLines.Add($"{indentSpaces}{{");
      dstLines.Add($"{indentSpaces}  \"{LicenseInfoNxpJsonValues.KeyOrigin}\": \"{ToJsonString(info.Origin)}\",");
      dstLines.Add($"{indentSpaces}  \"{LicenseInfoNxpJsonValues.KeyLicense}\": \"{ToJsonString(info.License)}\"");
      if (info.Comment != null)
      {
        dstLines[dstLines.Count - 1] += ",";
        dstLines.Add($"{indentSpaces}  \"{LicenseInfoNxpJsonValues.KeyComment}\": \"{ToJsonString(info.Comment)}\"");
      }
      if (info.Url != null)
      {
        dstLines[dstLines.Count - 1] += ",";
        dstLines.Add($"{indentSpaces}  \"{LicenseInfoNxpJsonValues.KeyUrl}\": \"{ToJsonString(info.Url)}\"");
      }
      dstLines.Add($"{indentSpaces}}}");
    }

    private static string ToJsonString(string str)
    {
      return str;
      //return System.Text.Json.JsonEncodedText.Encode(str).ToString();
    }
  }
}
