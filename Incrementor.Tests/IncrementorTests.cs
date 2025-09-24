using System.Collections.Concurrent;

namespace Incrementor.Tests
{
    /// <summary>
    /// Unit tests for the class <see cref="Incrementor"/>.
    /// </summary>
    public class IncrementorTests
    {
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

        [Fact]
        public void Constructor_WithNegativeMaximumValue_ThrowsArgumentException()
        {
            // Arrange, Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Incrementor(-1));
            Assert.Equal("maximumValue", exception.ParamName);
        }

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

        [Fact]
        public void SetMaximumValue_WithNegativeValue_ThrowsArgumentException()
        {
            // Arrange
            var incrementor = new Incrementor();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => incrementor.SetMaximumValue(-5));
            Assert.Equal("maximumValue", exception.ParamName);
        }

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