# Texture Packer

A texture atlas generation tool that helps create complex texture atlases.

## Why

With the new DPI aware UI framework we have a lot requirements to the atlas and font generation and are more likely to modify and experiment with the atlas content. So having better tools for the generation will save a lot of time.

## Features

* Texture atlas creation from a input xml
* Trim support (global and per element override)
* Shape padding (global and per element override)
* Border padding
* Duplicate detection
* Per element DPI tagging
* Smart image addition
  * Add elements to atlas
  * Add folder
  * Add angle code font
  * License file support
    * Create a merged license file for atlases.
    * Warn about missing licenses for atlas images.
* Supports both normal and pre-multiplied image output
* Writes
  * Atlas image: png
  * Binary texture atlas: bta
  * Basic font kerning: fbk
  * Bitmap fonts format: nbf, jsonBitmapFont
* AtlasFlavor support
  * Validate that texture atlases created for various resolutions (flavors) contain the same images and meta data,
    so errors can be detected during creation instead of at runtime when selecting the resolution.
* Build and version control friendly output:  
  * All output files should be deterministic given the same input.
  * Will never overwrite a file if no changes were made.
* Default settings are read from a config file
  * The default configuration is created like this:
    * Set the hard-coded configuration
    * If a configuration file exist next to the executable we apply the changes from that.
    * We then look for a configuration file next to the command file and if that is not found we search for the nearest configuration file in all parent directories and if one is found we apply its changes.

## Configuration

### TexturePackerConfig

Configures all settings used for pacing

Name                 | Description
---------------------|-------------------------
Version              | Current "1"
DefaultCompany       | The default company to associate with written atlases (for encoders that support it). This can be overridden per atlas.

```xml
<TexturePackerConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="1" DefaultCompany="NXP">
  ...
</TexturePackerConfig>
```

#### CreateAtlasConfig

Configures all settings used for creating texture atlases.

Name                 | Description
---------------------|-------------------------
OutputAtlasFormat    | Set the default atlas format. Can be: *bta3, bta4, bta4C#*

```xml
<CreateAtlasConfig OutputAtlasFormat="bta4">
  ...
</CreateAtlasConfig>
```

##### AtlasConfig

Name                 | Description
---------------------|-------------------------
TransparencyMode     | Set the transparency mode. Can be: *normal, premultiply, premultiply-linear*

```xml
<AtlasConfig TransparencyMode="premultiply">
  ...
</AtlasConfig>
```

###### ElementConfig

Name                  | Description
----------------------|-------------------------
DefaultDpi            | The default dpi for atlases that don't specify their dpi.
Trim                  | enable/disable image trim. Trim removes transparent pixels around the sprite, but the image appears to have its original size when they are used.
TrimMargin            | The minimum number of transparent pixels to leave around a trimmed sprite (default: 1 as leaving one will create nicer aliasing for most use cases).
TransparencyThreshold | The first alpha value that is considered non transparent (default: 1).
ShapePadding          | The number of pixels of padding that should be added around images to prevent texture bleeding.
BorderPadding         | The number of pixels that should be kept free at the border of the texture.

```xml
<ElementConfig DefaultDpi="160" Extrude="1" Trim="true" TrimMargin="1" TransparencyThreshold="1" ShapePadding="2" BorderPadding="2"/>
```

###### LayoutConfig

Name                 | Description
---------------------|-------------------------
AllowRotation        | Set to false as its not fully supported at the moment. Allows the texture packer to rotate the elements.

```xml
<LayoutConfig AllowRotation="false"/>
```

###### TextureConfig

Name                 | Description
---------------------|-------------------------
MaxSize              | The maximum allowed texture size
PixelFormat          | **Not supported yet.** Defaults to *R8G8B8A8*
SizeRestrictions     | *any*=any size, *pow2*=power to texture, *pow2square*=square power 2.

```xml
<TextureConfig MaxSize="2048,2048" SizeRestriction="pow2"/>
```

##### AddBitmapFontConfig

Name                 | Description
---------------------|-------------------------
OutputFontFormat     | The default bitmap font output format (default: *nbf*)

