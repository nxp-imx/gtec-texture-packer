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
  public sealed class ComplexLicenseInfo : ALicenseInfo
  {
    //private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    public readonly ImmutableArray<BasicLicenseInfo> Licenses;
    public readonly string Comment;

    public ComplexLicenseInfo(BasicLicenseInfo basicLisence, string comment)
      : this(ImmutableArray.Create(basicLisence), comment)
    {
    }

    public ComplexLicenseInfo(BasicLicenseInfo[] licenses, string comment)
      : this(ImmutableArray.Create(licenses), comment)
    {
    }

    public ComplexLicenseInfo(ImmutableArray<BasicLicenseInfo> licenses, string comment)
      : base(LicenseFormat.NxpJson, true)
    {
      ValidateLicenses(licenses);
      Licenses = Sort(licenses);
      Comment = comment;
    }


    public override bool IsContentConsideredEqual(ALicenseInfo other)
    {
      var rhs = other as ComplexLicenseInfo;
      if (rhs == null)
        return false;

      return AreLicensesConsideredEqual(rhs.Licenses) && Comment == rhs.Comment;
    }

    public override ALicenseInfo Merge(ALicenseInfo other)
    {
      var rhs = other as ComplexLicenseInfo;
      if (rhs == null)
        throw new Exception("Other must be a ComplexLicenseInfo");

      string mergedComment = MergeComment(Comment, rhs.Comment);
      var mergedLicenses = new List<BasicLicenseInfo>(Licenses.Length + rhs.Licenses.Length);
      for (int i = 0; i < Licenses.Length; ++i)
        mergedLicenses.Add(Licenses[i]);

      for (int i = 0; i < rhs.Licenses.Length; ++i)
      {
        if (!Contains(mergedLicenses, rhs.Licenses[i]))
          mergedLicenses.Add(rhs.Licenses[i]);
      }
      return new ComplexLicenseInfo(ImmutableArray.Create(mergedLicenses.ToArray()), mergedComment);
    }

    private static bool Contains(List<BasicLicenseInfo> existingLicenses, BasicLicenseInfo value)
    {
      foreach (var entry in existingLicenses)
      {
        if (BasicLicenseInfo.Compare(entry, value) == 0)
          return true;
      }
      return false;
    }

    private static string MergeComment(string comment1, string comment2)
    {
      if (comment1 == null)
        return comment2;
      if (comment2 == null)
        return comment1;

      if (comment1.EndsWith(';'))
        return $"{comment1}{comment2}";
      return $"{comment1};{comment2}";
    }

    private static void ValidateLicenses(ImmutableArray<BasicLicenseInfo> licenses)
    {
      if (licenses.Length < 1)
        throw new ArgumentException("a complex license must contain at least one entry");
    }

    private bool AreLicensesConsideredEqual(ImmutableArray<BasicLicenseInfo> rhsLicenses)
    {
      if (Licenses.Length != rhsLicenses.Length)
        return false;

      // We expect the licenses to be stored in the same order
      for (int i = 0; i < Licenses.Length; ++i)
      {
        if (!Licenses[i].IsConsideredEqual(rhsLicenses[i]))
          return false;
      }
      return true;
    }

    private static ImmutableArray<BasicLicenseInfo> Sort(ImmutableArray<BasicLicenseInfo> licenses)
    {
      return licenses.Sort((lhs, rhs) => { return BasicLicenseInfo.Compare(lhs, rhs); });
    }


  }
}
