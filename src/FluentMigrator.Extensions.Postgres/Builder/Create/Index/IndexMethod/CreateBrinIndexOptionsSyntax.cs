#region License
// Copyright (c) 2021, FluentMigrator Project
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

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    public class CreateBrinIndexOptionsSyntax : AbstractCreateIndexMethodOptionsSyntax, ICreateBrinIndexOptionsSyntax
    {
        /// <inheritdoc />
        public CreateBrinIndexOptionsSyntax([NotNull] ICreateIndexOptionsSyntax createIndexOptionsSyntax)
            : base(createIndexOptionsSyntax)
        {
        }


        public const string IndexPagesPerRange = "PostgresBrinPagesPerRange";

        /// <inheritdoc />
        public new ICreateBrinIndexOptionsSyntax Fillfactor(int fillfactor)
        {
            base.Fillfactor(fillfactor);
            return this;
        }

        /// <inheritdoc />
        public ICreateBrinIndexOptionsSyntax PagesPerRange(int range)
        {
            var additionalFeatures = CreateIndexOptionsSyntax as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexPagesPerRange, range);
            return this;
        }

        /// <inheritdoc />
        public ICreateBrinIndexOptionsSyntax Autosummarize()
        {
            return Autosummarize(true);
        }

        /// <inheritdoc />
        public ICreateBrinIndexOptionsSyntax DisableAutosummarize()
        {
            return Autosummarize(false);
        }

        public const string IndexAutosummarize = "PostgresBrinautosummarize";

        /// <inheritdoc />
        public ICreateBrinIndexOptionsSyntax Autosummarize(bool autosummarize)
        {
            var additionalFeatures = CreateIndexOptionsSyntax as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexAutosummarize, autosummarize);
            return this;
        }
    }
}