```xml
<AddBitmapFontConfig OutputFontFormat="nbf"/>
```

#### LicenseConfig

Name                    | Description
------------------------|-------------------------
File                    | The name of the license file. If empty license file support are disabled.
Type                    | The type of the license file: *nxpJson, unknown*. Unknown is considered a binary blob without merge support.
AllowMasterFileCreation | Allows the texture packer to created a merged license master file for atlases.
RequiredForAllContent   | If true a license file must be present in all directories used to create a texture atlas.

```xml
<LicenseConfig File="License.json" Type="nxpJson" AllowMasterFileCreation="true" RequiredForAllContent="true"/>
```

## Commands

### AtlasFlavors

Verify that all atlases created under it contain the exact same names, elements and meta data. This is useful to ensure that a various resolutions of atlases contain the exact same things making it safer to assume that its safe to switch between resolutions.

Can contain the following child elements:

Child elements       | Description
---------------------|-------------------------
CreateAtlas          | (optional) Add a atlas flavor.

```xml
<AtlasFlavors Name="UIAtlas/UIAtlas">
  ...
</AtlasFlavors>
```

### CreateAtlas

Create a texture atlas

Name                 | Description
---------------------|-------------------------
Name                 | The unique name of the atlas and its a possible sub folder to output to. If the name ends with *number*_dpi it will be interpreted as setting the DefaultDpi for the atlas. For example Test_480dpi will set the DefaultDpi to 480. If the DefaultDpi attribute is specified as well and its a different value its considered a error.
DefaultDpi           | (optional) Override the default dpi.
OutputAtlasFormat    | (optional) Override the default atlas output format.
License              | (optional) Override all found licenses with the supplied one
SourcePath           | (optional) Set a path that will be added to all 'relative sub command paths' (defaults to empty)
TransparencyMode     | (optional) Override the default transparency mode. Can be: *normal, premultiply, premultiply-linear*.

```xml
<CreateAtlas Name="UIAtlas/UIAtlas_80dpi">
  ...
</CreateAtlas>
```

#### CreateAtlas sub commands

##### AtlasConfig override

A specific AtlasConfig can be added here which will override the one from the configuration file.
If its present there can only be one and it must be the first sub command!

##### AddFolder

Add all image files from given folder to the atlas.
There can be zero to N of these.

Name                 | Description
---------------------|-------------------------
Path                 | The relative folder path (will be combined with the atlas source path)
DefaultDpi           | (optional) Override the default dpi for all images in the folder

Can contain the following child elements:

Child elements       | Description
---------------------|-------------------------
ElementConfig        | (optional) Must be the first child. Overrides the default settings with the supplied ones (unset config elements will use the current default).
Folder               | (optional) Customize a sub folder added by AddFolder. (must come before any File tags)
File                 | (optional) Customize a entry added by AddFolder.

```xml
<AddFolder Path="UIAtlas/${Atlas.DefaultDpi}">
  ...
</AddFolder>
```

###### Folder

Customize all files under the given folder which was added by *AddFolder*

Name                 | Description
---------------------|-------------------------
Path                 | The relative name of the element to modify (must exist in the folder).

Can contain the following child elements:

Child elements       | Description
---------------------|-------------------------
ElementConfig        | (optional) Must be the first child. Overrides the default settings with the supplied ones (unset config elements will use the current default).

```xml
<Folder Path="Dummy">
  ...
</Folder>
```

###### File

Customize a file added by *AddFolder*

Name                 | Description
---------------------|-------------------------
Path                 | The relative name of the element to modify (must exist in the folder).

Can contain the following child elements:

Child elements       | Description
---------------------|-------------------------
ElementConfig        | (optional) Must be the first child. Overrides the default settings with the supplied ones (unset config elements will use the current default).
AddAnchor            | (optional) Add a number of anchor points as meta data.
AddNineSlice         | (optional) Add nine slice information.
AddComplexPatch      | (optional) Add complex patch information (experimental).

```xml
<File Path="Dummy.jpg">
  ...
</File>
```

##### AddBitmapFont

