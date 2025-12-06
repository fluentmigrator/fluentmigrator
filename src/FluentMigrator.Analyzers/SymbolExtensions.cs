#region License
// Copyright (c) 2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Linq;
using Microsoft.CodeAnalysis;

namespace FluentMigrator.Analyzers
{
    internal static class SymbolExtensions
    {
        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
        {
            if (targetType == null)
            {
                return false;
            }

            while (sourceType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
                {
                    return true;
                }

                if (exactMatch)
                {
                    return false;
                }

                if (targetType.TypeKind == TypeKind.Interface)
                {
                    return sourceType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetType));
                }

                sourceType = sourceType.BaseType;
            }

            return false;
        }
    }
}
