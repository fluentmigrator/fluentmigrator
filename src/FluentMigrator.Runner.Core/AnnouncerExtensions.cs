#region License

// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for an <see cref="IAnnouncer"/>
    /// </summary>
    [Obsolete]
    public static class AnnouncerExtensions
    {
        /// <summary>
        /// Writes a formatted heading message
        /// </summary>
        /// <param name="announcer">The announcer used to write the message</param>
        /// <param name="message">The message to format</param>
        /// <param name="args">The arguments</param>
        [Obsolete]
        public static void Heading(this IAnnouncer announcer, string message, params object[] args)
        {
            announcer.Heading(string.Format(message, args));
        }

        /// <summary>
        /// Writes a formatted message
        /// </summary>
        /// <param name="announcer">The announcer used to write the message</param>
        /// <param name="message">The message to format</param>
        /// <param name="args">The arguments</param>
        [Obsolete]
        public static void Say(this IAnnouncer announcer, string message, params object[] args)
        {
            announcer.Say(string.Format(message, args));
        }

        /// <summary>
        /// Writes a formatted error message
        /// </summary>
        /// <param name="announcer">The announcer used to write the message</param>
        /// <param name="message">The message to format</param>
        /// <param name="args">The arguments</param>
        [Obsolete]
        public static void Error(this IAnnouncer announcer, string message, params object[] args)
        {
            announcer.Error(string.Format(message, args));
        }
    }
}
