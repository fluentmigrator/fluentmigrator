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

using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators
{
    public static class GeneratorExtensions
    {
        /// <summary>
        /// A helper method to get the <see cref="IQuoter"/> for the given migration generator since the
        /// <see cref="IMigrationGenerator"/> interface does not expose the quoter directly.
        /// </summary>
        /// <remarks>
        /// This method relies on the <see cref="IMigrationGenerator"/> being a <see cref="GeneratorBase"/>. If this is
        /// not the case, there is no way to get the <see cref="IQuoter"/> so this method will return
        /// <see langword="null"/>.
        /// </remarks>
        public static IQuoter GetQuoter(this IMigrationGenerator generator)
        {
            // Safe cast to GenericGenerator since IMigrationGenerator does not expose the quoter
            return (generator as GeneratorBase)?.Quoter;
        }
    }
}
