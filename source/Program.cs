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

using FslGraphics.Font.BF;
using MB.Base.MathEx.Pixel;
using MB.RectangleBinPack.TexturePack;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TexturePacker.Commands;
using TexturePacker.Config;
using TexturePacker.Input;

namespace TexturePacker
{
  static class Program
  {
    public const int VersionMajor = 1;
    public const int VersionMinor = 2;
    public const int VersionPatch = 2;

    private const int PROGRAM_RESULT_SUCCESS = 0;
    private const int PROGRAM_RESULT_ERROR = 1;

    private const string ConfigFilename = "TexturePackerConfig.xml";
    private const string DefaultFilename = "TexturePacker.xml";


    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();
    //private static readonly IFormatProvider g_invariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    private struct VariableKeyValue
    {
      public string Key;
      public string Value;

      public VariableKeyValue(string key, string value)
      {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));

        if (!NameUtil.IsValidVariableName(key))
          throw new Exception($"Invalid key name: '{key}'");
      }
    }

    static string SanitizeCommandFilename(string filename)
    {
      if (filename.StartsWith('/'))
        throw new Exception($"filename '{filename}' can not start with '/'");
      if (filename.StartsWith('\\'))
        throw new Exception($"filename '{filename}' can not start with '\\'");
      if (filename.EndsWith('/'))
        throw new Exception($"filename '{filename}' can not end with '/'");
      if (filename.EndsWith('\\'))
        throw new Exception($"filename '{filename}' can not end with '\\'");
      return IOUtil.NormalizePath(Path.GetFullPath(filename));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    static int Process(string[] args, bool allowThreads)
    {
      string currentPath = IOUtil.NormalizePath(Directory.GetCurrentDirectory());
      g_logger.Trace("CWD: '{}'", currentPath);
      string outputPath = currentPath;

      g_logger.Trace("Parsing command line arguments");
      var parsedArgs = ProcessArguments(args);

      if (parsedArgs.Output != null)
      {
        if (!Directory.Exists(parsedArgs.Output))
          throw new Exception($"The supplied output path of '{parsedArgs.Output}' is not a directory");
        outputPath = parsedArgs.Output;
        g_logger.Trace("OutputPath forced to '{}'", outputPath);
      }

      if (parsedArgs.PositionalArgs.Count > 1)
        return parsedArgs.HelpPrinted ? PROGRAM_RESULT_SUCCESS : PROGRAM_RESULT_ERROR;

      string userSuppliedFilename = parsedArgs.PositionalArgs.Count == 1 ? SanitizeCommandFilename(parsedArgs.PositionalArgs[0]) : string.Empty;


      // Add basic support for recursive processing
      var unresolvedCommandGroups = new List<CommandGroup>();
      if (!parsedArgs.Recursive)
      {
        unresolvedCommandGroups.Add(CreateCommandGroup(userSuppliedFilename, currentPath, parsedArgs.VariableKeyValue, outputPath));
      }
      else
      {
        if (userSuppliedFilename.Length != 0)
          throw new NotSupportedException("A commandFilename can not be specified together with the '-r' recursive processing argument");
        if (parsedArgs.Output != null)
          throw new NotSupportedException("'--output' incompatible with '-r'");

        RecursiveAddCommandGroups(unresolvedCommandGroups, currentPath, parsedArgs.VariableKeyValue);
      }

      // Process all command groups
      var timer = Stopwatch.StartNew();
      foreach (var unresolvedCommandGroup in unresolvedCommandGroups)
      {
        TexturePack.Process(unresolvedCommandGroup, allowThreads, parsedArgs.DisableLicenseFiles, parsedArgs.OverwritePolicy);
      }
      Console.WriteLine($"Processing time: {timer.Elapsed}");
      return PROGRAM_RESULT_SUCCESS;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static void RecursiveAddCommandGroups(List<CommandGroup> unresolvedCommandGroups, string currentPath, List<VariableKeyValue> variableKeyValue)
    {
      g_logger.Trace("Scanning '{0}' for command files to process", currentPath);
      if (!Directory.Exists(currentPath))
        throw new Exception($"Directory '{currentPath}' is not a valid directory");

      // Scan all sub directories
      var foundDirectories = Directory.GetDirectories(currentPath, "*.*", SearchOption.AllDirectories);


      { // Scan the current path
        var commandGroup = TryCreateCommandGroup("", currentPath, variableKeyValue, currentPath, out string rFinalFilename, false);
        if (commandGroup != null)
          unresolvedCommandGroups.Add(commandGroup);
      }

      foreach (var unnormalizedDirectory in foundDirectories)
      {
        string directory = IOUtil.NormalizePath(unnormalizedDirectory);
        g_logger.Trace("Scanning '{0}' for command file", directory);
        var commandGroup = TryCreateCommandGroup("", directory, variableKeyValue, directory, out string rFinalFilename, false);
        if (commandGroup != null)
        {
          g_logger.Trace("- Found command file at {0}", directory);
          unresolvedCommandGroups.Add(commandGroup);
        }
      }

      g_logger.Trace("Found {0} command files", unresolvedCommandGroups.Count);
    }

    private static CommandGroup CreateCommandGroup(string userSuppliedFilename, string currentPath, List<VariableKeyValue> variableKeyValue, string outputPath)
    {
      var commandGroup = TryCreateCommandGroup(userSuppliedFilename, currentPath, variableKeyValue, outputPath, out string finalFileName);
      if (commandGroup != null)
        return commandGroup;
      throw new Exception($"Command file not found {finalFileName}");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static CommandGroup TryCreateCommandGroup(string userSuppliedFilename, string currentPath, List<VariableKeyValue> variableKeyValue,
                                                      string outputPath, out string rFinalFilename, bool traceNoUserFilename = true)
    {
      // Determine command path
      string commandFilePath = userSuppliedFilename.Length > 0 ? IOUtil.GetDirectoryName(userSuppliedFilename) : currentPath;
      var texturePackerConfig = FindAndApplyConfigFiles(commandFilePath, GetDefaultConfig());

      if (userSuppliedFilename.Length == 0)
      {
        if (traceNoUserFilename)
        {
          g_logger.Trace("No commandFilename specified, using default which is configured to '{0}'", texturePackerConfig.DefaultFilename);
        }
        userSuppliedFilename = IOUtil.Combine(commandFilePath, texturePackerConfig.DefaultFilename);
      }
      rFinalFilename = userSuppliedFilename;

      if (!File.Exists(userSuppliedFilename))
        return null;

      g_logger.Trace("Reading command file '{0}'", userSuppliedFilename);
      string strXmlCommandFile = File.ReadAllText(userSuppliedFilename);

      var pathResolver = new PathResolver();
      {
        g_logger.Trace("Preparing PathResolver");
        foreach (var entry in variableKeyValue)
        {
          pathResolver.AddVariable(entry.Key, entry.Value);
        }
      }

      string atlasFileSourceDirectoryPath = IOUtil.NormalizePath(Path.GetDirectoryName(userSuppliedFilename));
      return CommandFileDecoder.Decode(strXmlCommandFile, texturePackerConfig, atlasFileSourceDirectoryPath, pathResolver, outputPath);
    }


    /// <summary>
    /// Find the verbosity level in a really dumb way
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static UInt32 FindVerbosityLevel(string[] args)
    {
      if (args.Contains("-vvvvv"))
        return 5;
      if (args.Contains("-vvvv"))
        return 4;
      if (args.Contains("-vvv"))
        return 3;
      if (args.Contains("-vv"))
        return 2;
      if (args.Contains("-v"))
        return 1;
      return 0;
    }

    static LogLevel SelectLogLevel(UInt32 verbosityLevel)
    {
      switch (verbosityLevel)
      {
        case 0:
          return LogLevel.Off;
        case 1:
          return LogLevel.Warn;
        case 2:
          return LogLevel.Debug;
        default:
          return LogLevel.Trace;
      }
    }


    static int Main(string[] args)
    {
      bool debug = args.Contains("--debug");
      UInt32 verbosityLevel = FindVerbosityLevel(args);
      if (verbosityLevel > 0u)
      {
        var logLevel = SelectLogLevel(verbosityLevel);
        var target = new ConsoleTarget();
        //target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message} (TID:${threadid})";
        target.Layout = "${logger} ${message} (TID:${threadid})";
        NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, logLevel);
      }

      bool allowThreads = !args.Contains("--noThreads");
      if (args.Contains("--dev"))
      {
        // allows the debugger to catch the exception at the source
        return Process(args, allowThreads);
      }

      try
      {
        return Process(args, allowThreads);
      }
      catch (System.Exception ex)
      {
        if (ex is AggregateException)
        {
          var aggregateException = (AggregateException)ex;
          foreach (var innerEx in aggregateException.InnerExceptions)
          {
            g_logger.Error(innerEx, "ERROR: {0}", innerEx);
            System.Console.WriteLine("ERROR: {0}", innerEx.Message);
          }
        }
        else
        {
          g_logger.Error(ex, "ERROR: {0}", ex);
          System.Console.WriteLine("ERROR: {0}", ex.Message);
        }
        if (!debug)
        {
          return PROGRAM_RESULT_ERROR;
        }
        else
        {
          throw;
        }
      }
    }

    readonly struct ParsedArgRecord
    {
      public readonly bool Recursive;
      public readonly List<string> PositionalArgs;
      public readonly List<VariableKeyValue> VariableKeyValue;
      public readonly bool HelpPrinted;
      public readonly bool DisableLicenseFiles;
      public readonly IOUtil.OverWritePolicy OverwritePolicy;
      public readonly string Output;

      public ParsedArgRecord(bool recursive, List<string> positionalArgs, List<VariableKeyValue> variableKeyValue, bool helpPrinted, bool disableLicenseFiles,
                             IOUtil.OverWritePolicy overwritePolicy, string output)
      {
        Recursive = recursive;
        PositionalArgs = positionalArgs ?? throw new ArgumentNullException(nameof(positionalArgs));
        VariableKeyValue = variableKeyValue ?? throw new ArgumentNullException(nameof(variableKeyValue));
        HelpPrinted = helpPrinted;
        DisableLicenseFiles = disableLicenseFiles;
        OverwritePolicy = overwritePolicy;
        Output = output;
      }
    }


    private static ParsedArgRecord ProcessArguments(string[] args)
    {
      bool printHelp = false;
      bool recursive = false;
      bool disableLicenseFiles = false;
      var positionalArgs = new List<string>();
      var overwritePolicy = IOUtil.OverWritePolicy.NotAllowed;
      var variableKeyValue = new List<VariableKeyValue>();
      UInt32 verbosity = 0;
      string output = null;
      { // Process arguments
        for (int i = 0; i < args.Length; ++i)
        {
          var strArg = args[i];
          if (strArg.StartsWith("--", StringComparison.Ordinal))
          {
            var strSwicth = strArg.Substring(2);
            switch (strSwicth)
            {
              case "dev":
              case "debug":
              case "noThreads":
                // This was handled earlier
                break;
              case "disableLicenseFiles":
                disableLicenseFiles = true;
                break;
              case "overwrite":
                overwritePolicy = IOUtil.OverWritePolicy.Allowed;
                g_logger.Trace("Allowing overwrite");
                break;
              case "variables":
                if ((i + 1) >= args.Length)
                  throw new ArgumentException("--variables requires a parameter");
                ParseVariables(variableKeyValue, args[i + 1]);
                ++i;
                break;
              case "output":
                if ((i + 1) >= args.Length)
                  throw new ArgumentException("--output requires a parameter");
                output = Path.GetFullPath(args[i + 1]);
                ++i;
                break;
              default:
                throw new NotSupportedException($"Unsupported switch {strArg}");
            }
          }
          else if (strArg.StartsWith("-", StringComparison.Ordinal))
          {
            var strSwicth = strArg.Substring(1);
            switch (strSwicth)
            {
              case "h":
                printHelp = true;
                break;
              case "r":
                recursive = true;
                break;
              case "v":
                ++verbosity;
                break;
              case "vv":
                verbosity += 2;
                break;
              case "vvv":
                verbosity += 3;
                break;
              case "vvvv":
                verbosity += 4;
                break;
              case "vvvvv":
                verbosity += 5;
                break;
              default:
                throw new NotSupportedException($"Unsupported switch {strArg}");
            }
          }
          else
          {
            positionalArgs.Add(strArg);
          }
        }
      }

      if (printHelp)
      {
        PrintHelp();
      }
      return new ParsedArgRecord(recursive, positionalArgs, variableKeyValue, printHelp, disableLicenseFiles, overwritePolicy, output);
    }

    private static void ParseVariables(List<VariableKeyValue> variableKeyValue, string str)
    {
      var variablesPairs = str.Split(';');
      foreach (var strVariablePair in variablesPairs)
      {
        var pair = strVariablePair.Split('=');
        if (pair.Length != 2)
          throw new Exception($"{strVariablePair} is not a pair of the type key=value");

        if (!NameUtil.IsValidVariableName(pair[0]))
          throw new Exception("'${pair[0]}' is not a valid variable name");

        variableKeyValue.Add(new VariableKeyValue(pair[0], pair[1]));
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Simple tool")]
    private static void PrintHelp()
    {
      Console.WriteLine($"TexturePacker V{VersionMajor}.{VersionMinor}.{VersionPatch}");
      Console.WriteLine("Usage:   TexturePacker [<commandFilename>]");
      Console.WriteLine("Commands: ");
      Console.WriteLine(" -v                             : Enable verbose logging (up to -vvvvv)");
      Console.WriteLine(" -r                             : Process all current directory and its subdirectories recursively. Incompatible with specifying a commandFileName.");
      Console.WriteLine(" --debug                        : Enable debugging");
      Console.WriteLine(" --dev                          : Disable main exception catcher");
      Console.WriteLine(" --disableLicenseFiles          : Disable license files");
      Console.WriteLine(" --noThreads                    : Disable multi threading");
      Console.WriteLine(" --output <path>                : Set the output path");
      Console.WriteLine(" --overwrite                    : Allow overwrite");
      Console.WriteLine(" --variables variableList       : Add a list of key-value pairs: key0=value0;key1=value1");
    }


    private static string GetRootConfigFilename()
    {
      string prefix = "file:///";
      var appRootDir = System.Reflection.Assembly.GetExecutingAssembly().Location;
      if (appRootDir.StartsWith(prefix, StringComparison.Ordinal))
        appRootDir = appRootDir.Substring(prefix.Length);
      var appDirectory = Path.GetDirectoryName(appRootDir);
      //var appFilenameWithoutExt = Path.GetFileNameWithoutExtension(appRootDir);
      //return Path.Combine(appDirectory, $"{appFilenameWithoutExt}.xml");
      return Path.Combine(appDirectory, ConfigFilename);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static TexturePackerConfig ApplyConfigFile(string currentPath, in TexturePackerConfig defaultValues)
    {
      var strXmlConfigFile = IOUtil.TryReadAllText(currentPath);
      if (strXmlConfigFile == null)
      {
        g_logger.Trace("Config file not found at '{0}', no changes", currentPath);
        return defaultValues;
      }
      return ConfigFileDecoder.Decode(strXmlConfigFile, defaultValues);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static TexturePackerConfig FindAndApplyConfigFiles(string currentPath, in TexturePackerConfig defaultValues)
    {
      g_logger.Trace("Building default configuration");

      var finalConfig = defaultValues;
      var configFile = GetRootConfigFilename();
      if (File.Exists(configFile))
      {
        g_logger.Trace("Trying to apply 'exe' config file '{0}'", configFile);
        finalConfig = ApplyConfigFile(configFile, finalConfig);
      }
      else
      {
        g_logger.Trace("No 'exe' config file found at '{0}'", configFile);
      }

      // closest parent
      configFile = TryLocateClosestConfigFile(currentPath);
      if (configFile != null)
      {
        g_logger.Trace("Trying to apply closest parent config file '{0}'", configFile);
        finalConfig = ApplyConfigFile(configFile, finalConfig);
      }
      else
      {
        g_logger.Trace("No closest parent config file found.");
      }
      g_logger.Trace("Default configuration ready");
      return finalConfig;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    private static string TryLocateClosestConfigFile(string currentPath)
    {
      do
      {
        string configFilename = IOUtil.Combine(currentPath, ConfigFilename);
        g_logger.Trace("Trying to locate config file at '{0}'", configFilename);
        if (File.Exists(configFilename))
          return configFilename;

        currentPath = Path.GetDirectoryName(currentPath);
      } while (currentPath != null && currentPath.Length > 0);
      return null;
    }

    private static AtlasConfig GetDefaultAtlasConfig()
    {
      var transparencyMode = TransparencyMode.Normal;
      var textureConfig = new AtlasTextureConfig(new PxSize2D(2048, 2048), TextureSizeRestriction.Pow2);
      var layoutConfig = new AtlasLayoutConfig(false);
      var elementConfig = new AtlasElementConfig(160, 1, true, 1, 1, 2, 2);
      return new AtlasConfig(transparencyMode, textureConfig, layoutConfig, elementConfig);
    }

    private static AddBitmapFontConfig GetDefaultAddBitmapFontConfig()
    {
      return new AddBitmapFontConfig(BitmapFontType.Bitmap, OutputFontFormat.NBF);
    }

    private static TexturePackerConfig GetDefaultConfig()
    {
      var outputFormat = OutputAtlasFormat.BTA4;
      var atlasConfig = GetDefaultAtlasConfig();
      var addBitmapFontConfig = GetDefaultAddBitmapFontConfig();
      var createAtlasConfig = new CreateAtlasConfig(outputFormat, atlasConfig, addBitmapFontConfig);
      var licenseConfig = new LicenseConfig();
      return new TexturePackerConfig(null, null, DefaultFilename, createAtlasConfig, licenseConfig);
    }

  }
}
