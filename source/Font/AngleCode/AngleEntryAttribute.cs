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

//----------------------------------------------------------------------------------------------------------------------------------------------------

namespace FslGraphics.Font.AngleCode
{
  struct AngleEntryAttribute : IEquatable<AngleEntryAttribute>
  {
    public string Name;
    public string Value;

    public AngleEntryAttribute(string name, string value)
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsValid
    {
      get
      {
        return Name != null && Value != null;
      }
    }

    public static bool operator ==(AngleEntryAttribute lhs, AngleEntryAttribute rhs)
    {
      return (lhs.Name == rhs.Name && lhs.Value == rhs.Value);
    }


    public static bool operator !=(AngleEntryAttribute lhs, AngleEntryAttribute rhs)
    {
      return !(lhs == rhs);
    }


    public override bool Equals(object obj)
    {
      return !(obj is AngleEntryAttribute) ? false : (this == (AngleEntryAttribute)obj);
    }


    public override int GetHashCode()
    {
      return (Name != null ? Name.GetHashCode(StringComparison.Ordinal) : 0) ^ (Value != null ? Value.GetHashCode(StringComparison.Ordinal) : 0);
    }


    public bool Equals(AngleEntryAttribute other)
    {
      return this == other;
    }

    public override string ToString()
    {
      return $"Name: {Name} Value: {Value}";
    }
  }

}

//****************************************************************************************************************************************************
