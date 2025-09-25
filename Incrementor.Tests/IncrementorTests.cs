using System.Collections.Concurrent;

namespace Incrementor.Tests
{
    /// <summary>
    /// Unit tests for the class <see cref="Incrementor"/>.
    /// </summary>
    public class IncrementorTests
    {
        /// <summary>
        /// Verifies that the constructor of the <see cref="Incrementor"/> class always initializes the current number to 0,
        /// regardless of whether a maximum value was provided.
        /// </summary>
        /// <param name="maxValue">
        /// The maximum value passed to the constructor. May be <c>null</c> to invoke the default constructor.
        /// </param>
        [Theory]
        [InlineData(null)]  // Default constructor
        [InlineData(10)]    // Constructor with value
        public void Constructor_Always_InitializesCurrentNumberToZero(int? maxValue)
        {
            // Arrange & Act
            var incrementor = maxValue.HasValue ? new Incrementor(maxValue.Value) : new Incrementor();

            // Assert
            Assert.Equal(0, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that the <see cref="Incrementor"/> constructor throws an <see cref="ArgumentException"/>
        /// when initialized with a negative maximum value.
        /// </summary>
        /// <remarks>
        /// This test ensures that invalid input is properly validated and that the exception includes the correct parameter name.
        /// </remarks>
        [Fact]
        public void Constructor_WithNegativeMaximumValue_ThrowsArgumentException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Incrementor(-1));
            Assert.Equal("maximumValue", exception.ParamName);
        }

