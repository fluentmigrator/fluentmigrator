#region License
// Copyright (c) 2026, Fluent Migrator Project
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

using System;

namespace FluentMigrator.Builder.SecurityLabel.Provider;

public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Generates a PostgreSQL <c>anon.lorem_ipsum</c> function call for TEXT and VARCHAR columns.
    /// </summary>
    /// <param name="paragraphs">
    /// The number of paragraphs to generate. If both <paramref name="words"/> and <paramref name="characters"/> are not specified,
    /// this value defaults to 5 paragraphs when <c>null</c> or 0 is provided.
    /// </param>
    /// <param name="words">
    /// The number of words to generate. If specified, <paramref name="paragraphs"/> and <paramref name="characters"/> must be omitted.
    /// </param>
    /// <param name="characters">
    /// The number of characters to generate, or an expression such as <c>anon.length(table.column)</c>. If specified,
    /// <paramref name="paragraphs"/> and <paramref name="words"/> must be omitted.
    /// </param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <example>
    /// <code>
    /// // Returns 5 paragraphs (default)
    /// builder.MaskedWithLoremIpsum();
    ///
    /// // Returns 2 paragraphs
    /// builder.MaskedWithLoremIpsum(paragraphs: 2);
    ///
    /// // Returns 4 paragraphs
    /// builder.MaskedWithLoremIpsum(paragraphs: 4);
    ///
    /// // Returns 20 words
    /// builder.MaskedWithLoremIpsum(words: 20);
    ///
    /// // Returns 7 characters
    /// builder.MaskedWithLoremIpsum(characters: "7");
    ///
    /// // Returns the same number of characters as the original string
    /// builder.MaskedWithLoremIpsum(characters: "anon.length(table.column)");
    /// </code>
    /// </example>
    public AnonSecurityLabelBuilder MaskedWithLoremIpsum(int? paragraphs = null, int? words = null, string characters = null)
    {
        // Ensure only one parameter is used and that the parameters are valid
        if ((paragraphs > 0 ? 1 : 0) + (words > 0 ? 1 : 0) + (!string.IsNullOrEmpty(characters) ? 1 : 0) > 1)
        {
            throw new ArgumentException("Only one of 'paragraphs', 'words', or 'characters' can be specified.");
        }

        if (words == null && string.IsNullOrEmpty(characters))
        {
            if (paragraphs is null or 0)
            {
                paragraphs = 5; // Default to 5 paragraphs if none specified
            }

            return MaskedWithFunction($"anon.lorem_ipsum( paragraphs := {paragraphs} )");
        }

        if (words > 0)
        {
            return MaskedWithFunction($"anon.lorem_ipsum( words := {words} )");
        }

        return MaskedWithFunction($"anon.lorem_ipsum( characters := {characters} )");
    }
}
