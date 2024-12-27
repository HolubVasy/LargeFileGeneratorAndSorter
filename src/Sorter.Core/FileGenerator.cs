using System.Text;

namespace Sorter.Library
{
    /// <summary>
    /// Provides functionality to generate a test file with random content.
    /// </summary>
    public sealed class FileGenerator
    {
        private static readonly Lock FileLock = new();

        /// <summary>
        /// Generates a test file with random content.
        /// </summary>
        /// <param name="filePath">The path where the test file will be created.</param>
        /// <param name="fileSizeInMb">The size of the test file in megabytes.</param>
        /// <param name="bufferFlushThreshold">The number of lines to accumulate before flushing
        /// them to disk, reducing lock contention.</param>
        /// <returns>The number of tasks spawned for generation.</returns>
        public int GenerateTestFile(string filePath, int fileSizeInMb, int bufferFlushThreshold)
        {
            var targetSizeBytes = ConvertMbToBytes(fileSizeInMb);
            long currentSize = 0;
            var cancellationTokenSource = new CancellationTokenSource();

            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            var processorCount = Environment.ProcessorCount;
            var tasks = new List<Task>();

            for (var i = 0; i < processorCount; i++)
            {
                tasks.Add(Task.Run(() =>
                    WriteData(writer,
                        bufferFlushThreshold,
                        ref currentSize,
                        targetSizeBytes,
                        cancellationTokenSource.Token,
                        cancellationTokenSource)));
            }

            Task.WaitAll(tasks.ToArray());

            return tasks.Count;
        }

        private long ConvertMbToBytes(int mb) => mb * 1024L * 1024L;

        /// <summary>
        /// Writes random lines to the single StreamWriter until the target size is reached.
        /// Here, we reduce lock contention by aggregating lines in a local buffer.
        /// </summary>
        private void WriteData(
            StreamWriter globalWriter,
            int bufferFlushThreshold,
            ref long currentSize,
            long targetSize,
            CancellationToken token,
            CancellationTokenSource cts)
        {
            var random = new Random();
            var predefinedStrings = new[]
            {
                "Apple",
                "Banana is yellow",
                "Cherry is the best",
                "Something something something"
            };

            var localBuffer = new StringBuilder(capacity: 32_768);
            long localBufferSize = 0;

            while (!token.IsCancellationRequested)
            {
                var line = GenerateRandomLine(random, predefinedStrings);
                long lineSize = Encoding.UTF8.GetByteCount(line + Environment.NewLine);

                var newSize = Interlocked.Add(ref currentSize, lineSize);

                if (newSize > targetSize)
                {
                    // Revert and signal cancellation if overshoot
                    Interlocked.Add(ref currentSize, -lineSize);
                    cts.Cancel();
                    break;
                }

                // Accumulate line in local buffer
                localBuffer.AppendLine(line);
                localBufferSize += lineSize;

                // Flush if buffer threshold reached
                if (localBuffer.Length >= bufferFlushThreshold)
                {
                    FlushLocalBuffer(globalWriter, localBuffer, ref localBufferSize, token);
                }
            }

            // Final flush
            FlushLocalBuffer(globalWriter, localBuffer, ref localBufferSize, token);
        }

        private string GenerateRandomLine(Random random, string[] predefinedStrings)
        {
            var number = random.Next(1, 100_000);
            var text = predefinedStrings[random.Next(predefinedStrings.Length)];
            return $"{number}. {text}";
        }

        private void FlushLocalBuffer(
            StreamWriter globalWriter,
            StringBuilder localBuffer,
            ref long localBufferSize,
            CancellationToken token)
        {
            if (localBuffer.Length == 0 || token.IsCancellationRequested)
                return;

            lock (FileLock)
            {
                if (!token.IsCancellationRequested)
                {
                    globalWriter.Write(localBuffer.ToString());
                    globalWriter.Flush();
                }
            }

            localBuffer.Clear();
            localBufferSize = 0;
        }
    }
}