        /// <summary>
        /// Verifies that calling <see cref="Incrementor.IncrementNumber"/> multiple times correctly increments the internal counter.
        /// </summary>
        /// <param name="callCount">
        /// The number of times <see cref="Incrementor.IncrementNumber"/> is called.
        /// </param>
        /// <remarks>
        /// Ensures that the incrementor's internal state reflects the expected value after repeated increments.
        /// </remarks>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void IncrementNumber_CalledMultipleTimes_IncrementsCorrectly(int callCount)
        {
            // Arrange
            var incrementor = new Incrementor();

            // Act
            for (int i = 0; i < callCount; i++)
            {
                incrementor.IncrementNumber();
            }

            // Assert
            Assert.Equal(callCount, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that <see cref="Incrementor.IncrementNumber"/> resets to 0 on the increment
        /// that would exceed the maximum value.
        /// </summary>
        /// <remarks>
        /// This test checks the full increment sequence to ensure that the counter wraps around correctly
        /// when the maximum is reached.
        /// </remarks>
        [Fact]
        public void IncrementNumber_WhenReachesMaximum_ResetsToZero()
        {
            // Arrange
            var incrementor = new Incrementor(3);

            // Act & Assert - we check the entire sequence
            incrementor.IncrementNumber();
            Assert.Equal(1, incrementor.GetNumber()); // 1

            incrementor.IncrementNumber();
            Assert.Equal(2, incrementor.GetNumber()); // 2

            incrementor.IncrementNumber();
            Assert.Equal(3, incrementor.GetNumber()); // 3 (maximum))

            incrementor.IncrementNumber();
            Assert.Equal(0, incrementor.GetNumber()); // 0 (reset)
        }

        /// <summary>
        /// Verifies that after the <see cref="Incrementor"/> reaches its maximum value and resets to zero,
        /// subsequent calls to <see cref="Incrementor.IncrementNumber"/> continue incrementing correctly from zero.
        /// </summary>
        /// <remarks>
        /// This test ensures the counter behaves cyclically and resumes incrementing after a reset without errors or unexpected behavior.
        /// </remarks>
        [Fact]
        public void IncrementNumber_AfterReset_ContinuesIncrementingFromZero()
        {
            // Arrange
            var incrementor = new Incrementor(2);

            // Act & Assert - we check the entire sequence
            incrementor.IncrementNumber(); // 1
            Assert.Equal(1, incrementor.GetNumber());

            incrementor.IncrementNumber(); // 2
            Assert.Equal(2, incrementor.GetNumber());

            incrementor.IncrementNumber(); // 0 (reset)
            Assert.Equal(0, incrementor.GetNumber());

            incrementor.IncrementNumber(); // 1
            Assert.Equal(1, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that the <see cref="Incrementor"/> correctly resets its current number to 0
        /// after reaching the specified maximum value.
        /// </summary>
        /// <param name="maxValue">
        /// The maximum value to set before incrementing. The test runs the increment operation
        /// <c>maxValue + 1</c> times to ensure the counter wraps around to zero.
        /// </param>
        /// <remarks>
        /// This test confirms that the reset behavior works consistently across a range of maximum values.
        /// </remarks>
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void IncrementNumber_WithVariousMaximums_ResetsCorrectly(int maxValue)
        {
            // Arrange
            var incrementor = new Incrementor();

            // Act
            incrementor.SetMaximumValue(maxValue);
            for (int i = 0; i <= maxValue; i++)
            {
                incrementor.IncrementNumber();
            }

            // Assert
            Assert.Equal(0, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that setting the maximum value of the <see cref="Incrementor"/> to <see cref="int.MaxValue"/>
        /// does not throw an exception and allows incrementing as expected.
        /// </summary>
        /// <remarks>
        /// This test ensures that the incrementor can handle the largest possible integer value without failure or overflow.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_WithMaxInt_DoesNotThrow()
        {
            // Arrange
            var incrementor = new Incrementor();

            // Act & Assert
            incrementor.SetMaximumValue(int.MaxValue);
            incrementor.IncrementNumber();
            Assert.Equal(1, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that setting a valid maximum value using <see cref="Incrementor.SetMaximumValue"/> 
        /// updates the internal limit and causes the counter to reset correctly upon reaching the new maximum.
        /// </summary>
        /// <remarks>
        /// This test ensures that the incrementor respects the newly assigned maximum and wraps around to zero as expected.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_WithValidValue_RespectsNewMaximum()
        {
            // Arrange
            var incrementor = new Incrementor();

            // Act & Assert - we check the entire sequence
            incrementor.SetMaximumValue(2);

            Assert.Equal(0, incrementor.GetNumber()); 

            incrementor.IncrementNumber(); // 1
            Assert.Equal(1, incrementor.GetNumber());

            incrementor.IncrementNumber(); // 2  
            Assert.Equal(2, incrementor.GetNumber());

            incrementor.IncrementNumber(); // 0 (reset at new maximum)
            Assert.Equal(0, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that calling <see cref="Incrementor.SetMaximumValue"/> with a negative value
        /// throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <remarks>
        /// This test ensures that the method validates input correctly and rejects invalid maximum values,
        /// providing the appropriate parameter name in the exception.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_WithNegativeValue_ThrowsArgumentException()
        {
            // Arrange
            var incrementor = new Incrementor();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => incrementor.SetMaximumValue(-5));
            Assert.Equal("maximumValue", exception.ParamName);
        }

        /// <summary>
        /// Verifies that when <see cref="Incrementor.SetMaximumValue"/> is called with a value lower than the current number,
        /// the current number is reset to zero.
        /// </summary>
        /// <remarks>
        /// This test ensures that the incrementor maintains consistency by resetting its state when the new maximum
        /// is less than the current value.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_WhenCurrentExceedsNewMaximum_ResetsToZero()
        {
            // Arrange
            var incrementor = new Incrementor(10);

            // Act - increment to 5
            for (int i = 0; i < 5; i++)
            {
                incrementor.IncrementNumber(); // Current = 5
            }

            // Assert - verify current value before changing maximum
            Assert.Equal(5, incrementor.GetNumber());

            // Act - set maximum lower than current value
            incrementor.SetMaximumValue(3); // Current = 5 > 3, so reset to 0

            // Assert - verify reset to zero
            Assert.Equal(0, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that when the maximum value of the <see cref="Incrementor"/> is set to zero,
        /// any call to <see cref="Incrementor.IncrementNumber"/> immediately resets the current number to zero.
        /// </summary>
        /// <remarks>
        /// This test ensures that the incrementor handles a zero maximum value correctly by preventing any increment beyond zero.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_ToZero_ResetsToZeroOnAnyIncrement()
        {
            // Arrange
            var incrementor = new Incrementor(5);
            incrementor.SetMaximumValue(0);

            // Act
            incrementor.IncrementNumber();

            // Assert
            Assert.Equal(0, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that setting the maximum value of the <see cref="Incrementor"/> to the current number
        /// does not cause a reset of the current value.
        /// </summary>
        /// <remarks>
        /// This test ensures that the incrementor maintains its current state when the new maximum equals the current number,
        /// preserving expected behavior without unnecessary resets.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_SameAsCurrent_DoesNotReset()
        {
            // Arrange
            var incrementor = new Incrementor(10);
            incrementor.IncrementNumber(); // Current = 1
            
            // Act
            incrementor.SetMaximumValue(1); // Current = 1, Maximum = 1

            // Assert
            Assert.Equal(1, incrementor.GetNumber());
        }

        /// <summary>
        /// Verifies that <see cref="Incrementor.IncrementNumber"/> behaves correctly under concurrent access,
        /// ensuring no increments are lost and no exceptions are thrown.
        /// </summary>
        /// <remarks>
        /// This test runs multiple parallel increment operations and checks that the final value matches the expected result
        /// based on the cycle length. It also confirms that the incrementor handles multithreaded usage without throwing errors.
        /// </remarks>
        [Fact]
        public void IncrementNumber_Multithreaded_NoLostIncrements()
        {
            // Arrange
            var incrementor = new Incrementor(1000);
            const int iterations = 10000;
            const int cycleLength = 1001;
            var exceptions = new ConcurrentQueue<Exception>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            try
            {
                Parallel.For(0, iterations, new ParallelOptions { CancellationToken = cts.Token }, _ =>
                {
                    try
                    {
                        incrementor.IncrementNumber();
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Assert.Fail("Test timed out after 30 seconds");
            }

            // Assert
            Assert.Empty(exceptions);
            var expected = iterations % cycleLength;
            Assert.Equal(expected, incrementor.GetNumber());
            Assert.InRange(incrementor.GetNumber(), 0, 1000);
        }

        /// <summary>
        /// Verifies that <see cref="Incrementor.GetNumber"/> returns consistent and valid values
        /// when accessed concurrently during multiple increment operations.
        /// </summary>
        /// <remarks>
        /// This test ensures that the incrementor behaves reliably under multithreaded conditions,
        /// producing values within the expected range and without throwing exceptions.
        /// </remarks>
        [Fact]
        public void GetNumber_WhenCalledConcurrently_ReturnsConsistentValues()
        {
            // Arrange
            var incrementor = new Incrementor(100);
            const int iterations = 1000;
            var results = new ConcurrentBag<int>();
            var exceptions = new ConcurrentQueue<Exception>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            try
            {
                Parallel.For(0, iterations, new ParallelOptions { CancellationToken = cts.Token }, i =>
                {
                    try
                    {
                        incrementor.IncrementNumber();
                        var value = incrementor.GetNumber();
                        results.Add(value);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Assert.Fail("Test timed out after 30 seconds");
            }

            // Assert
            Assert.Empty(exceptions);
            foreach (var result in results)
            {
                Assert.InRange(result, 0, 100);
            }
        }

        /// <summary>
        /// Verifies that <see cref="Incrementor.SetMaximumValue"/> and <see cref="Incrementor.IncrementNumber"/>
        /// can be called concurrently without throwing exceptions or causing inconsistent behavior.
        /// </summary>
        /// <remarks>
        /// This test performs simultaneous updates to the maximum value and increment operations using <see cref="Parallel.Invoke"/>.
        /// It ensures that the incrementor maintains a valid state and that the current number remains within the expected range.
        /// </remarks>
        [Fact]
        public void SetMaximumValue_WhenCalledConcurrently_MaintainsConsistency()
        {
            // Arrange
            var incrementor = new Incrementor(1000);
            const int iterations = 1000;
            var exceptions = new ConcurrentQueue<Exception>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act - update maximum and increment concurrently
            try
            {
                Parallel.Invoke(
                    () =>
                    {
                        for (int i = 0; i < iterations; i++)
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            try
                            {
                                incrementor.SetMaximumValue(i % 100 + 1);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Enqueue(ex);
                            }
                        }
                    },
                    () =>
                    {
                        for (int i = 0; i < iterations; i++)
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            try
                            {
                                incrementor.IncrementNumber();
                            }
                            catch (Exception ex)
                            {
                                exceptions.Enqueue(ex);
                            }
                        }
                    }
                );
            }
            catch (OperationCanceledException)
            {
                Assert.Fail("Test timed out after 30 seconds");
            }

            // Assert
            Assert.Empty(exceptions);
            Assert.InRange(incrementor.GetNumber(), 0, 100);
        }

        /// <summary>
        /// Verifies that the <see cref="Incrementor"/> maintains consistent behavior under high-concurrency stress conditions
        /// involving mixed operations: incrementing, reading, and updating the maximum value.
        /// </summary>
        /// <remarks>
        /// This test performs 5000 concurrent operations using <see cref="Parallel.For"/>, combining calls to
        /// <see cref="Incrementor.IncrementNumber"/>, <see cref="Incrementor.GetNumber"/>, and <see cref="Incrementor.SetMaximumValue"/>.
        /// It ensures that the incrementor remains stable, throws no exceptions, and keeps its internal state within valid bounds.
        /// </remarks>
        [Fact]
        public void StressTest_ConcurrentOperations_MaintainsConsistency()
        {
            // Arrange
            var incrementor = new Incrementor(500);
            const int operations = 5000;
            var exceptions = new ConcurrentQueue<Exception>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            try
            {
                Parallel.For(0, operations, new ParallelOptions { CancellationToken = cts.Token }, i =>
                {
                    try
                    {
                        switch (i % 4)
                        {
                            case 0: incrementor.IncrementNumber(); break;
                            case 1: incrementor.GetNumber(); break;
                            case 2: incrementor.SetMaximumValue(i % 100 + 1); break;
                            case 3: incrementor.IncrementNumber(); break;
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                Assert.Fail("Test timed out after 30 seconds");
            }

            // Assert
            Assert.Empty(exceptions);
            Assert.InRange(incrementor.GetNumber(), 0, 100);
        }

        /// <summary>
        /// Verifies a complete usage scenario of the <see cref="Incrementor"/> class,
        /// including incrementing to the maximum value, automatic reset, and updating the maximum value mid-cycle.
        /// </summary>
        /// <remarks>
        /// This test simulates two full increment cycles with different maximum values (3 and 2),
        /// ensuring that the counter resets correctly and continues functioning as expected after configuration changes.
        /// </remarks>
        [Fact]
        public void IntegrationTest_CompleteScenario()
        {
            // Arrange
            var incrementor = new Incrementor(3);
            // Initial increment cycle with max = 3
            incrementor.IncrementNumber();
            Assert.Equal(1, incrementor.GetNumber());

            incrementor.IncrementNumber();
            Assert.Equal(2, incrementor.GetNumber());

            incrementor.IncrementNumber();
            Assert.Equal(3, incrementor.GetNumber());

            incrementor.IncrementNumber(); // reset
            Assert.Equal(0, incrementor.GetNumber());

            // Change max to 2
            incrementor.SetMaximumValue(2);
            Assert.Equal(0, incrementor.GetNumber());

            // New increment cycle with max = 2
            incrementor.IncrementNumber();
            Assert.Equal(1, incrementor.GetNumber());

            incrementor.IncrementNumber();
            Assert.Equal(2, incrementor.GetNumber());

            incrementor.IncrementNumber(); // reset
            // Final state should be 0 after second reset
            Assert.Equal(0, incrementor.GetNumber());
        }
    }
}