Add a bitmap font to the atlas.
There can be zero to N of these.

Name                 | Description
---------------------|-------------------------
OutputFontFormat     | (optional) Override the default font output format: *fbk, nbf, jsonBitmapFont, nbfC#*. Multiple formats can be specified here to produce multiple output file. Example: fbk,nbf.
Path                 | The relative font file path (will be combined with the atlas source path)
Type                 | The type of the font bitmap data. It can be: bitmap, sdf.
DefaultDpi           | (optional) Override the default dpi for all font characters.
LineSpacingPx        | (optional) Force the given line-spacing
MeasureChar          | (optional) Measure the given char, this is used in conjunction with 'MeasureHeightPx'
MeasureHeightPx      | (optional) The expected height of the 'MeasureChar' if 'MeasureChar' is set.

Can contain the following child elements:

Child elements       | Description
---------------------|-------------------------
ElementConfig        | (optional) Must be the first child. Overrides the default settings with the supplied ones (unset config elements will use the current default).

```xml
<AddBitmapFont Path="Font2/Font28/Font.fnt" BaseLinePx="20" LineSpacingPx="26" MeasureChar="B" MeasureHeightPx="14"/>
```

##### AddImage

Add a single image to the atlas.
There can be zero to N of these.

Name                 | Description
---------------------|-------------------------
Path                 | The relative image path (will be combined with the atlas source path). If the path ends with *number*_dpi it will be interpreted as setting the dpi for the image. For example Test_480dpi.png will set the dpi to 480. If the Dpi attribute is specified as well and its a different value its considered a error.
Dpi                  | (optional) Set the dpi for the added image
TargetPath           | (optional) Override the default scheme. **Not implemented.** The relative target path name (will be combined with the atlas source path)

Can contain the following child elements:

Child elements       | Description
---------------------|-------------------------
ElementConfig        | (optional) Must be the first child. Overrides the default settings with the supplied ones (unset config elements will use the current default).
AddAnchor            | (optional) Add a number of anchor points as meta data.
AddNineSlice         | (optional) Add nine slice information.
AddComplexPatch      | (optional) Add complex patch information (experimental).

```xml
<AddImage Path="Scale/RedBlue/RedBlueNineSlice_${Atlas.DefaultDpi}dpi.9.png"/>
<AddImage Path="Scale/RedBlue/RedBlueNineSliceTrimL4T4R4B4_${Atlas.DefaultDpi}dpi.9.png">
  <ElementConfig TrimMargin="0"/>
</AddImage>
```

###### AddAnchor

Add anchor information for the image, this is additional meta data that can be associated with a image.

Name                 | Description
---------------------|-------------------------
Position             | The position of the anchor in pixels. This can either be one point of a array of points.

```xml
<File Path="Control/White/CheckBox_BG.png">
  <AddAnchor Position="24,24"/>
</File>
<File Path="Control/White/RadioButton_BG.png">
  <AddAnchor Position="24,24"/>
</File>
<File Path="Control/White/Switch_BG.png">
  <AddAnchor Position="{16,24},{32,24}"/>
</File>
```

###### AddNineSlice

Customize a image entry by adding nine-slice information (not compatible with the ".9" filename tag)

Name                 | Description
---------------------|-------------------------
NineSlice            | The nine slice to apply. It must fit inside the image)
ContentMargin        | The nine slice content margin to apply It can be larger than the actual image as its only applied to the content put inside the nine-slice element.

```xml
<File Path="Background9R.png">
  <AddNineSlice NineSlice="32, 32, 32, 32" ContentMargin="25, 25, 25, 25"/>
</File>
```

###### AddComplexPatch

Customize a image entry by adding patch information (not compatible with the ".9" filename tag).
This is a experimental new sprite type.

Name                 | Description
---------------------|----------------------------------
SliceX               | Horizontal slice points.
SliceY               | Vertical slice points.
ContentSliceX        | Horizontal content slice points.
ContentSliceY        | Vertical content slice points.
Flags                | Patch flags: MirrorX (mirrors the x slices), MirrorY (mirrors the y slices).

