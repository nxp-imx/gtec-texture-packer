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

namespace TexturePacker
{
  static class NameUtil
  {
    public static string ValidatePathName(string path)
    {
      if (path == null)
        throw new ArgumentNullException(nameof(path));
      if (path.Contains("..", StringComparison.Ordinal))
        throw new NotSupportedException($"path '{path}' is not allowed to contain '..'");
      if (path.Contains(':', StringComparison.Ordinal))
        throw new NotSupportedException($"path '{path}' is not allowed to contain ':'");
      if (path.Contains('\\', StringComparison.Ordinal))
        throw new NotSupportedException($"path '{path}' is not allowed to contain '\\'");
      if (path.StartsWith("/", StringComparison.Ordinal))
        throw new NotSupportedException($"path '{path}' is not allowed to start with '/'");
      if (path.Length < 1)
        throw new NotSupportedException($"path can not be empty");
      string validChars = "_.$(){}/-";
      foreach (var ch in path)
      {
        if (!((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || validChars.Contains(ch, StringComparison.Ordinal)))
        {
          throw new NotSupportedException($"Path '{path}' is not allowed to contain '{ch}'");
        }
      }
      return path;
    }

    public static bool IsValidNameFirstCharacter(char ch)
    {
      return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
    }

    public static bool IsValidNameCharacter(char ch)
    {
      return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_';
    }

    public static bool IsValidVariableName(string name)
    {
      return IsValidVariableName(name.AsSpan());
    }


    public static void ValidateFilename(string filename)
    {
      if (filename == null)
        throw new ArgumentNullException(nameof(filename));
      if (filename.Contains("..", StringComparison.Ordinal))
        throw new NotSupportedException($"filename '{filename}' is not allowed to contain '..'");
      if (filename.Contains('$', StringComparison.Ordinal))
        throw new NotSupportedException($"filename '{filename}' is not allowed to contain '$'");
      if (filename.Contains(':', StringComparison.Ordinal))
        throw new NotSupportedException($"filename '{filename}' is not allowed to contain ':'");
      if (filename.Contains('/', StringComparison.Ordinal))
        throw new NotSupportedException($"filename '{filename}' is not allowed to contain '/'");
      if (filename.Contains('\\', StringComparison.Ordinal))
        throw new NotSupportedException($"filename '{filename}' is not allowed to contain '\\'");
      if (filename.Length < 1)
        throw new NotSupportedException($"filename can not be empty");
      foreach (var ch in filename)
      {
        if (!((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '.'))
        {
          throw new NotSupportedException($"filename '{filename}' is not allowed to contain '{ch}'");
        }
      }
    }

    public static bool IsValidComplexVariableName(string name)
    {
      return IsValidComplexVariableName(name.AsSpan());
    }

    public static bool IsValidVariableName(ReadOnlySpan<char> name)
    {
      if (name.Length < 1 || !IsValidNameFirstCharacter(name[0]))
        return false;

      for (int i = 1; i < name.Length; ++i)
      {
        if (!IsValidNameCharacter(name[i]))
          return false;
      }
      return true;
    }


    public static bool IsValidComplexVariableCharacter(char ch)
    {
      return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '.';
    }

    public static bool IsValidComplexVariableName(ReadOnlySpan<char> name)
    {
      if (name.Length < 1 || !IsValidNameFirstCharacter(name[0]))
        return false;

      int length = name.Length - 1;
      for (int i = 1; i < length; ++i)
      {
        if (!IsValidComplexVariableCharacter(name[i]))
          return false;
      }
      return (length >= 2 ? IsValidNameCharacter(name[name.Length - 1]) : true);
    }

    public static bool IsValidPathFirstCharacter(char ch)
    {
      return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '-' || ch == '_';
    }

    public static bool IsValidPathCharacter(char ch)
    {
      return IsValidPathFirstCharacter(ch) || ch == '/';
    }

    public static bool IsValidPathName(ReadOnlySpan<char> name)
    {
      if (name.Length < 1 || !IsValidPathFirstCharacter(name[0]))
        return false;

      for (int i = 1; i < name.Length; ++i)
      {
        if (!IsValidPathCharacter(name[i]))
          return false;
      }
      return true;
    }

  }
}
