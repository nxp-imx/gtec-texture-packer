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
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace TexturePacker.License
{
  public sealed class LicenseInfoNxpJsonDecoder
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "A object for future improvements")]
    public ComplexLicenseInfo DecodeFile(string strFilename)
    {
      if (strFilename == null)
        throw new ArgumentNullException(nameof(strFilename));

      try
      {
        return Decode(File.ReadAllText(strFilename));
      }
      catch (Exception ex)
      {
        g_logger.Warn(ex, "Failed to decode file '{0}'", strFilename);
        throw;
      }
    }

    private static ComplexLicenseInfo Decode(string strJson)
    {
      if (strJson == null)
        throw new ArgumentNullException(nameof(strJson));

      var resultDict = JsonSerializer.Deserialize(strJson, LicenseJsonContext.Default.DictionaryStringJsonElement);
      if (resultDict == null)
      {
        throw new NotSupportedException("license is not in a supported format");
      }

      if (resultDict.ContainsKey(LicenseInfoNxpJsonValues.KeyComplexLicense))
      {
        return DecodeComplexLicense(resultDict);
      }
      return new ComplexLicenseInfo(DecodeBasicLicense(resultDict), null);
    }

    private static ComplexLicenseInfo DecodeComplexLicense(Dictionary<string, JsonElement> dict)
    {
      var complexElementDict = GetValueAsDict(dict, LicenseInfoNxpJsonValues.KeyComplexLicense);
      string? comment = TryGetValueAsString(complexElementDict, LicenseInfoNxpJsonValues.KeyComment);
      var licensesArray = GetValueAsLicenseInfoNxpJsonArray(complexElementDict, LicenseInfoNxpJsonValues.KeyComplexLicenses);

      ValidateCheckKeys(dict, LicenseInfoNxpJsonValues.ValidComplexLicenseKeys);

      return new ComplexLicenseInfo(licensesArray, comment);
    }

    private static BasicLicenseInfo DecodeBasicLicense(Dictionary<string, JsonElement> dict)
    {
      string origin = GetValueAsString(dict, LicenseInfoNxpJsonValues.KeyOrigin);
      string license = GetValueAsString(dict, LicenseInfoNxpJsonValues.KeyLicense);
      string? url = TryGetValueAsString(dict, LicenseInfoNxpJsonValues.KeyUrl);
      string? comment = TryGetValueAsString(dict, LicenseInfoNxpJsonValues.KeyComment);

      ValidateCheckKeys(dict, LicenseInfoNxpJsonValues.ValidKeys);

      var licenseInfo = new LicenseInfoNxpJson(origin, license, url, comment);
      return new BasicLicenseInfo(licenseInfo.Origin, licenseInfo.License, licenseInfo.Url, licenseInfo.Comment);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void ValidateCheckKeys(Dictionary<string, JsonElement> dict, string[] validKeys)
    {
      { // Check for unsupported keys
        foreach (var key in validKeys)
        {
          dict.Remove(key);
        }
        foreach (var pair in dict)
        {
          g_logger.Warn("Found unsupported key: {0}", pair.Key);
        }
      }
    }

    private static Dictionary<string, JsonElement> GetValueAsDict(Dictionary<string, JsonElement> dict, string key)
    {
      if (!dict.TryGetValue(key, out JsonElement valueElement))
        throw new Exception($"The key '{key}' was not found");

      if (valueElement.ValueKind != JsonValueKind.Object)
        throw new Exception($"The value '{valueElement}' was not a object");

      return JsonObjectToDict(valueElement);
    }

    private static Dictionary<string, JsonElement> JsonObjectToDict(JsonElement element)
    {
      if (element.ValueKind != JsonValueKind.Object)
        throw new Exception($"The value '{element}' was not a object");

      var newDict = new Dictionary<string, JsonElement>();
      foreach (var entry in element.EnumerateObject())
      {
        newDict[entry.Name] = entry.Value;
      }
      return newDict;
    }

    private static BasicLicenseInfo[] GetValueAsLicenseInfoNxpJsonArray(Dictionary<string, JsonElement> dict, string key)
    {
      if (!dict.TryGetValue(key, out JsonElement valueElement))
        throw new Exception($"The key '{key}' was not found");

      if (valueElement.ValueKind != JsonValueKind.Array)
        throw new Exception($"The value '{valueElement}' was not a array");

      var result = new BasicLicenseInfo[valueElement.GetArrayLength()];
      int dstIndex = 0;
      foreach (var arrayEntry in valueElement.EnumerateArray())
      {
        var childObject = JsonObjectToDict(arrayEntry);
        result[dstIndex] = DecodeBasicLicense(childObject);
        ++dstIndex;
      }
      Debug.Assert(dstIndex == result.Length);
      return result;
    }

    private static string GetValueAsString(Dictionary<string, JsonElement> dict, string key)
    {
      if (!dict.TryGetValue(key, out JsonElement valueElement))
        throw new Exception($"The key '{key}' was not found");
      if (valueElement.ValueKind != JsonValueKind.String)
        throw new Exception($"The value '{valueElement}' was not a string");
      var result = valueElement.GetString();
      if (result == null)
        throw new Exception("the value was null");
      return result;
    }

    private static string GetValueAsString(Dictionary<string, JsonElement> dict, string key, string defaultValue)
    {
      if (!dict.TryGetValue(key, out JsonElement valueElement))
        return defaultValue;
      if (valueElement.ValueKind != JsonValueKind.String)
        throw new Exception($"The value '{valueElement}' was not a string");
      var result = valueElement.GetString();
      return result != null ? result : defaultValue;
    }

    private static string? TryGetValueAsString(Dictionary<string, JsonElement> dict, string key)
    {
      if (!dict.TryGetValue(key, out JsonElement valueElement))
        return null;
      if (valueElement.ValueKind != JsonValueKind.String)
        throw new Exception($"The value '{valueElement}' was not a string");
      return valueElement.GetString();
    }
  }
}
