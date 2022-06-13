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

using MB.Base.MathEx.Pixel;
using MB.Encoder.TextureAtlas.BTA;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TexturePacker.Commands;

namespace TexturePacker
{
  static class PostProcessAtlasFlavorValidation
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();
    private readonly struct ImageMetaData
    {
      public readonly string Name;
      public readonly string Encoded;

      public ImageMetaData(string name, string encoded)
      {
        Name = name;
        Encoded = encoded;
      }
    }

    private class ImageFlavorRecord
    {
      public readonly ResolvedCommandCreateAtlas Flavor;
      public readonly ImageMetaData MetaData;

      public ImageFlavorRecord(ResolvedCommandCreateAtlas flavor, ImageMetaData metaData)
      {
        Flavor = flavor ?? throw new ArgumentNullException(nameof(flavor));
        MetaData = metaData;
      }
    }

    private class ValidationEntry
    {
      public string Name;
      public List<ImageFlavorRecord> Flavors = new List<ImageFlavorRecord>();

      public ValidationEntry(string name, ImageFlavorRecord imageFlavorRecord)
      {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Flavors.Add(imageFlavorRecord);
      }
    }

    public class TextureAtlasCache
    {
      public readonly BasicTextureAtlas TextureAtlas;
      public readonly Dictionary<string, NamedAtlasNineSlice> NineSliceDict;
      public readonly Dictionary<string, NamedComplexPatch> PatchDict;
      public readonly Dictionary<string, NamedAnchorPoints> AnchorPointDict;

      public TextureAtlasCache(BasicTextureAtlas textureAtlas)
      {
        TextureAtlas = textureAtlas ?? throw new ArgumentNullException(nameof(textureAtlas));
        NineSliceDict = ToDict(textureAtlas.NineSliceEntries);
        PatchDict = ToDict(textureAtlas.PatchEntries);
        AnchorPointDict = ToDict(textureAtlas.AnchorPointEntries);
      }

      private static Dictionary<string, NamedAtlasNineSlice> ToDict(ImmutableArray<NamedAtlasNineSlice> entries)
      {
        var dict = new Dictionary<string, NamedAtlasNineSlice>(entries.Length);
        foreach (var entry in entries)
          dict[entry.Name] = entry;
        return dict;
      }

      private static Dictionary<string, NamedComplexPatch> ToDict(ImmutableArray<NamedComplexPatch> entries)
      {
        var dict = new Dictionary<string, NamedComplexPatch>(entries.Length);
        foreach (var entry in entries)
          dict[entry.Name] = entry;
        return dict;
      }

      private static Dictionary<string, NamedAnchorPoints> ToDict(ImmutableArray<NamedAnchorPoints> entries)
      {
        var dict = new Dictionary<string, NamedAnchorPoints>(entries.Length);
        foreach (var entry in entries)
          dict[entry.Name] = entry;
        return dict;
      }

    }

