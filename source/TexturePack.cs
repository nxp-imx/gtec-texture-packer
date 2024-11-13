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

using FslGraphics.Font.AngleCode;
using FslGraphics.Font.Basic;
using FslGraphics.Font.BF;
using FslGraphics.Font.Process;
using MB.Base;
using MB.Base.Container;
using MB.Base.MathEx.Pixel;
using MB.Encoder.TextureAtlas.BTA;
using MB.Graphics2.Patch.Advanced;
using MB.Graphics2.TextureAtlas.Basic;
using MB.RectangleBinPack.TexturePack;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TexturePacker.Atlas;
using TexturePacker.Commands;
using TexturePacker.Commands.Atlas;
using TexturePacker.Config;
using TexturePacker.License;

namespace TexturePacker
{
  static class TexturePack
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();

    readonly struct CommandSettings
    {
      public readonly string CompanyName;
      public readonly string NamespaceName;
      public readonly UInt32 CreationYear;
      public readonly IOUtil.OverWritePolicy OverwritePolicy;

      public CommandSettings(string companyName, string namespaceName, UInt32 creationYear, IOUtil.OverWritePolicy overwritePolicy)
      {
        CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        NamespaceName = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
        CreationYear = creationYear;
        OverwritePolicy = overwritePolicy;
      }
    }

    readonly struct PackedDict
    {
      public readonly Dictionary<AtlasElement, PackedAtlasImage> LookupDict;

      public PackedDict(Dictionary<AtlasElement, PackedAtlasImage> lookupDict)
      {
        LookupDict = lookupDict;
      }
    }

    struct EmbeddedFontRecord
    {
      public ResolvedAtlasCommandAddBitmapFont SourceCmd;
      public BitmapFont Font;

      public EmbeddedFontRecord(ResolvedAtlasCommandAddBitmapFont sourceCmd, BitmapFont font)
      {
        SourceCmd = sourceCmd ?? throw new ArgumentNullException(nameof(sourceCmd));
        Font = font ?? throw new ArgumentNullException(nameof(font));
      }
    }

    [DoNotUseDefaultConstruction]
    struct EmbeddedFontRecord2
    {
      public ResolvedAtlasCommandAddBitmapFont SourceCmd;
      public BitmapFont Font;
      public ImmutableArray<PxThickness> TrimInfo;

      public EmbeddedFontRecord2(ResolvedAtlasCommandAddBitmapFont sourceCmd, BitmapFont font, ImmutableArray<PxThickness> trimInfo)
      {
        SourceCmd = sourceCmd ?? throw new ArgumentNullException(nameof(sourceCmd));
        Font = font ?? throw new ArgumentNullException(nameof(font));
        TrimInfo = trimInfo;
        if (trimInfo.Length != font.Chars.Length)
          throw new ArgumentException($"trimInfo must contain a entry per char in the font");
      }
    }

    [DoNotUseDefaultConstruction]
    private struct FontRecord
    {
      public FontChar CharInfo;
      public Image CharImage;

      public FontRecord(FontChar charInfo, Image charImage)
      {
        CharInfo = charInfo;
        CharImage = charImage;
      }
    }

    [DoNotUseDefaultConstruction]
    private struct RemappedFontRecord
    {
      public BitmapFont Font;
      public ImmutableArray<PxThickness> TrimInfo;

      public RemappedFontRecord(BitmapFont font, ImmutableArray<PxThickness> trimInfo)
      {
        Font = font ?? throw new ArgumentNullException(nameof(font));
        TrimInfo = trimInfo;
        if (trimInfo.Length != font.Chars.Length)
        {
          throw new ArgumentException($"trimInfo must contain a entry per char in the font");
        }
      }
    }

    static Rectangle ToRectangle(PxRectangle value)
    {
      return new Rectangle(value.X, value.Y, value.Width, value.Height);
    }


