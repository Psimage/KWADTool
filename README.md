[![Build Status](https://travis-ci.org/Psimage/KWADTool.svg)](https://travis-ci.org/Psimage/KWADTool)

# KWADTool
A tool to work with Invisible Inc. KWAD files

## Requirements
.NET Framework 4.5

## Usage
```
KWADTool -i <kwadFile> [-e (textures|blobs|all)] [-o <outputDir>]

  -e <type>, --extract=<type>    (Default: All) Extract resources. Valid types
                                 are Textures|Blobs|All (case insensitive).

  -i <file>, --input=<file>      Required. Input KWAD file.

  -o <dir>, --output=<dir>     (Default: <kwadFileName>.d) Output directory.

  --help                         Display this help screen.
```
### Example usage:
```
KWADTool -i anims.kwad
KWADTool -e textures -i anims.kwad
KWADTool -i anims.kwad -o outputDir -e blobs
```