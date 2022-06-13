# Build setup for Ubuntu 18.04

## Prerequisites

* Ubuntu 18.04
  * Goto the [official .net core download site](https://dotnet.microsoft.com/download#linuxubuntu)
  * Download and install the .NET core SDK (be sure to select the one for 18.04)

## Build

```bash
dotnet build -c release
```

## Standalone build for Linux

Run the supplied ```BuildStandAloneLinux.sh``` file.

## Standalone build for Win64

Run the supplied ```BuildStandAloneWin64.sh``` file.
