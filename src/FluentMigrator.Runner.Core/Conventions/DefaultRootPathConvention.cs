#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// The default implementation of a <see cref="IRootPathConvention"/>
    /// </summary>
    /// <remarks>
    /// It sets the working directory, which is either the
    /// path given in the constructor or - when the given path is
    /// null - the current directory.
    /// </remarks>
    public class DefaultRootPathConvention : IRootPathConvention
    {
        private readonly string _rootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRootPathConvention"/> class.
        /// </summary>
        /// <param name="rootPath">The root path for expressions requiring file system access.</param>
        /// <remarks>When <paramref name="rootPath"/> is null, then the current directory is
        /// returned</remarks>
        public DefaultRootPathConvention(string rootPath)
        {
            _rootPath = rootPath ?? Environment.CurrentDirectory;
        }

        /// <inheritdoc />
        public IFileSystemExpression Apply(IFileSystemExpression expression)
        {
            if (string.IsNullOrEmpty(expression.RootPath))
            {
                expression.RootPath = _rootPath;
            }

            return expression;
        }
    }
}