    private static TrimInfo ProcessImageElement(ref SafeImage rImage, AtlasElementConfig config)
    {
      var trimMarginPx = new PxThickness();
      var finalTrimmedRectanglePx = new PxRectangle(0, 0, rImage.Size.Width, rImage.Size.Height);
      if (config.Trim)
      {
        var trimmedRectPx = rImage.CalcTrimmedImageRect(config.TransparencyThreshold, config.TrimMargin);
        if (trimmedRectPx.Width < rImage.Size.Width || trimmedRectPx.Height < rImage.Size.Height)
        {
          Debug.Assert(trimmedRectPx.Left >= 0 && trimmedRectPx.Top >= 0 && trimmedRectPx.Right <= rImage.Size.Width && trimmedRectPx.Bottom <= rImage.Size.Height);

          finalTrimmedRectanglePx = trimmedRectPx;
          Debug.Assert(finalTrimmedRectanglePx.Right <= rImage.Size.Width);
          Debug.Assert(finalTrimmedRectanglePx.Bottom <= rImage.Size.Height);
          trimMarginPx = new PxThickness(finalTrimmedRectanglePx.X, finalTrimmedRectanglePx.Y, rImage.Size.Width - finalTrimmedRectanglePx.Right,
                                         rImage.Size.Height - finalTrimmedRectanglePx.Bottom);

          var trimmedImage = rImage.CloneCrop(trimmedRectPx);
          rImage.Dispose();
          rImage = trimmedImage;

          finalTrimmedRectanglePx = PxRectangle.SubLocation(finalTrimmedRectanglePx, trimmedRectPx.Location);

          Debug.Assert(finalTrimmedRectanglePx.Left >= 0 && finalTrimmedRectanglePx.Top >= 0 &&
                       finalTrimmedRectanglePx.Right <= rImage.Size.Width &&
                       finalTrimmedRectanglePx.Bottom <= rImage.Size.Height);
        }
      }
      if (config.ShapePadding > 0 || config.Extrude > 0)
      {
        Debug.Assert(finalTrimmedRectanglePx.Left >= 0 && finalTrimmedRectanglePx.Top >= 0 &&
                     finalTrimmedRectanglePx.Right <= rImage.Size.Width &&
                     finalTrimmedRectanglePx.Bottom <= rImage.Size.Height);

        // Add padding (we basically just add it to the top and left side since when we do that to all elements we get it everywhere
        var srcImage = rImage;
        int padding = config.ShapePadding + (config.Extrude * 2);
        var newImage = new SafeImage(rImage.Size + new PxSize2D(padding, padding));
        int offset = config.ShapePadding + config.Extrude;
        var dstPositionPx = new PxPoint2(offset, offset);
        newImage.DrawImage(dstPositionPx, srcImage);
        if (config.Extrude > 0)
        {
          // Do extrude
          newImage.Extrude(new PxRectangle(dstPositionPx, rImage.Size), config.Extrude);
        }
        rImage = newImage;
        srcImage.Dispose();
        // ensure we offset the trimmed rectangle as well
        finalTrimmedRectanglePx = PxRectangle.AddLocation(finalTrimmedRectanglePx, new PxPoint2(offset, offset));

        Debug.Assert(finalTrimmedRectanglePx.Left >= 0 && finalTrimmedRectanglePx.Top >= 0 &&
                     finalTrimmedRectanglePx.Right <= rImage.Size.Width &&
                     finalTrimmedRectanglePx.Bottom <= rImage.Size.Height);
      }
      return new TrimInfo(finalTrimmedRectanglePx, trimMarginPx);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static Tuple<AtlasBitmapFontElement[], EmbeddedFontRecord> AddBitmapFont(ResolvedAtlasCommandAddBitmapFont cmd)
    {
      g_logger.Trace("AddBitmapFont");
      if (cmd.FilePath.RelativeResolvedSourcePath.EndsWith(".fnt", StringComparison.InvariantCultureIgnoreCase))
      {
        return AddAngleCodeFont(cmd);
      }
      if (cmd.FilePath.RelativeResolvedSourcePath.EndsWith($".{BinaryFontBasicKerning.DefaultFileExtension}", StringComparison.InvariantCultureIgnoreCase))
      {
        return AddBasicFont(cmd);
      }
      throw new NotSupportedException($"Unsupported font format in file: {cmd.FilePath.RelativeResolvedSourcePath}");
    }

    private static Tuple<AtlasBitmapFontElement[], EmbeddedFontRecord> AddAngleCodeFont(ResolvedAtlasCommandAddBitmapFont cmd)
    {
      g_logger.Trace("- Loading angle code font definition {0}", cmd.FilePath.AbsolutePath);

      var config = cmd.ElementConfig;

      var strFontFormat = File.ReadAllText(cmd.FilePath.AbsolutePath);
      var angleFontInfo = AngleCodeFontReader.Decode(strFontFormat);
      var bitmapFont = FslGraphics.Font.Converter.TypeConverter.ToBitmapFont(angleFontInfo, cmd.Type, cmd.TweakConfig.SdfConfig, config.DefaultDpi);
      bitmapFont = FslGraphics.Font.Process.ProcessUtil.Tweak(bitmapFont, cmd.TweakConfig, new TraceInfo("AddBitmapFont", cmd.FilePath.RelativeResolvedSourcePath));
      var fontDir = IOUtil.GetDirectoryName(cmd.FilePath.AbsolutePath);
      var bitmapPath = IOUtil.Combine(fontDir, bitmapFont.TextureName);

      g_logger.Trace("- Loading image {0}", bitmapPath);

      SafeImage? dummyEmptyImage = null;
      using (var fontImage = new SafeImage(Image.Load<Rgba32>(bitmapPath)))
      {
        var result = new AtlasBitmapFontElement[bitmapFont.Chars.Length];
        for (int i = 0; i < bitmapFont.Chars.Length; ++i)
        {
          var fontChar = bitmapFont.Chars[i];
          string charPath = IOUtil.Combine(cmd.RelativeFontAtlasPath, $"{fontChar.Id:X2}");

          if (fontChar.SrcTextureRectPx.Size.Width > 0 && fontChar.SrcTextureRectPx.Height > 0)
          {
            var charImage = fontImage.CloneCrop(fontChar.SrcTextureRectPx);
            var trimInfo = ProcessImageElement(ref charImage, config);
            result[i] = new AtlasBitmapFontElement(charPath, charImage, trimInfo, config.DefaultDpi, bitmapFont, fontChar.Id, i);
          }
          else
          {
            if (dummyEmptyImage == null)
              dummyEmptyImage = new SafeImage();
            result[i] = new AtlasBitmapFontElement(charPath, dummyEmptyImage, new TrimInfo(), config.DefaultDpi, bitmapFont, fontChar.Id, i);
          }
        }
        return Tuple.Create(result, new EmbeddedFontRecord(cmd, bitmapFont));
      }
    }

    private static Tuple<AtlasBitmapFontElement[], EmbeddedFontRecord> AddBasicFont(ResolvedAtlasCommandAddBitmapFont cmd)
    {
      g_logger.Trace("- Loading basic font definition {0}", cmd.FilePath.AbsolutePath);

      var config = cmd.ElementConfig;

      var fontFormat = File.ReadAllBytes(cmd.FilePath.AbsolutePath);
      BasicFont basicFont = BinaryFontBasicKerninDecoder.Decode(fontFormat);
      var bitmapFont = FslGraphics.Font.Converter.TypeConverter.ToBitmapFont(basicFont, cmd.Type, cmd.TweakConfig.SdfConfig, config.DefaultDpi);

      // When we add a basic font like this we assume the fonts glyphs are stored in separate bitmaps

      var fontDir = IOUtil.GetDirectoryName(cmd.FilePath.AbsolutePath);
      //var bitmapPath = IOUtil.Combine(fontDir, bitmapFont.TextureName);

      // Count the glyphs
      var result = new AtlasBitmapFontElement[bitmapFont.Chars.Length];

      // Run through each available glyph
      SafeImage? dummyEmptyImage = null;
      int charIndex = 0;
      foreach (var range in basicFont.Ranges)
      {
        var glyphStartIndex = range.From;
        var glyphEndIndex = range.From + range.Length;
        for (int glyphIndex = glyphStartIndex; glyphIndex < glyphEndIndex; ++glyphIndex)
        {
          UInt32 fontCharId = NumericCast.ToUInt32(glyphIndex);
          string bitmapPath = IOUtil.Combine(fontDir, $"{fontCharId:X}.png");
          string atlasCharPath = IOUtil.Combine(cmd.RelativeFontAtlasPath, $"{fontCharId:X2}");
          if (File.Exists(bitmapPath))
          {
            g_logger.Trace("- Loading image {0}", bitmapPath);
            using (var fontImage = new SafeImage(Image.Load<Rgba32>(bitmapPath)))
            {
              if (fontImage.Size.Width > 0 && fontImage.Size.Height > 0)
              {
                var charImage = fontImage.CloneCrop(new PxRectangle(0, 0, fontImage.Size.Width, fontImage.Size.Height));
                var trimInfo = ProcessImageElement(ref charImage, config);
                result[charIndex] = new AtlasBitmapFontElement(atlasCharPath, charImage, trimInfo, config.DefaultDpi, bitmapFont, fontCharId, charIndex);
              }
              else
              {
                if (dummyEmptyImage == null)
                  dummyEmptyImage = new SafeImage();
                result[charIndex] = new AtlasBitmapFontElement(atlasCharPath, dummyEmptyImage, new TrimInfo(), config.DefaultDpi, bitmapFont,
                                                               fontCharId, charIndex);
              }
            }
          }
          else
          {
            if (dummyEmptyImage == null)
              dummyEmptyImage = new SafeImage();
            result[charIndex] = new AtlasBitmapFontElement(atlasCharPath, dummyEmptyImage, new TrimInfo(), config.DefaultDpi, bitmapFont,
                                                           fontCharId, charIndex);
          }
          ++charIndex;
        }
      }

      // Finally we patch the bitmap font with the actual source rects
      var patchedChars = PatchBitmapFont(bitmapFont, result);
      var patchedBitmapFont = new BitmapFont(bitmapFont.Name, bitmapFont.Dpi, bitmapFont.Size, bitmapFont.LineSpacingPx, bitmapFont.BaseLinePx,
                                             bitmapFont.PaddingPx, bitmapFont.TextureName, bitmapFont.FontType, bitmapFont.SdfSpread,
                                             bitmapFont.SdfDesiredBaseLinePx, patchedChars, bitmapFont.Kernings);
      patchedBitmapFont = FslGraphics.Font.Process.ProcessUtil.Tweak(patchedBitmapFont, cmd.TweakConfig, new TraceInfo("AddBitmapFont", cmd.FilePath.RelativeResolvedSourcePath));

      // Patch the result records
      for (int i = 0; i < result.Length; ++i)
      {
        result[i] = AtlasBitmapFontElement.PatchFont(result[i], patchedBitmapFont);
      }
      return Tuple.Create(result, new EmbeddedFontRecord(cmd, patchedBitmapFont));
    }

    private static ImmutableArray<BitmapFontChar> PatchBitmapFont(BitmapFont bitmapFont, AtlasBitmapFontElement[] result)
    {
      if (bitmapFont.Chars.Length != result.Length)
        throw new Exception("Internal error");

      var builder = ImmutableArray.CreateBuilder<BitmapFontChar>(result.Length);
      for (int i = 0; i < result.Length; ++i)
      {
        var srcChar = bitmapFont.Chars[i];
        UInt32 id = srcChar.Id;
        PxRectangle srcTextureRectPx = result[i].CroppedRectanglePx;
        PxPoint2 offsetPx = srcChar.OffsetPx;
        UInt16 xAdvancePx = srcChar.XAdvancePx;
        builder.Add(new BitmapFontChar(id, srcTextureRectPx, offsetPx, xAdvancePx));
      }
      return builder.MoveToImmutable();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static List<AtlasElement> AddFolder(ResolvedAtlasCommandAddFolder cmd)
    {
      g_logger.Trace("AddFolder '{0}'", cmd.FolderPath.AbsolutePath);

      var result = new List<AtlasElement>(cmd.ImageFiles.Length);
      foreach (var resolvedImageFile in cmd.ImageFiles)
      {
        var atlasElement = CreateImageAtlasElement(resolvedImageFile, cmd.CreateRelativeAtlasPath(resolvedImageFile.AtlasPath.AbsolutePath));
        result.Add(atlasElement);
      }
      return result;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static AtlasElement AddImage(ResolvedAtlasCommandAddImage cmd)
    {
      g_logger.Trace("AddImage '{0}'", cmd.ImageFile.Path.AbsolutePath);

      return CreateImageAtlasElement(cmd.ImageFile, cmd.CreateRelativeAtlasPath(cmd.ImageFile.AtlasPath.AbsolutePath));
    }

    private static AtlasElement CreateImageAtlasElement(ResolvedImageFile srcImageFile, string sourcePath)
    {
      var filePath = srcImageFile.Path.AbsolutePath;
      g_logger.Trace("- Loading image '{0}' with dpi of {1}", filePath, srcImageFile.Dpi);

      var image = new SafeImage(Image.Load<Rgba32>(filePath));

      AtlasElementPatchInfo? patchInfo = null;
      if (srcImageFile.IsPatch)
      {
        patchInfo = ProcessPatchImage(ref image, filePath);
        patchInfo = ApplyTransparency(image, patchInfo);
      }
      else if (srcImageFile.AddNineSlice != null)
      {
        var addNS = srcImageFile.AddNineSlice;
        var complexPatch = ImmutableComplexPatchUtil.CreateExtendedTransparentComplexPatch(addNS.NineSlicePx, addNS.ContentMarginPx, image.Size);
        patchInfo = new AtlasElementPatchInfo(complexPatch, true, false);
        patchInfo = ApplyTransparency(image, patchInfo);
      }
      else if (srcImageFile.AddComplexPatch != null)
      {
        patchInfo = new AtlasElementPatchInfo(srcImageFile.AddComplexPatch.Patch, false, false);
      }

      var trimInfo = ProcessImageElement(ref image, srcImageFile.ElementConfig);
      return new AtlasElement(sourcePath, image, trimInfo, srcImageFile.Dpi, AtlasImageType.Normal, patchInfo, srcImageFile.AddAnchor.Points);
    }

    private static AtlasElementPatchInfo ApplyTransparency(SafeImage image, AtlasElementPatchInfo patchInfo)
    {
      var spanX = patchInfo.ComplexPatch.Slices.AsSpanX();
      var spanY = patchInfo.ComplexPatch.Slices.AsSpanY();
      if (spanX.Length < 2 || spanY.Length < 2)
      {
        throw new NotSupportedException("A patch must contain at least two slices in both X and Y");
      }
      // Determine which spans are considered transparent
      int countX = spanX.Length - 1;
      int countY = spanY.Length - 1;
      int gridSize = countX * countY;
      bool[] transparency = new bool[spanX.Length + spanY.Length];
      var flagSpanX = transparency.AsSpan(0, spanX.Length);
      var flagSpanY = transparency.AsSpan(spanX.Length, spanY.Length);
      var gridFlags = new ComplexPatchGridFlags[gridSize];

      int yOffset = 0;
      for (int y = 0; y < countY; ++y)
      {
        for (int x = 0; x < countX; ++x)
        {
          var srcRectPx = PxRectangle.FromLeftTopRightBottom(spanX[x].Position, spanY[y].Position, spanX[x + 1].Position, spanY[y + 1].Position);
          if (!image.IsOpaque(srcRectPx))
          {
            flagSpanX[x] = true;
            flagSpanY[y] = true;
            gridFlags[yOffset + x] = ComplexPatchGridFlags.Transparent;
          }
        }
        yOffset += countX;
      }

      // finally generate updated complex patch slices with this information
      ImmutableComplexPatchSlice[] finalSpans = new ImmutableComplexPatchSlice[spanX.Length + spanY.Length];
      var finalSpanX = finalSpans.AsSpan(0, spanX.Length);
      var finalSpanY = finalSpans.AsSpan(spanX.Length, spanY.Length);
      FillSpan(finalSpanX, spanX, flagSpanX);
      FillSpan(finalSpanY, spanY, flagSpanY);

      var newArraySegment = ReadOnlyArraySegment.Create(finalSpans);

      var newPatchSilces = new ImmutableComplexPatchSlices(newArraySegment, patchInfo.ComplexPatch.Slices.CountX, patchInfo.ComplexPatch.Slices.CountY,
                                                           patchInfo.ComplexPatch.Slices.Flags);

      var newGridFlags = new ReadOnlyArraySegment<ComplexPatchGridFlags>(gridFlags, 0, gridFlags.Length);
      var newPatch = new ImmutableComplexPatch(newPatchSilces, patchInfo.ComplexPatch.ContentSpans, newGridFlags);


      return new AtlasElementPatchInfo(newPatch, patchInfo.AllowConvertPatchToNineSlice,
                                       patchInfo.AllowContentMarginToExceedImageBoundary);
    }

    private static void FillSpan(Span<ImmutableComplexPatchSlice> finalSpan, ReadOnlySpan<ImmutableComplexPatchSlice> srcSpan, Span<bool> flagSpan)
    {
      Debug.Assert(finalSpan.Length == srcSpan.Length);
      Debug.Assert(srcSpan.Length == flagSpan.Length);
      for (int i = 0; i < finalSpan.Length; ++i)
      {
        var finalFlags = (srcSpan[i].Flags & (~ComplexPatchSliceFlags.Transparent)) | (flagSpan[i] ? ComplexPatchSliceFlags.Transparent : ComplexPatchSliceFlags.None);
        finalSpan[i] = new ImmutableComplexPatchSlice(srcSpan[i].Position, finalFlags);
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static AtlasElementPatchInfo ProcessPatchImage(ref SafeImage rImage, string debugPath)
    {
      if (rImage.Size.Width < 3 || rImage.Size.Height < 3)
        throw new Exception($"Patch images must be atleast 3x3 pixels '{debugPath}'");

      AtlasElementPatchInfo? patchInfo = rImage.TryProcessPatchInfo();
      if (patchInfo == null)
        throw new Exception("Failed to acquire patch info");

      // Remove the 'patch' area
      {
        var trimmedImage = rImage.CloneCrop(new PxThicknessU(1, 1, 1, 1));
        rImage.Dispose();
        rImage = trimmedImage;
      }

      if (g_logger.IsEnabled(LogLevel.Trace))
      {
        g_logger.Trace("- Image resolution {0}x{1} found patch: left {2}, top {3}, right {4}, bottom {5}", rImage.Size.Width, rImage.Size.Height,
          ToString(patchInfo.ComplexPatch.Slices.AsSpanY()), ToString(patchInfo.ComplexPatch.Slices.AsSpanX()),
          ToString(patchInfo.ComplexPatch.ContentSpans.AsSpanY()), ToString(patchInfo.ComplexPatch.ContentSpans.AsSpanX()));
      }

      return patchInfo;
    }

    private static string ToString(ReadOnlySpan<ImmutableComplexPatchSlice> slices)
    {
      if (slices.Length == 0)
        return "[]";

      string res = $"[{slices[0]}";
      for (int i = 1; i < slices.Length; ++i)
      {
        res += $", {slices[i]}";

      }
      return res + "]";
    }

    private static string ToString(ReadOnlySpan<ImmutableContentSpan> spans)
    {
      if (spans.Length == 0)
        return "[]";

      string res = $"[{spans[0]}";
      for (int i = 1; i < spans.Length; ++i)
      {
        res += $", {spans[i]}";

      }
      return res + "]";
    }

    private static Tuple<List<AtlasElement>, List<EmbeddedFontRecord>> PrepareAtlasElements(ImmutableArray<ResolvedAtlasCommand> commands)
    {
      var embeddedFonts = new List<EmbeddedFontRecord>();
      var atlasElements = new List<AtlasElement>();
      // Parallel.ForEach
      foreach (var command in commands)
      {
        switch (command.Id)
        {
          case AtlasCommandId.AddBitmapFont:
            {
              var (range, emeddedFont) = AddBitmapFont((ResolvedAtlasCommandAddBitmapFont)command);
              embeddedFonts.Add(emeddedFont);
              atlasElements.AddRange(range);
            }
            break;
          case AtlasCommandId.AddFolder:
            {
              var range = AddFolder((ResolvedAtlasCommandAddFolder)command);
              atlasElements.AddRange(range);
            }
            break;
          case AtlasCommandId.AddImage:
            {
              var result = AddImage((ResolvedAtlasCommandAddImage)command);
              atlasElements.Add(result);
            }
            break;
          default:
            throw new NotSupportedException($"Unsupported commandId {command.Id}");
        }
      }
      return Tuple.Create(atlasElements, embeddedFonts);
    }

    public struct PackedDuplicateElement
    {
      public AtlasElement Element;
      public AtlasElement DuplicateOf;

      public PackedDuplicateElement(AtlasElement element, AtlasElement duplicateOf)
      {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        DuplicateOf = duplicateOf ?? throw new ArgumentNullException(nameof(duplicateOf));
      }
    }

    public struct PackedElements
    {
      public List<AtlasImageInfo> ElementsToPack;
      public List<PackedDuplicateElement> DuplicatedElements;


      public PackedElements(List<AtlasImageInfo> elementsToPack, List<PackedDuplicateElement> duplicatedElements)
      {
        ElementsToPack = elementsToPack ?? throw new ArgumentNullException(nameof(elementsToPack));
        DuplicatedElements = duplicatedElements ?? throw new ArgumentNullException(nameof(duplicatedElements));
      }
    }


    private static PackedElements PrepareTexturePacking(List<AtlasElement> elements)
    {
      var duplicateIndexDict = DuplicateImageDetector.TryDetectDuplicates(elements);

      var srcAtlasImages = new List<AtlasImageInfo>(elements.Count);
      var duplicatedAtlasImages = new List<PackedDuplicateElement>(elements.Count);
      for (int i = 0; i < elements.Count; ++i)
      {
        var entry = elements[i];
        var elementEx = (AtlasElement)entry;
        if (duplicateIndexDict == null || !duplicateIndexDict.TryGetValue(i, out int initialDuplicateIndex))
        {
          // Not a duplicate so just add it to the normal list
          srcAtlasImages.Add(new AtlasImageInfo(elementEx.CroppedRectanglePx, elementEx));
        }
        else
        {
          var duplicateOf = (AtlasElement)elements[initialDuplicateIndex];
          duplicatedAtlasImages.Add(new PackedDuplicateElement(elementEx, duplicateOf));
        }
      }
      return new PackedElements(srcAtlasImages, duplicatedAtlasImages);
    }

    private static SafeImage CreateAtlasImage(TextureBinPacker.PackResult packResult, PxPoint2 offsetPx, TransparencyMode transparencyMode)
    {
      Debug.Assert(packResult.IsValid);

      var atlasImage = new SafeImage(packResult.Size);

      var filteredImages = new List<PackedAtlasImage>(packResult.Images.Count);
      DrawImages(packResult.Size, packResult.Images, offsetPx, atlasImage, AtlasImageType.Normal, filteredImages);

      // Apply transparency mode
      ApplyTransparencyMode(atlasImage, transparencyMode);

      if (filteredImages.Count > 0)
      {
        DrawImages(packResult.Size, filteredImages, offsetPx, atlasImage, AtlasImageType.Sdf);
      }

      return atlasImage;
    }

    private static void DrawImages(PxSize2D dstImageSize, List<PackedAtlasImage> images, PxPoint2 offsetPx, SafeImage atlasImage,
                                   AtlasImageType drawImageType, List<PackedAtlasImage>? filteredImages = null)
    {
      foreach (var packEntry in images)
      {
        var atlasElement = (AtlasElement)packEntry.SrcImageInfo.SourceTag;
        var dstPositionPx = offsetPx + packEntry.DstRectanglePx.Location;
        if (atlasElement.ImageType == drawImageType)
        {
          Debug.Assert(packEntry.DstRectanglePx.Size.Width == atlasElement.SourceImage.Size.Width);
          Debug.Assert(packEntry.DstRectanglePx.Size.Height == atlasElement.SourceImage.Size.Height);
          Debug.Assert((dstPositionPx.X + packEntry.DstRectanglePx.Size.Width) <= dstImageSize.Width);
          Debug.Assert((dstPositionPx.Y + packEntry.DstRectanglePx.Size.Height) <= dstImageSize.Height);

          if (packEntry.DstRectanglePx.Size.Width > 0 && packEntry.DstRectanglePx.Size.Height > 0)
          {
            atlasImage.DrawImage(dstPositionPx, atlasElement.SourceImage);
          }
        }
        else if (filteredImages != null)
        {
          filteredImages.Add(packEntry);
        }
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static BasicTextureAtlas CreateBasicTextureAtlas(TextureBinPacker.PackResult packResult, PackedDict packedDict,
                                                             List<PackedDuplicateElement> duplicatedElements, string debugName,
                                                             UInt16 defaultDpi)
    {
      g_logger.Trace("CreateBasicTextureAtlas {0}", debugName);
      Debug.Assert(duplicatedElements != null);
      var nineSliceEntries = new List<NamedAtlasNineSlice>();
      var patchEntries = new List<NamedComplexPatch>();
      var anchorPointEntries = new List<NamedAnchorPoints>();

      var atlasEntries = new NamedAtlasTexture[packResult.Images.Count + duplicatedElements.Count];
      for (int i = 0; i < packResult.Images.Count; ++i)
      {
        var packedImage = packResult.Images[i];
        if (packedImage.IsRotated)
          throw new NotSupportedException("rotation not supported");

        var source = (AtlasElement)packedImage.SrcImageInfo.SourceTag;
        //g_logger.Trace("- Adding '{0}'", source.SourcePath);

        string name = source.SourcePath;
        var trimMarginPx = PxUncheckedTypeConverter.ToPxThicknessU(source.SourceTrimInfo.MarginPx);

        // Shift the source rectangle by the dstRectangle offset (so we get a trimmed rect offset to the atlas location)
        var sourceTrimmedRectPx = PxRectangle.AddLocation(source.SourceTrimInfo.RectanglePx, packedImage.DstRectanglePx.Location);

        var trimmedRectPx = PxUncheckedTypeConverter.ToPxRectangleU(sourceTrimmedRectPx);

        UInt32 dpi = UncheckedNumericCast.ToUInt32(source.Dpi);
        var textureInfo = new AtlasTextureInfo(trimmedRectPx, trimMarginPx, dpi);
        atlasEntries[i] = new NamedAtlasTexture(name, textureInfo);
        if (source.PatchInfo != null)
        {
          ProcessPatch(nineSliceEntries, patchEntries, source, name);
        }
        if (source.AnchorPoints.Length > 0)
        {
          ProcessAnchorPoints(anchorPointEntries, source, name);
        }
      }

      // Write the duplicate entries
      if (packResult.Images.Count > 0)
      {
        int dstIdx = packResult.Images.Count;
        for (int srcIdx = 0; srcIdx < duplicatedElements.Count; ++srcIdx)
        {
          var source = duplicatedElements[srcIdx].Element;
          //g_logger.Trace("- Adding '{0}'", source.SourcePath);

          string name = source.SourcePath;
          UInt32 dpi = UncheckedNumericCast.ToUInt32(source.Dpi);
          // We take the trim margin from the duplicate entry (since its not necessarily the same as the duplication source)
          // since the duplication check works on the trimmed and padded final texture atlas image
          var trimMarginPx = PxUncheckedTypeConverter.ToPxThicknessU(source.SourceTrimInfo.MarginPx);
          var sourceTrimmedRectPx = source.SourceTrimInfo.RectanglePx;

          // Shift the source rectangle by the dstRectangle offset (so we get a trimmed rect offset to the atlas location)
          var packedImage = packedDict.LookupDict[duplicatedElements[srcIdx].DuplicateOf];
          sourceTrimmedRectPx = PxRectangle.AddLocation(sourceTrimmedRectPx, packedImage.DstRectanglePx.Location);

          var trimmedRectPx = PxUncheckedTypeConverter.ToPxRectangleU(sourceTrimmedRectPx);
          var textureInfo = new AtlasTextureInfo(trimmedRectPx, trimMarginPx, dpi);
          atlasEntries[dstIdx] = new NamedAtlasTexture(name, textureInfo);
          ++dstIdx;
          if (source.PatchInfo != null)
          {
            ProcessPatch(nineSliceEntries, patchEntries, source, name);
          }
          if (source.AnchorPoints.Length > 0)
          {
            ProcessAnchorPoints(anchorPointEntries, source, name);
          }
        }
        Debug.Assert(dstIdx == atlasEntries.Length);
      }

      return new BasicTextureAtlas(atlasEntries, nineSliceEntries.ToArray(), patchEntries.ToArray(), anchorPointEntries.ToArray(), defaultDpi);
    }

    private static void ProcessAnchorPoints(List<NamedAnchorPoints> anchorPointEntries, AtlasElement source, string name)
    {
      anchorPointEntries.Add(new NamedAnchorPoints(name, source.AnchorPoints));
    }

    private static void ProcessPatch(List<NamedAtlasNineSlice> nineSliceEntries, List<NamedComplexPatch> patchEntries, AtlasElement source, string name)
    {
      if (source.PatchInfo == null)
        throw new Exception("internal error, no patch to process");

      if (source.PatchInfo.AllowConvertPatchToNineSlice && ImmutablePatchHelper.IsNineSlicePatch(source.PatchInfo.ComplexPatch))
        nineSliceEntries.Add(ProcessComplexPatchAsNineSlice(name, source.OriginalSizePx, source.PatchInfo));
      else
        patchEntries.Add(ProcessPatchInfoAsPatch(name, source.OriginalSizePx, source.PatchInfo));
    }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static NamedAtlasNineSlice ProcessComplexPatchAsNineSlice(string name, PxSize2D imageSize, in AtlasElementPatchInfo patchInfo)
    {
      Debug.Assert(ImmutablePatchHelper.IsNineSlicePatch(patchInfo.ComplexPatch));

      g_logger.Trace("Processing nine slice for '{0}'", name);

      AtlasNineSliceInfo nineslice = ImmutablePatchHelper.ProcessComplexPatchAsNineSlice(patchInfo, imageSize, name);

      g_logger.Trace("Adding nineslice info to '{0}' {1}", name, nineslice);
      return new NamedAtlasNineSlice(name, nineslice);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static NamedComplexPatch ProcessPatchInfoAsPatch(string name, PxSize2D imageSize, AtlasElementPatchInfo patchInfo)
    {
      g_logger.Trace("Processing patch info for '{0}'", name);

      UInt32 imageWidthPx = UncheckedNumericCast.ToUInt32(imageSize.Width);
      UInt32 imageHeightPx = UncheckedNumericCast.ToUInt32(imageSize.Height);

      // Validate that the slices are inside the image dimensions
      ValidateSlices(patchInfo.ComplexPatch.Slices.AsSpanX(), imageWidthPx, name);
      ValidateSlices(patchInfo.ComplexPatch.Slices.AsSpanY(), imageHeightPx, name);

      return new NamedComplexPatch(name, patchInfo.ComplexPatch);
    }

    private static void ValidateSlices(ReadOnlySpan<ImmutableComplexPatchSlice> slices, uint maxSizePx, string debugName)
    {
      if (slices.Length < 2)
        throw new Exception("internal error invalid patch");

      int previousPosition = -1;
      for (int i = 0; i < slices.Length - 1; ++i)
      {
        ref readonly var slice = ref slices[i];
        if (slice.Position < previousPosition || slice.Position >= maxSizePx)
          throw new Exception($"patch error '{debugName}' the slice did not fit the image. Size {maxSizePx} slice[{i}]: {slice}");
      }
      if (slices[slices.Length - 1].Position != maxSizePx)
        throw new Exception($"patch error '{debugName}' the slice did not fit the image the end slice must match the image size. Size {maxSizePx}!={slices[slices.Length - 1].Position}");
    }

    private static void SaveAtlas(string atlasName, BasicTextureAtlas basicTextureAtlas, OutputAtlasFormat outputFormat,
                                  string companyName, string namespaceName, UInt32 creationYear, IOUtil.OverWritePolicy overwritePolicy)
    {
      byte[] bytes;
      string formatExtension;
      switch (outputFormat)
      {
        case OutputAtlasFormat.BTA3:
          {
            var encoder = new BTAEncoder();
            formatExtension = encoder.DefaultExtension;
            bytes = encoder.Encode(basicTextureAtlas, BtaVersion.V3).ToArray();
          }
          break;
        case OutputAtlasFormat.BTA4:
          {
            var encoder = new BTAEncoder();
            formatExtension = encoder.DefaultExtension;
            bytes = encoder.Encode(basicTextureAtlas, BtaVersion.V4).ToArray();
          }
          break;
        case OutputAtlasFormat.BTA4CSharp:
          {
            var encoder = new BTACSharpEncoder();
            formatExtension = encoder.DefaultExtension;
            bytes = encoder.Encode(basicTextureAtlas, BtaVersion.V4, Path.GetFileNameWithoutExtension(atlasName), companyName, namespaceName, creationYear).ToArray();
          }
          break;
        default:
          throw new NotSupportedException($"Unsupported output format {outputFormat}");
      }

      var dstFilename = $"{atlasName}.{formatExtension}";
      IOUtil.SaveFileIfModified(dstFilename, bytes, overwritePolicy);
    }

    private static void SaveFont(string dstFilenameWithoutExtension, EmbeddedFontRecord2 fontRecord, IOUtil.OverWritePolicy overwritePolicy,
                                 string companyName, string namespaceName, UInt32 creationYear)
    {
      byte[] bytes;
      string formatExtension;
      foreach (var outputFormat in fontRecord.SourceCmd.OutputFontFormats)
      {
        switch (outputFormat)
        {
          case OutputFontFormat.FBK:
            {
              var basicFont = FslGraphics.Font.Converter.TypeConverter.ToBasicFont(fontRecord.Font, fontRecord.TrimInfo, fontRecord.SourceCmd.RelativeFontAtlasPath);
              var encoder = new BinaryFontBasicKerningEncoder();
              formatExtension = encoder.DefaultExtension;
              bytes = encoder.Encode(basicFont).ToArray();
            }
            break;
          case OutputFontFormat.NBF:
            {
              var encoder = new BitmapFontEncoder();
              formatExtension = encoder.DefaultExtension;
              bytes = encoder.Encode(fontRecord.Font).ToArray();
            }
            break;
          case OutputFontFormat.NBFCSharp:
            {
              var encoder = new BitmapFontEncoderCSharp();
              formatExtension = encoder.DefaultExtension;
              bytes = encoder.Encode(fontRecord.Font, Path.GetFileNameWithoutExtension(dstFilenameWithoutExtension), companyName, namespaceName, creationYear).ToArray();
            }
            break;
          case OutputFontFormat.JsonBitmapFont:
            {
              var encoder = new BitmapFontJsonEncoder();
              formatExtension = encoder.DefaultExtension;
              bytes = encoder.Encode(fontRecord.Font).ToArray();
            }
            break;
          default:
            throw new NotSupportedException($"Unsupported output format {outputFormat}");
        }

        var dstFilename = $"{dstFilenameWithoutExtension}.{formatExtension}";
        IOUtil.SaveFileIfModified(dstFilename, bytes, overwritePolicy);
      }
    }

    private static PackedDict CreatePackDict(TextureBinPacker.PackResult packResult)
    {
      var lookupDict = new Dictionary<AtlasElement, PackedAtlasImage>(packResult.Images.Count);
      foreach (var entry in packResult.Images)
      {
        lookupDict[(AtlasElement)entry.SrcImageInfo.SourceTag] = entry;
      }
      return new PackedDict(lookupDict);
    }

    private static PackedDict CreatePackDictWithDuplicates(PackedDict packDict, List<PackedDuplicateElement> duplicatedElements)
    {
      var srcLookupDict = packDict.LookupDict;
      var lookupDict = new Dictionary<AtlasElement, PackedAtlasImage>(srcLookupDict);
      foreach (var entry in duplicatedElements)
      {
        var duplicateOf = srcLookupDict[entry.DuplicateOf];
        var patchedImageInfo = new AtlasImageInfo(duplicateOf.SrcImageInfo.SrcRectPx, entry.Element);
        lookupDict[entry.Element] = new PackedAtlasImage(patchedImageInfo, duplicateOf.DstRectanglePx, duplicateOf.IsRotated);
      }
      return new PackedDict(lookupDict);
    }

    private static ReadonlyGeneratedAtlasInformation? TryCreateAtlas(ResolvedCommandCreateAtlas createAtlasCmd, in CommandSettings commandSettings, bool generateAtlasInformation)
    {
      g_logger.Trace(nameof(TryCreateAtlas));

      var atlasImagePathSet = new HashSet<string>();
      List<AtlasElement>? atlasElements = null;
      BasicTextureAtlas? basicTextureAtlas = null;
      try
      {
        var (newAtlasElements, embeddedFonts) = PrepareAtlasElements(createAtlasCmd.Commands);
        atlasElements = newAtlasElements;
        var packElements = PrepareTexturePacking(atlasElements);
        g_logger.Trace("Packing {0} elements, found {1} duplicates", packElements.ElementsToPack.Count, packElements.DuplicatedElements.Count);

        // Since the border and shape padding is the same number on all sides we just need to take into account that
        // all shapes are padded on top, left with the shape margin (which means that we just need to apply a small margin on left,top and a bigger one on right, bottom
        int minBorderMarginLeft = Math.Max(createAtlasCmd.Config.Element.BorderPadding - createAtlasCmd.Config.Element.ShapePadding, 0);
        int minBorderMarginRight = Math.Max(createAtlasCmd.Config.Element.BorderPadding, createAtlasCmd.Config.Element.ShapePadding);

        var borderReservedPx = new PxThickness(minBorderMarginLeft, minBorderMarginLeft, minBorderMarginRight, minBorderMarginRight);
        var packer = new TextureBinPacker(createAtlasCmd.Config.Texture.MaxSize, createAtlasCmd.Config.Texture.SizeRestriction, createAtlasCmd.Config.Layout.AllowRotation, borderReservedPx);
        TextureBinPacker.PackResult res = packer.TryProcess(packElements.ElementsToPack);
        if (!res.IsValid)
          throw new Exception("Failed to pack elements");

        g_logger.Trace("Packed to {0}x{0} texture", res.Size.Width, res.Size.Height);

        {
          using (var atlasImage = CreateAtlasImage(res, new PxPoint2(minBorderMarginLeft, minBorderMarginLeft), createAtlasCmd.Config.TransparencyMode))
          { // Save the atlas image
            IOUtil.CreateFileDirectoryIfMissing(createAtlasCmd.DstAtlasFilename.AbsolutePath);
            SaveImageIfModified(createAtlasCmd.DstAtlasFilename.AbsolutePath, atlasImage, commandSettings.OverwritePolicy);
          }
        }

        PackedDict packDict = CreatePackDict(res);

        basicTextureAtlas = CreateBasicTextureAtlas(res, packDict, packElements.DuplicatedElements, createAtlasCmd.Name, createAtlasCmd.Config.Element.DefaultDpi);

        { // Save the atlas information
          IOUtil.CreateFileDirectoryIfMissing(createAtlasCmd.DstAtlasFilename.AbsolutePath);
          SaveAtlas(createAtlasCmd.DstAtlasFilename.AbsolutePath, basicTextureAtlas, createAtlasCmd.OutputFormat, commandSettings.CompanyName,
                    commandSettings.NamespaceName, commandSettings.CreationYear, commandSettings.OverwritePolicy);
        }

        PackedDict packDictWithDuplicates = CreatePackDictWithDuplicates(packDict, packElements.DuplicatedElements);

        if (generateAtlasInformation)
        {
          foreach (var entry in packDictWithDuplicates.LookupDict.Keys)
          {
            if (atlasImagePathSet.Contains(entry.SourcePath))
              throw new Exception($"Duplicated path entry found for '{entry.SourcePath}'");
            atlasImagePathSet.Add(entry.SourcePath);
          }
        }

        { // Save font information
          foreach (var embedded in embeddedFonts)
          {
            var remappedFontRecord = RemapFont(embedded.Font, packDictWithDuplicates, Path.GetFileName(createAtlasCmd.DstAtlasFilename.AbsolutePath));

            var dstFilename = embedded.SourceCmd.DstFilename.AbsolutePath;

            IOUtil.CreateFileDirectoryIfMissing(dstFilename);
            SaveFont(dstFilename, new EmbeddedFontRecord2(embedded.SourceCmd, remappedFontRecord.Font, remappedFontRecord.TrimInfo),
                     commandSettings.OverwritePolicy, commandSettings.CompanyName, commandSettings.NamespaceName, commandSettings.CreationYear);
          }
        }
      }
      finally
      {
        if (atlasElements != null)
        {
          foreach (var entry in atlasElements)
          {
            entry.Dispose();
          }
          atlasElements = null;
        }
      }
      return generateAtlasInformation ? new ReadonlyGeneratedAtlasInformation(ImmutableHashSet.Create(atlasImagePathSet.ToArray()), basicTextureAtlas) : null;
    }


    private static void ApplyTransparencyMode(SafeImage atlasImage, TransparencyMode transparencyMode)
    {
      switch (transparencyMode)
      {
        case TransparencyMode.Normal:
          return;
        case TransparencyMode.Premultiply:
          atlasImage.Premultiply();
          return;
        case TransparencyMode.PremultiplyUsingLinearColors:
          atlasImage.PremultiplyUsingLinearColors();
          return;
        default:
          throw new NotSupportedException($"Unsupported transparency mode: {transparencyMode}");
      }
    }

    private static Dictionary<UInt32, PackedAtlasImage> FilterToFontChars(PackedDict packedDict, BitmapFont bitmapFont)
    {
      var filteredDict = new Dictionary<UInt32, PackedAtlasImage>(packedDict.LookupDict.Count);
      foreach (var entry in packedDict.LookupDict.Values)
      {
        var fontElement = entry.SrcImageInfo.SourceTag as AtlasBitmapFontElement;
        if (fontElement != null && fontElement.Font == bitmapFont)
        {
          filteredDict[fontElement.CharId] = entry;
        }
      }
      return filteredDict;
    }


    private static RemappedFontRecord RemapFont(BitmapFont embeddedFont, PackedDict packedDictWithDuplicates, string atlasImageFilename)
    {
      var trimInfo = new PxThickness[embeddedFont.Chars.Length];

      var filteredDict = FilterToFontChars(packedDictWithDuplicates, embeddedFont);

      // We now use final the atlas texture name for the remapped font and
      // we remap the chars to the packed texture location
      var remappedChars = new BitmapFontChar[embeddedFont.Chars.Length];
      for (int i = 0; i < remappedChars.Length; ++i)
      {
        var record = embeddedFont.Chars[i];
        {
          if (!filteredDict.TryGetValue(record.Id, out var packedRecord))
          {
            throw new Exception($"Could not find packed bitmap entry for charId {record.Id}");
          }
          if (packedRecord.IsRotated)
            throw new NotSupportedException("Font elements can not be rotated");

          var fontElement = (AtlasBitmapFontElement)packedRecord.SrcImageInfo.SourceTag;
          record.SrcTextureRectPx = PxRectangle.AddLocation(fontElement.SourceTrimInfo.RectanglePx, packedRecord.DstRectanglePx.Location);
          record.OffsetPx += new PxPoint2(fontElement.SourceTrimInfo.MarginPx.Left, fontElement.SourceTrimInfo.MarginPx.Top);
          trimInfo[i] = fontElement.SourceTrimInfo.MarginPx;
        }
        remappedChars[i] = record;
      }
      var bf = new BitmapFont(embeddedFont.Name, embeddedFont.Dpi, embeddedFont.Size, embeddedFont.LineSpacingPx, embeddedFont.BaseLinePx,
                              embeddedFont.PaddingPx, atlasImageFilename, embeddedFont.FontType, embeddedFont.SdfSpread,
                              embeddedFont.SdfDesiredBaseLinePx, remappedChars, embeddedFont.Kernings);
      return new RemappedFontRecord(bf, ImmutableArray.Create(trimInfo));
    }


    private static void CopyFiles(ResolvedCommandCopyFiles command, in CommandSettings commandSettings)
    {
      g_logger.Trace("CopyFiles");
      g_logger.Trace("- File copy");
      foreach (var entry in command.FilesToCopy)
      {
        IOUtil.CopyIfDifferent(entry.From, entry.To, commandSettings.OverwritePolicy);
      }

      g_logger.Trace("- Dynamic license files");
      var encoder = new LicenseInfoNxpJsonEncoder();
      foreach (var entry in command.DynamicLicenseFiles)
      {
        var license = (ComplexLicenseInfo)entry.LicenseInfo;
        var jsonText = encoder.Encode(license);
        IOUtil.SaveFileIfModified(entry.To, jsonText, commandSettings.OverwritePolicy);
      }
    }

    private static void ProcessCommand(ResolvedCommand command, in CommandSettings commandSettings)
    {
      switch (command.Id)
      {
        case CommandId.AtlasFlavors:
          // This is a pure post process, so we skip it here
          break;
        case CommandId.CreateAtlasFlavor:
          {
            // Create the atlas and generate the information about it that can be used to validate the atlas flavor against all other flavors
            var commandEx = (ResolvedCommandCreateAtlasFlavor)command;
            ReadonlyGeneratedAtlasInformation? generatedAtlasInformation = TryCreateAtlas(commandEx.CreateAtlasCommand, commandSettings, true);
            commandEx.SetResult(generatedAtlasInformation);
          }
          break;
        case CommandId.CreateAtlas:
          TryCreateAtlas((ResolvedCommandCreateAtlas)command, commandSettings, false);
          break;
        case CommandId.CopyFiles:
          CopyFiles((ResolvedCommandCopyFiles)command, commandSettings);
          break;
        default:
          throw new NotSupportedException($"Unsupported commandId {command.Id}");
      }
    }


    private static void PostProcessCommand(ResolvedCommand command)
    {
      switch (command.Id)
      {
        case CommandId.AtlasFlavors:
          PostProcessAtlasFlavorValidation.Validate((ResolvedCommandAtlasFlavors)command);
          break;
        default:
          break;
      }
    }

    private static void ProcessCommands(ResolvedCommandGroup commandGroup, bool allowThreads,
                                      IOUtil.OverWritePolicy overwritePolicy)
    {
      g_logger.Trace("ProcessCommands");

      if (commandGroup == null)
        throw new ArgumentNullException(nameof(commandGroup));

      var commandSettings = new CommandSettings(commandGroup.CompanyName, commandGroup.NamespaceName, commandGroup.CreationYear, overwritePolicy);

      if (!allowThreads)
      {
        foreach (var command in commandGroup.CommandList)
        {
          ProcessCommand(command, commandSettings);
        }

        foreach (var command in commandGroup.CommandList)
        {
          PostProcessCommand(command);
        }
      }
      else
      {
        Parallel.ForEach(commandGroup.CommandList, (command) =>
        {
          ProcessCommand(command, commandSettings);
        });

        Parallel.ForEach(commandGroup.CommandList, (command) =>
        {
          PostProcessCommand(command);
        });
      }
    }



    public static void Process(CommandGroup unresolvedCommandGroup, bool allowThreads,
                               bool disableLicenseFiles, IOUtil.OverWritePolicy overwritePolicy)
    {
      var imageExtensionsSet = new HashSet<string>() { ".PNG", ".JPG" };

      var resolvedCommandGroup = unresolvedCommandGroup.Resolve(imageExtensionsSet.ToImmutableHashSet<string>(), disableLicenseFiles);
      ProcessCommands(resolvedCommandGroup, allowThreads, overwritePolicy);
    }



    private static void SaveImageIfModified(string dstFilename, SafeImage image, IOUtil.OverWritePolicy overwritePolicy)
    {
      if (dstFilename == null)
        throw new ArgumentNullException(nameof(dstFilename));
      if (image == null)
        throw new ArgumentNullException(nameof(image));

      var dstFilenameFinal = $"{dstFilename}.png";

      byte[]? serialized = null;
      using (var stream = new MemoryStream())
      {
        image.Save(stream, SafeImage.ImageFormat.Png);
        serialized = stream.ToArray();
      }

      IOUtil.SaveFileIfModified(dstFilenameFinal, serialized, overwritePolicy);
    }
  }
}
