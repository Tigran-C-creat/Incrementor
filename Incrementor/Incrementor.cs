namespace Incrementor
{
    /// <summary>
    /// Represents a thread-safe incrementor that can increment a number, reset it when a maximum value is reached, 
    /// and allows setting a custom maximum value.
    /// </summary>
    public class Incrementor : IIncrementor
    {
        private int _currentNumber;
        private int _maximumValue = int.MaxValue;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Incrementor"/> class with the default maximum value.
        /// </summary>
        public Incrementor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Incrementor"/> class with a specified maximum value.
        /// </summary>
        /// <param name="maximumValue">The maximum value for the incrementor. Must be non-negative.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="maximumValue"/> is negative.</exception>
        public Incrementor(int maximumValue)
        {
            SetMaximumValue(maximumValue);
        }

        /// <summary>
        /// Gets the current number.
        /// </summary>
        /// <returns>The current number.</returns>
        public int GetNumber()
        {
            lock (_lockObject)
            {
                return _currentNumber;
            }
        }

        /// <summary>
        /// Increments the current number by one. If the number exceeds the maximum value, it resets to zero.
        /// </summary>
        public void IncrementNumber()
        {
            lock (_lockObject)
            {
                _currentNumber++;

                if (_currentNumber > _maximumValue)
                {
                    _currentNumber = 0;
                }
            }
        }

        /// <summary>
        /// Sets the maximum value for the incrementor. If the current number exceeds the new maximum value, it resets to zero.
        /// </summary>
        /// <param name="maximumValue">The new maximum value. Must be non-negative.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="maximumValue"/> is negative.</exception>
        public void SetMaximumValue(int maximumValue)
        {
            if (maximumValue < 0)
            {
                throw new ArgumentException(
                    "The maximum value cannot be negative.",
                    nameof(maximumValue));
            }

            lock (_lockObject)
            {
                _maximumValue = maximumValue;

                if (_currentNumber > _maximumValue)
                {
                    _currentNumber = 0;
                }
            }
        }
    }
}
