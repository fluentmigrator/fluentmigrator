#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Represents a stopwatch utility for measuring elapsed time during operations.
    /// </summary>
    /// <remarks>
    /// This class provides functionality to start, stop, and measure the elapsed time of an operation.
    /// It also includes a method to measure the execution time of a given action.
    /// </remarks>
    public class StopWatch : IStopWatch
    {
        /// <summary>
        /// A delegate that provides the current date and time.
        /// </summary>
        /// <remarks>
        /// This field allows overriding the default behavior of retrieving the current date and time
        /// (using <see cref="DateTime.Now"/>) for testing or customization purposes.
        /// </remarks>
        public static Func<DateTime> TimeNow = () => DateTime.Now;

        private DateTime _startTime;
        private DateTime _endTime;

        /// <summary>
        /// Starts the stopwatch, recording the current time as the starting point.
        /// </summary>
        /// <remarks>
        /// This method initializes the stopwatch to measure elapsed time. 
        /// It sets the starting time to the current time, as determined by the <see cref="TimeNow"/> method.
        /// </remarks>
        public void Start()
        {
            _startTime = TimeNow();
        }

        /// <summary>
        /// Stops the stopwatch and records the end time.
        /// </summary>
        /// <remarks>
        /// This method sets the internal end time of the stopwatch to the current time, 
        /// effectively stopping the measurement of elapsed time.
        /// </remarks>
        public void Stop()
        {
            _endTime = TimeNow();
        }

        /// <summary>
        /// Retrieves the total elapsed time measured by the stopwatch.
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the duration between the start and stop times.
        /// </returns>
        /// <remarks>
        /// Ensure that the stopwatch has been started and stopped before calling this method.
        /// Otherwise, the returned value may not represent a valid duration.
        /// </remarks>
        public TimeSpan ElapsedTime()
        {
            return _endTime - _startTime;
        }

        /// <summary>
        /// Measures the time taken to execute the specified action.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to be executed and timed.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the duration of the action's execution.</returns>
        /// <remarks>
        /// This method starts the stopwatch, executes the provided action, stops the stopwatch, 
        /// and then returns the elapsed time.
        /// </remarks>
        public TimeSpan Time(Action action)
        {
            Start();

            action();

            Stop();

            return ElapsedTime();
        }
    }
}
