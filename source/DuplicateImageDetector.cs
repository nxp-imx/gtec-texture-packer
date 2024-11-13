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
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using TexturePacker.Atlas;

namespace TexturePacker
{
  static class DuplicateImageDetector
  {
    //private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    private struct ImageHashRecord
    {
      public int OriginalIndex;
      public string Hash;
      public SafeImage SrcImage;

      public ImageHashRecord(int originalIndex, string hash, SafeImage srcImage)
      {
        OriginalIndex = originalIndex;
        Hash = hash;
        SrcImage = srcImage ?? throw new ArgumentNullException(nameof(srcImage));
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "We just use it as a hash id so its not a problem")]
    private static List<ImageHashRecord> ComputeImageHashes(List<AtlasElement> elements)
    {
      // Compute hashes
      var resultQueue = new List<ImageHashRecord>(elements.Count);
      var builder = new StringBuilder();
      var numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
      using (var sha = MD5.Create())
      {
        for (int i = 0; i < elements.Count; ++i)
        {
          var element = elements[i];
          var bytes = element.SourceImage.ToByteArray();
          var hashArray = sha.ComputeHash(bytes);

          builder.Clear();
          for (int index = 0; index < hashArray.Length; ++index)
          {
            builder.Append(hashArray[index].ToString("x2", numberFormat));
          }
          resultQueue.Add(new ImageHashRecord(i, builder.ToString(), element.SourceImage));
        }
      }
      return resultQueue;
    }

    /// <summary>
    /// Detect duplicates
    /// </summary>
    /// <param name="elements"></param>
    /// <returns>The key is the original image index. The value will be the original index of the first duplicate
    ///          (the first duplicate will not be part of the dict).
    ///          The first duplicate will always tbe the lowest original index.</returns>
    public static Dictionary<int, int>? TryDetectDuplicates(List<AtlasElement> elements)
    {
      var hashes = ComputeImageHashes(elements);

      // Detect possible duplicates
      var possibleDuplicatesDict = new Dictionary<string, List<ImageHashRecord>>(elements.Count);
      int possibleDuplicateCount = 0;
      foreach (var entry in hashes)
      {
        if (possibleDuplicatesDict.TryGetValue(entry.Hash, out List<ImageHashRecord>? possibleDuplicateList))
        {
          // Possible duplicate found
          possibleDuplicateList.Add(entry);
          ++possibleDuplicateCount;
        }
        else
        {
          // Not a duplicate
          var entryList = new List<ImageHashRecord>();
          entryList.Add(entry);
          possibleDuplicatesDict[entry.Hash] = entryList;
        }
      }
      if (possibleDuplicateCount <= 0)
        return null;

      var duplicateDict = new Dictionary<int, int>();
      foreach (var pair in possibleDuplicatesDict)
      {
        if (pair.Value.Count > 1)
        {
          // Detect duplicates
          DetectDuplicates(duplicateDict, pair.Value);
        }
      }
      return duplicateDict;
    }

    /// <summary>
    /// Detect duplicates
    /// </summary>
    /// <param name="duplicateDict"></param>
    /// <param name="duplicateCandidates">WARNING: this will be modified</param>
    private static void DetectDuplicates(Dictionary<int, int> duplicateDict, List<ImageHashRecord> duplicateCandidates)
    {
      // Sort the list high to low so we know that the initial key will be the lowest and the entries will be inserted in sorted order
      duplicateCandidates.Sort((lhs, rhs) => rhs.OriginalIndex.CompareTo(lhs.OriginalIndex));

      for (int srcIndex = duplicateCandidates.Count - 1; srcIndex >= 1; --srcIndex)
      {
        var entry = duplicateCandidates[srcIndex];
        int swapIndex = srcIndex - 1;
        for (int compareIndex = swapIndex; compareIndex >= 0; --compareIndex)
        {
          Debug.Assert(srcIndex != compareIndex);
          var compEntry = duplicateCandidates[compareIndex];
          if (SafeImage.IsDuplicate(entry.SrcImage, compEntry.SrcImage))
          {
            // We have a duplicate
            duplicateDict[compEntry.OriginalIndex] = entry.OriginalIndex;

            // Swap the duplicate to the swapIndex position
            Debug.Assert(compareIndex >= 0);
            Debug.Assert(swapIndex >= 0);
            Debug.Assert(srcIndex > 0);
            Debug.Assert(compareIndex <= swapIndex);
            duplicateCandidates[compareIndex] = duplicateCandidates[swapIndex];
            duplicateCandidates[swapIndex] = compEntry;
            // decrease the swapIndex and the srcIndex so we skip the next check of them
            --swapIndex;
            --srcIndex;
          }
        }
      }
    }
  }
}
