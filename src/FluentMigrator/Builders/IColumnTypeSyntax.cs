#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
    public interface IColumnTypeSyntax<TNext> : IFluentSyntax
        where TNext : IFluentSyntax
    {
        TNext AsAnsiString();
        TNext AsAnsiString(string collationName);
        TNext AsAnsiString(int size);
        TNext AsAnsiString(int size, string collationName);
        TNext AsBinary();
        TNext AsBinary(int size);
        TNext AsBoolean();
        TNext AsByte();
        TNext AsCurrency();
        TNext AsDate();
        TNext AsDateTime();
        TNext AsDateTimeOffset();
        TNext AsDecimal();
        TNext AsDecimal(int size, int precision);
        TNext AsDouble();
        TNext AsGuid();
        TNext AsFixedLengthString(int size);
        TNext AsFixedLengthString(int size, string collationName);
        TNext AsFixedLengthAnsiString(int size);
        TNext AsFixedLengthAnsiString(int size, string collationName);
        TNext AsFloat();
        TNext AsInt16();
        TNext AsInt32();
        TNext AsInt64();
        TNext AsString();
        TNext AsString(string collationName);
        TNext AsString(int size);
        TNext AsString(int size, string collationName);
        TNext AsTime();
        TNext AsXml();
        TNext AsXml(int size);
        TNext AsCustom(string customType);
    }
}