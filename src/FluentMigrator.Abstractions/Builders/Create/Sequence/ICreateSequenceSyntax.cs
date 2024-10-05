#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

namespace FluentMigrator.Builders.Create.Sequence
{
    /// <summary>
    /// Define the sequence options
    /// </summary>
    public interface ICreateSequenceSyntax : IFluentSyntax
    {
        /// <summary>
        /// Defines the increment
        /// </summary>
        /// <param name="increment">The value used to increment the sequence</param>
        /// <returns>Define the sequence options</returns>
        ICreateSequenceSyntax IncrementBy(long increment);

        /// <summary>
        /// Sets the minimum value of the sequence
        /// </summary>
        /// <param name="minValue">The minimum value of the sequence</param>
        /// <returns>Define the sequence options</returns>
        ICreateSequenceSyntax MinValue(long minValue);

        /// <summary>
        /// Sets the maximum value of the sequence
        /// </summary>
        /// <param name="maxValue">The maximum value of the sequence</param>
        /// <returns>Define the sequence options</returns>
        ICreateSequenceSyntax MaxValue(long maxValue);

        /// <summary>
        /// Sets the start value of the sequence
        /// </summary>
        /// <param name="startWith">The start value</param>
        /// <returns>Define the sequence options</returns>
        ICreateSequenceSyntax StartWith(long startWith);

        /// <summary>
        /// Cache the next <paramref name="value"/> number of values for a single sequence increment
        /// </summary>
        /// <remarks>Normally used together with <see cref="IncrementBy"/></remarks>
        /// <param name="value">The number of values to cache</param>
        /// <returns>Define the sequence options</returns>
        ICreateSequenceSyntax Cache(long value);

        /// <summary>
        /// Defines that the sequence starts again with the <see cref="MinValue"/> value for the next value after <see cref="MaxValue"/>
        /// </summary>
        /// <returns>Define the sequence options</returns>
        ICreateSequenceSyntax Cycle();
    }
}
