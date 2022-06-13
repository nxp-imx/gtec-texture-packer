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
  public readonly struct LicenseConfig : IEquatable<LicenseConfig>
  {
    public readonly bool Enabled;
    public readonly string Filename;
    public readonly LicenseFormat LicenseFormat;
    public readonly bool AllowMasterFileCreation;
    public readonly bool RequiredForAllContent;

    public LicenseConfig(string licenseFileName, LicenseFormat licenseFormat, bool allowMasterFileCreation, bool requiredForAllContent)
    {
      ValidatePathEntry(licenseFileName);
      Enabled = licenseFileName != null;
      Filename = licenseFileName;
      LicenseFormat = licenseFormat;
      AllowMasterFileCreation = allowMasterFileCreation;
      RequiredForAllContent = requiredForAllContent;
    }

    private static void ValidatePathEntry(string entry)
    {
      if (entry == null)
        throw new ArgumentNullException(nameof(entry));

      NameUtil.ValidateFilename(entry);
    }

    public static bool operator ==(LicenseConfig lhs, LicenseConfig rhs)
      => lhs.Enabled == rhs.Enabled && lhs.Filename == rhs.Filename && lhs.LicenseFormat == rhs.LicenseFormat &&
         lhs.AllowMasterFileCreation == rhs.AllowMasterFileCreation && lhs.RequiredForAllContent == rhs.RequiredForAllContent;

    public static bool operator !=(LicenseConfig lhs, LicenseConfig rhs) => !(lhs == rhs);


    public override bool Equals(object obj)
    {
      return !(obj is LicenseConfig) ? false : (this == (LicenseConfig)obj);
    }


    public override int GetHashCode()
      => Enabled.GetHashCode() ^ (Filename != null ? Filename.GetHashCode(StringComparison.Ordinal) : 0) ^ LicenseFormat.GetHashCode() ^
         AllowMasterFileCreation.GetHashCode() ^ RequiredForAllContent.GetHashCode();


    public bool Equals(LicenseConfig other) => this == other;

    public override string ToString() => $"Enabled: {Enabled} Filename: {Filename} LicenseFormat: {LicenseFormat} AllowMasterFileCreation: {AllowMasterFileCreation} RequiredForAllContent {RequiredForAllContent}";
  }
}
