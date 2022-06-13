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
using System.IO;

namespace TexturePacker.Commands
{
  public class PathResolver
  {
    private static readonly Logger g_logger = LogManager.GetCurrentClassLogger();
    private static readonly IFormatProvider g_invariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    private Dictionary<string, string> m_variables;
    private Dictionary<string, string> m_envVariables;

    public PathResolver()
    {
      m_variables = new Dictionary<string, string>();
      m_envVariables = new Dictionary<string, string>();
    }

    public PathResolver(PathResolver resolver)
    {
      if (resolver == null)
        throw new ArgumentNullException(nameof(resolver));

      m_variables = new Dictionary<string, string>(resolver.m_variables);
      m_envVariables = new Dictionary<string, string>(resolver.m_envVariables);
    }

    public void AddVariable(string name, string value)
    {
      g_logger.Trace("AddVariable '{0}'='{1}'", name, value);
      if (name == null)
        throw new ArgumentNullException(nameof(name));
      if (value == null)
        throw new ArgumentNullException(nameof(value));
      if (!NameUtil.IsValidVariableName(name))
        throw new ArgumentException("Invalid variable name", nameof(name));

      if (m_variables.ContainsKey(name))
        throw new ArgumentException($"variable '{name}' already added");

      m_variables[name] = value;
    }

    public void AddComplexVariable(string name, string value)
    {
      g_logger.Trace("AddComplexVariable '{0}'='{1}'", name, value);
      if (name == null)
        throw new ArgumentNullException(nameof(name));
      if (value == null)
        throw new ArgumentNullException(nameof(value));
      if (!NameUtil.IsValidComplexVariableName(name))
        throw new ArgumentException("Invalid variable name", nameof(name));

      if (m_variables.ContainsKey(name))
        throw new ArgumentException($"variable '{name}' already added");

      m_variables[name] = value;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
    public void RemoveComplexVariable(string name)
    {
      g_logger.Trace("RemoveComplexVariable '{0}'}'", name);
      m_variables.Remove(name);
    }


    public ResolvedPath ResolvePath(string sourcePath)
    {
      string resolvedPath = DoResolvePathFullPath(sourcePath);
      return new ResolvedPath(resolvedPath, sourcePath, sourcePath, sourcePath);
    }

    public ResolvedPath Combine(ResolvedPath sourcePath, string path)
    {
      if (sourcePath == null)
        throw new ArgumentNullException(nameof(sourcePath));
      if (path == null)
        throw new ArgumentNullException(nameof(path));

      //var absPath = Path.Combine(sourcePath.AbsolutePath, path);
      //var srcPath = Path.Combine(sourcePath.SourcePath, path);
      //var resolvedSourcePath = DoResolvePathFullPath(srcPath);
      //return new ResolvedPath(resolvedSourcePath, absPath, sourcePath);

      var originalSourcePath = IOUtil.Combine(sourcePath.UnresolvedSourcePath, path);
      var unresolvedAbsPath = IOUtil.Combine(sourcePath.AbsolutePath, path);
      var resolvedAbsPath = DoResolvePathFullPath(unresolvedAbsPath);

      // OLD: public ResolvedPath(string absolutePath, string relativeResolvedSourcePath, string unresolvedSourcePath, ResolvedPath parentPath = null)
      //return new ResolvedPath(resolvedAbsPath, DoResolvePath(path), originalSourcePath, sourcePath);
      return new ResolvedPath(resolvedAbsPath, originalSourcePath, sourcePath);
    }



    private static ReadOnlySpan<char> GetVariableName(ReadOnlySpan<char> src, char endIndicator)
    {
      int index = src.IndexOf(endIndicator);
      if (index < 0)
        throw new Exception("invalid variable");

      var variableView = src.Slice(0, index);
      if (!NameUtil.IsValidComplexVariableName(variableView))
        throw new Exception($"invalid variable name {variableView.ToString()}");

      return variableView;
    }


    private string DoResolvePath(string sourcePath)
    {
      string resolvedPath = string.Empty;
      var scanSpan = sourcePath.AsSpan();
      while (scanSpan.Length > 0)
      {
        int foundIndex = scanSpan.IndexOf("$");
        if (foundIndex >= 0)
        {
          if (foundIndex > 0)
          {
            resolvedPath += scanSpan.Slice(0, foundIndex).ToString();
          }
          scanSpan = scanSpan.Slice(foundIndex + 1);
          if (scanSpan.StartsWith("{"))
          {
            scanSpan = scanSpan.Slice(1);
            var variableName = GetVariableName(scanSpan, '}').ToString();
            if (!m_variables.TryGetValue(variableName, out string variableValue))
              throw new Exception($"Unknown variable {variableName}");

            scanSpan = scanSpan.Slice(variableName.Length + 1);
            resolvedPath += variableValue;
          }
          else if (scanSpan.StartsWith("("))
          {
            scanSpan = scanSpan.Slice(1);
            var variableName = GetVariableName(scanSpan, ')').ToString();
            if (!m_envVariables.TryGetValue(variableName, out string variableValue))
              throw new Exception($"Unknown variable {variableName}");

            scanSpan = scanSpan.Slice(variableName.Length + 1);
            resolvedPath += variableValue.ToString(g_invariantCulture);
          }
          else
          {
            resolvedPath += "$";
          }
        }
        else
        {
          resolvedPath += scanSpan.ToString();
          scanSpan = scanSpan.Slice(scanSpan.Length);
        }
      }
      return resolvedPath;
    }


    private string DoResolvePathFullPath(string sourcePath)
    {
      return IOUtil.NormalizePath(System.IO.Path.GetFullPath(DoResolvePath(sourcePath)));
    }
  }
}
