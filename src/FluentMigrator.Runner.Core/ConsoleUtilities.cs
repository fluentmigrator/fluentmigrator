#region License
// Copyright (c) 2018, Fluent Migrator Project
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
    /// Console helper methods
    /// </summary>
    public static class ConsoleUtilities
    {
        /// <summary>
        /// Changes the console color and calls the action
        /// </summary>
        /// <param name="action">Called after the console color has been set</param>
        public static void AsError(Action action)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                action();
            }
            finally
            {
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Changes the console color and calls the action
        /// </summary>
        /// <param name="action">Called after the console color has been set</param>
        public static void AsEmphasize(Action action)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            try
            {
                action();
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
