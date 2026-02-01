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
    /// Lorem ipsum generator for TEXT and VARCHAR columns :
    /// anon.lorem_ipsum() returns 5 paragraphs
    /// anon.lorem_ipsum(2) returns 2 paragraphs
    /// anon.lorem_ipsum( paragraphs := 4 ) returns 4 paragraphs
    /// anon.lorem_ipsum( words := 20 ) returns 20 words
    /// anon.lorem_ipsum( characters := 7 ) returns 7 characters
    /// anon.lorem_ipsum( characters := anon.length(table.column) ) returns the same amount of characters as the original string
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
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