    public static void Validate(ResolvedCommandAtlasFlavors command)
    {
      g_logger.Trace($"AtlasFlavor '{command.Name}' post validation");

      // We do post process validation because
      // - BitmapFont entries are not checking during preprocessing
      // - Not all patch information is available during preprocessing
      // - To validate the pre-processing check

      var allImageEntriesDict = GenerateAllImageDict(command);
      ValidateFlavorImages(allImageEntriesDict, command.Flavors, command.Name);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void ValidateFlavorImages(Dictionary<string, ValidationEntry> allImageEntriesDict, ImmutableArray<ResolvedCommandCreateAtlasFlavor> flavors, string debugName)
    {
      int expectedRecordCount = flavors.Length;

      // Validate that each file was defined in all packages
      g_logger.Trace("AtlasFlavor '{0}' validating that all {1} atlases contain all {2} image files", debugName, expectedRecordCount, allImageEntriesDict.Count);

      var exceptions = new List<Exception>();
      foreach (var record in allImageEntriesDict.Values)
      {
        if (record.Flavors.Count == expectedRecordCount)
        {
          g_logger.Trace("The file '{0}' is present in all flavors", record.Name);
        }
        else
        {
          g_logger.Warn("The file '{0}' is not present in all flavors", record.Name);
          exceptions.Add(new Exception($"'{record.Name}' was only defined in {GetAtlasNameList(record.Flavors)} but not in {GetMissingAtlasNameList(flavors, record.Flavors)}"));
        }

        var result = ValidateFlavors(record);
        if (result != null)
          exceptions.Add(result);
      }
      if (exceptions.Count > 0)
      {
        throw new AggregateException(exceptions);
      }
    }


    private static Exception ValidateFlavors(ValidationEntry record)
    {
      var dict = new Dictionary<string, List<int>>();
      for (int i = 0; i < record.Flavors.Count; ++i)
      {
        var encoded = record.Flavors[i].MetaData.Encoded;
        if (dict.TryGetValue(encoded, out var duplicateList))
          duplicateList.Add(i);
        else
          dict[encoded] = new List<int>(record.Flavors.Count) { i };
      }

      if (dict.Count == 1)
        return null;
      return new Exception(DescribeError(record.Flavors, dict, record.Name));
    }

    private static string DescribeError(List<ImageFlavorRecord> flavors, Dictionary<string, List<int>> dict, string name)
    {
      string res = $"ERROR meta data differences in the flavors for '{name}'";
      foreach (var entry in dict)
      {
        res += $"\n- {entry.Key} in {ExtractFlavorNames(flavors, entry.Value)}";
      }
      return res;
    }

    private static string ExtractFlavorNames(List<ImageFlavorRecord> flavors, List<int> value)
    {
      var res = new string[value.Count];
      for (int i = 0; i < value.Count; ++i)
      {
        res[i] = flavors[value[i]].Flavor.Name;
      }
      return string.Join(", ", res);
    }

    private static Dictionary<string, ValidationEntry> GenerateAllImageDict(ResolvedCommandAtlasFlavors command)
    {
      var allImageEntriesDict = new Dictionary<string, ValidationEntry>();
      foreach (var flavor in command.Flavors)
      {
        ReadonlyGeneratedAtlasInformation info = flavor.TryGetResult();
        if (info == null)
          throw new Exception("AtlasFlavor has no result, this is a internal error");

        var textureAtlasCache = new TextureAtlasCache(info.TextureAtlas);

        foreach (var imageName in info.AtlasImageNameSet)
        {
          string imageNameId = imageName.ToUpperInvariant();
          ImageMetaData metaData = ExtractMetaData(imageName, textureAtlasCache);
          if (!allImageEntriesDict.TryGetValue(imageNameId, out ValidationEntry validationEntry))
          {
            validationEntry = new ValidationEntry(imageName, new ImageFlavorRecord(flavor.CreateAtlasCommand, metaData));
            allImageEntriesDict.Add(imageNameId, validationEntry);
          }
          else
          {
            if (imageName != validationEntry.Name)
              throw new Exception($"'{imageName}' from CreateAtlas '{flavor.CreateAtlasCommand.Name}' clashes with '{validationEntry.Name}' defined by the atlases '{GetAtlasNameList(validationEntry.Flavors)}' due to different casing. Flavor atlas image files must use the exact same casing");
            validationEntry.Flavors.Add(new ImageFlavorRecord(flavor.CreateAtlasCommand, metaData));
          }
        }
      }
      return allImageEntriesDict;
    }


    private static ImageMetaData ExtractMetaData(string imageName, TextureAtlasCache textureAtlasCache)
    {
      string encoded = Encoded(imageName, textureAtlasCache);
      return new ImageMetaData(imageName, encoded);
    }

    private static string Encoded(string imageName, TextureAtlasCache textureAtlasCache)
    {

      return $"{{ {EncodeNineSlice(textureAtlasCache, imageName)}, {EncodePatchEntries(textureAtlasCache, imageName)}, {EncodeAnchorPointEntries(textureAtlasCache, imageName)} }}";
    }

    private static string EncodeNineSlice(TextureAtlasCache textureAtlasCache, string imageName)
    {
      return textureAtlasCache.NineSliceDict.ContainsKey(imageName) ? "{NineSlice}" : "{}";
    }

    private static string EncodePatchEntries(TextureAtlasCache textureAtlasCache, string imageName)
    {
      if (!textureAtlasCache.PatchDict.TryGetValue(imageName, out NamedComplexPatch patch))
        return "{}";
      return ResolvedImageMetaData.Encode(patch.ComplexPatch);
    }

    private static string EncodeAnchorPointEntries(TextureAtlasCache textureAtlasCache, string imageName)
    {
      if (!textureAtlasCache.AnchorPointDict.TryGetValue(imageName, out NamedAnchorPoints anchorPoints))
        return "{}";
      return Encode(anchorPoints.AnchorPoints);
    }

    private static string Encode(ImmutableArray<PxPoint2> anchorPoints)
    {
      return $"{{ {anchorPoints.Length} }}";
    }

    private static string GetMissingAtlasNameList(ImmutableArray<ResolvedCommandCreateAtlasFlavor> allArray, List<ImageFlavorRecord> definedList)
    {
      if (allArray == null)
        throw new ArgumentNullException(nameof(allArray));
      if (definedList == null)
        throw new ArgumentNullException(nameof(definedList));
      var missingList = new List<ResolvedCommandCreateAtlas>(allArray.Length);
      foreach (var entry in allArray)
      {
        if (definedList.Find(val => val.Flavor == entry.CreateAtlasCommand) != null)
          missingList.Add(entry.CreateAtlasCommand);
      }
      return CommandAtlasFlavors.GetAtlasNameList(missingList);
    }

    private static string GetAtlasNameList(List<ImageFlavorRecord> source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof(source));
      var names = new string[source.Count];
      for (int i = 0; i < source.Count; ++i)
        names[i] = source[i].Flavor.Name;
      Array.Sort(names);
      return string.Join(',', names);
    }

  }
}