Normal slice points describe how to slice the sprite and each slice point can be tagged with various flags.

Example:

```xml
<File Path="Control/Button/Rectangular/Contained_Background.png">
  <AddComplexPatch SliceX="0(t),10,17(s),44" SliceY="0(t),4,11(s),37,44(t),48" ContentSliceX="0,26(c),44" ContentSliceY="0,10(c),38,48" Flags="MirrorX"/>
</File>
<File Path="Control/Button/Rectangular/Outlined_Background.png">
  <AddComplexPatch SliceX="0(t),15(t|s),44" SliceY="0(t),9(s),24" ContentSliceX="0,26(c),44" ContentSliceY="0,10(c),24" Flags="MirrorX|MirrorY"/>
</File>
  ```

The slice point flags are:
Flag|Description
----|-----------
  t |slice is transparent
  s |slice can be scaled

The content slice point flags are:

Flag|Description
----|-----------
  c |content slice

## Examples

### Example config file

```xml
<?xml version="1.0" encoding="UTF-8"?>
<TexturePackerConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="1" DefaultCompany="NXP">
  <CreateAtlasConfig OutputAtlasFormat="bta3">
    <AtlasConfig TransparencyMode="premultiply">
      <ElementConfig DefaultDpi="160" Trim="true" TrimMargin="1" TransparencyTreshold="1" ShapePadding="2" BorderPadding="2"/>
      <LayoutConfig AllowRotation="false"/>
      <TextureConfig MaxSize="2048,2048" SizeRestriction="pow2"/>
    </AtlasConfig>
    <AddBitmapFontConfig OutputFontFormat="nbf"/>
  </CreateAtlasConfig>
  <LicenseConfig File="License.json" Type="nxpJson" AllowMasterFileCreation="true" RequiredForAllContent="true"/>
</TexturePackerConfig>
```

## Example command file

```xml
<?xml version="1.0" encoding="UTF-8"?>
<TexturePacker xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Version="1">
  <AtlasFlavor Name="UIAtlas/UIAtlas">
    <CreateAtlas Name="UIAtlas/UIAtlas_160dpi" OutputAtlasFormat="bta4">
      <AddFolder Path="UIAtlas/${Atlas.DefaultDpi}">
      <AddBitmapFont Path="Font/Font20/Font.fnt"/>
    </CreateAtlas>
    <CreateAtlas Name="UIAtlas/UIAtlas_320dpi" OutputAtlasFormat="bta4">
      <AddFolder Path="UIAtlas/${Atlas.DefaultDpi}"/>
      <AddBitmapFont Path="Font/Font40/Font.fnt"/>
    </CreateAtlas>
    <CreateAtlas Name="UIAtlas/UIAtlas_480dpi" OutputAtlasFormat="bta4">
      <AddFolder Path="UIAtlas/${Atlas.DefaultDpi}"/>
      <AddBitmapFont Path="Font/Font60/Font.fnt"/>
    </CreateAtlas>
    <CreateAtlas Name="UIAtlas/UIAtlas_640dpi" OutputAtlasFormat="bta4">
      <AddFolder Path="UIAtlas/${Atlas.DefaultDpi}"/>
      <AddBitmapFont Path="Font/Font80/Font.fnt"/>
    </CreateAtlas>
  </AtlasFlavor>
  <CreateAtlas Name="Font/SdfAtlas/FontAtlas" OutputAtlasFormat="bta4">
    <AddBitmapFont Path="Font/Sdf/Font.fnt" Type="sdf" OutputFontFormat="fbk,nbf"/>
  </CreateAtlas>
  <CreateAtlas Name="Font/NormalAtlas/FontAtlas" OutputAtlasFormat="bta4" TransparencyMode="normal">
    <AddBitmapFont Path="Font/Normal/Font.fnt" Type="bitmap" OutputFontFormat="fbk,nbf"/>
  </CreateAtlas>
  <CreateAtlas Name="Magic" OutputAtlasFormat="bta4">
    <AddFolder Path="Magic1337">
      <ElementConfig TrimMargin="10">
      <File Path="MagicButton.png">
        <ElementConfig TrimMargin="0">
        <AddNineSlice NineSlice="1, 2, 3, 4" ContentMargin="10, 20, 10, 20"/>
      </File>
      <File Path="MagicButton2.png">
        <AddNineSlice NineSlice="1, 2, 3, 4" ContentMargin="10, 20, 10, 20"/>
      </File>
    </AddFolder>
    <AddImage Path="FlyingFish.png">
      <ElementConfig TrimMargin="0">
      <AddNineSlice NineSlice="1, 2, 3, 4" ContentMargin="10, 20, 10, 20"/>
    </AddImage>
    <AddImage Path="FlyingFish2.png">
      <AddNineSlice NineSlice="1, 2, 3, 4" ContentMargin="10, 20, 10, 20"/>
    </AddImage>
  </CreateAtlas>
</TexturePacker>
```

