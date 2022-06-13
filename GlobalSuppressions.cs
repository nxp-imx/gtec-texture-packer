// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Not relevant", Scope = "module")]

[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:FslGraphics.Font.Basic.BasicFont.Ranges")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:FslGraphics.Font.Basic.BasicFont.Kerning")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:FslGraphics.Font.BF.BitmapFont.Kernings")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:FslGraphics.Font.BF.BitmapFont.Chars")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.ResolvedCommandAtlasFlavors.Flavors")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.ResolvedCommandAtlasFlavors.Flavors")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.ResolvedCommandCopyFiles.DynamicLicenseFiles")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.ResolvedCommandCopyFiles.FilesToCopy")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.ResolvedCommandCreateAtlas.Commands")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.ResolvedCommandGroup.CommandList")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.CommandCreateAtlas.Commands")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.License.UnknownLicenseInfo.Blob")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.License.ComplexLicenseInfo.Licenses")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.TexturePack.RemappedFontRecord.TrimInfo")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.TexturePack.EmbeddedFontRecord2.TrimInfo")]
[assembly: SuppressMessage("CodeSmell", "EPS11:Do not embed non-defaultable structs into another structs.", Justification = "<Pending>", Scope = "member", Target = "~F:TexturePacker.Commands.Atlas.ResolvedAtlasCommandAddFolder.ImageFiles")]
[assembly: SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>", Scope = "member", Target = "~M:TexturePacker.Atlas.AtlasElement.Dispose")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Not relevant", Scope = "module")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>", Scope = "member", Target = "~F:MB.Base.MathEx.MathUtil.TO_DEGREES")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>", Scope = "member", Target = "~F:MB.Base.MathEx.MathUtil.TO_RADS")]
[assembly: SuppressMessage("Performance", "EPS02:A non-readonly struct used as in-parameter", Justification = "<Pending>", Scope = "member", Target = "~M:MB.Base.MathEx.Pixel.PxUncheckedTypeConverter.ToPxRectangleU(MB.Base.MathEx.Pixel.PxRectangle@)~MB.Base.MathEx.Pixel.PxRectangleU")]
[assembly: SuppressMessage("Performance", "EPS02:A non-readonly struct used as in-parameter", Justification = "<Pending>", Scope = "member", Target = "~M:MB.Base.MathEx.Pixel.PxUncheckedTypeConverter.ToPxThicknessU(MB.Base.MathEx.Pixel.PxThickness@)~MB.Base.MathEx.Pixel.PxThicknessU")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not relevant for now", Scope = "module")]
[assembly: SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Not relevant", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:TexturePacker.Input.CommandFileDecoder.ParseAtlasCommandAddBitmapFont(System.Xml.Linq.XElement,TexturePacker.Config.AddBitmapFontConfig,TexturePacker.Config.AtlasElementConfig)~TexturePacker.Commands.Atlas.AtlasCommand")]
