/****************************************************************************************************************************************************
 * Copyright 2020, 2024 NXP
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
using System.Diagnostics.CodeAnalysis;

namespace TexturePacker.Config
{
  public readonly struct TexturePackerConfig : IEquatable<TexturePackerConfig>
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly string DefaultCompany;
    public readonly string DefaultNamespaceName;
    public readonly CreateAtlasConfig CreateAtlas;
    public readonly string DefaultFilename;

    /// <summary>
    /// Default license file settings
    /// </summary>
    public readonly LicenseConfig License;

    public TexturePackerConfig(string? defaultCompany, string? defaultNamespaceName, string defaultFilename, CreateAtlasConfig createAtlas, LicenseConfig license)
    {
      if (defaultCompany == null || defaultCompany.Length == 0)
      {
        if (defaultCompany != null)
          g_logger.Warn("Empty default company name found, please supply one");
        defaultCompany = "Company not specified";
      }
      if (defaultNamespaceName == null || defaultNamespaceName.Length == 0)
      {
        if (defaultNamespaceName != null)
          g_logger.Warn("Empty default namespace name, please supply one");
        defaultNamespaceName = "NXP.Base";
      }

      DefaultCompany = defaultCompany.Trim();
      DefaultNamespaceName = defaultNamespaceName.Trim();
      DefaultFilename = defaultFilename ?? throw new ArgumentNullException(nameof(defaultFilename));
      CreateAtlas = createAtlas;
      License = license;

      NameUtil.ValidateFilename(DefaultFilename);
    }

    public static bool operator ==(TexturePackerConfig lhs, TexturePackerConfig rhs) => lhs.CreateAtlas == rhs.CreateAtlas && lhs.License == rhs.License;

    public static bool operator !=(TexturePackerConfig lhs, TexturePackerConfig rhs) => !(lhs == rhs);


    public override bool Equals([NotNullWhen(true)] object? obj) => obj is TexturePackerConfig objValue && (this == objValue);

    public override int GetHashCode() => HashCode.Combine(CreateAtlas, License);


    public bool Equals(TexturePackerConfig other) => this == other;

    public override string ToString() => $"CreateAtlas: {CreateAtlas} License: {License}";

    public static TexturePackerConfig PatchCreateAtlas(TexturePackerConfig src, CreateAtlasConfig createAtlas)
    {
      return new TexturePackerConfig(src.DefaultCompany, src.DefaultNamespaceName, src.DefaultFilename, createAtlas, src.License);
    }
  }
}
