namespace FluentMigrator.Helpers
{
    /// <summary>
    /// Allows callers to easily increment from a given first value using a given increment.
    /// </summary>
    public class Incrementor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Incrementor"/> class.
        /// </summary>
        /// <param name="firstValue">The first value that the Increment method will return.</param>
        /// <param name="increment">The value that the Increment method will increment by.</param>
        public Incrementor(int firstValue, int increment)
        {
            _increment = increment;
            _nextValue = firstValue;
        }

        /// <summary>
        /// Increments the previously returned value.
        /// </summary>
        /// <returns>The previous value of the Increment method, plus the Increment provided in the constructor.</returns>
        public int Increment()
        {
            var result = _nextValue;
            _nextValue += _increment;
            return result;
        }

        /// <summary>
        /// Gets the next value that will be returned by the Increment method.
        /// </summary>
        public int NextValue
        {
            get
            {
                return _nextValue;
            }
        }

        /// <summary>
        /// Gets the previous value returned by the Increment method.
        /// </summary>
        public int PreviousValue
        {
            get
            {
                return _nextValue - _increment;
            }
        }

        private readonly int _increment;

        private int _nextValue;
    }
}
