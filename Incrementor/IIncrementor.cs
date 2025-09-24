namespace Incrementor
{
    public interface IIncrementor
    {
        /// <summary>
        /// Returns the current number. Initially, this is zero.
        /// </summary>
        /// <returns>The current number.</returns>
        int GetNumber();

        /// <summary>
        /// Increments the current number by one. After each call,
        /// <see cref="GetNumber"/> will return a value increased by one.
        /// </summary>
        void IncrementNumber();

        /// <summary>
        /// Sets the maximum value for the current number.
        /// When <see cref="IncrementNumber"/> is called and the current number
        /// reaches this maximum, it is reset to zero.
        /// The default value is <see cref="int.MaxValue"/>.
        /// If the current value exceeds the new maximum when changing it,
        /// the current value will be reset to zero.
        /// Make sure that the value cannot be set below zero.
        /// </summary>
        /// <param name="maximumValue">The maximum value.</param>
        void SetMaximumValue(int maximumValue);
    }
}
