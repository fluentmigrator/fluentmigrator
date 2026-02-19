using System;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Represents a stopwatch utility for measuring elapsed time and timing actions.
    /// </summary>
    public interface IStopWatch
    {
        /// <summary>
        /// Starts the stopwatch to begin measuring elapsed time.
        /// </summary>
        /// <remarks>
        /// This method initializes the stopwatch and sets the starting point for measuring time.
        /// It should be paired with a call to <see cref="Stop"/> to determine the elapsed time.
        /// </remarks>
        void Start();
        /// <summary>
        /// Stops the stopwatch and records the end time.
        /// </summary>
        /// <remarks>
        /// This method is used to stop the stopwatch after it has been started using <see cref="Start"/>.
        /// The elapsed time can then be retrieved using <see cref="ElapsedTime"/>.
        /// </remarks>
        void Stop();
        /// <summary>
        /// Retrieves the total elapsed time measured by the stopwatch.
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the duration between the start and stop times.
        /// </returns>
        /// <remarks>
        /// This method calculates the elapsed time by subtracting the start time from the stop time.
        /// Ensure that the stopwatch has been started using <see cref="Start"/> and stopped using <see cref="Stop"/> 
        /// before calling this method to get an accurate measurement.
        /// </remarks>
        TimeSpan ElapsedTime();
        /// <summary>
        /// Measures the time taken to execute the specified action.
        /// </summary>
        /// <param name="action">The action to be executed and timed.</param>
        /// <returns>The elapsed time as a <see cref="TimeSpan"/> representing the duration of the action.</returns>
        TimeSpan Time(Action action);
    }
}