This creates the following atlases (if run with the example config file as well):

* Atlas: UIAtlas/UIAtlas_160dpi
  * Texture atlas file: ```UIAtlas/UIAtlas_160dpi.png```
  * Binary texture atlas ```UIAtlas/UIAtlas_160dpi.bta```
  * Binary font info ```UIAtlas/UIAtlas_160dpi_Font.nbf```
  * Merged license file ```UIAtlas/License.json``` (this will be a merge of all licenses in the atlas)
* Atlas: UIAtlas/UIAtlas_320dpi
  * Texture atlas file: ```UIAtlas/UIAtlas_320dpi.png```
  * Binary texture atlas ```UIAtlas/UIAtlas_320dpi.bta```
  * Binary font info ```UIAtlas/UIAtlas_320dpi_Font.nbf```
  * Merged license file ```UIAtlas/License.json``` (this will be a merge of all licenses in the atlas)
* Atlas: UIAtlas/UIAtlas_480dpi
  * Texture atlas file: ```UIAtlas/UIAtlas_480dpi.png```
  * Binary texture atlas ```UIAtlas/UIAtlas_480dpi.bta```
  * Binary font info ```UIAtlas/UIAtlas_480dpi_Font.nbf```
  * Merged license file ```UIAtlas/License.json``` (this will be a merge of all licenses in the atlas)
* Atlas: UIAtlas/UIAtlas_640dpi
  * Texture atlas file: ```UIAtlas/UIAtlas_640dpi.png```
  * Binary texture atlas ```UIAtlas/UIAtlas_640dpi.bta```
  * Binary font info ```UIAtlas/UIAtlas_640dpi_Font.nbf```
  * Merged license file ```UIAtlas/License.json``` (this will be a merge of all licenses in the atlas)
* Atlas: *Font/NormalAtlas/FontAtlas*
  * Texture atlas file: ```Font/NormalAtlas/FontAtlas.png```
  * Binary texture atlas ```Font/NormalAtlas/FontAtlas.bta```
  * Binary font info ```Font/NormalAtlas/FontAtlas_Font.nbf```
  * Merged license file ```Font/NormalAtlas/License.json``` (this will be a merge of all licenses in the atlas)
* Atlas: *Font/SdfAtlas/FontAtlas*
  * Texture atlas file: ```Font/SdfAtlas/FontAtlas.png```
  * Binary texture atlas ```Font/SdfAtlas/FontAtlas.bta```
  * Binary font info ```Font/SdfAtlas/FontAtlas_Font.nbf```
  * Merged license file ```Font/SdfAtlas/License.json``` (this will be a merge of all licenses in the atlas)
* Atlas: *Magic*
  * Texture atlas file: ```Magic.png```
  * Binary texture atlas ```Magic.bta```
  * Merged license file ```License.json``` (this will be a merge of all licenses in the atlas)

It also validates that the atlases UIAtlas/UIAtlas_160dpi, UIAtlas/UIAtlas_320dpi, Atlas: UIAtlas/UIAtlas_480dpi and UIAtlas/UIAtlas_640dpi contains the same image resources (same image path name) and the same meta data for each entry.

## License

BSD-3 Clause "New" or "Revised" License
