using System.Text;

namespace Sorter.Library;

/// <summary>
/// Provides functionality to sort large files by splitting them into chunks,
/// sorting each chunk, and merging them back into a single sorted file.
/// </summary>
public sealed class FileSorter
{
    /// <summary>
    /// Splits the input file into sorted chunks, then merges the sorted chunks into
    /// the final sorted output file.
    /// </summary>
    public void MergeSort(string inputFilePath,string outputFilePath,string tempDirectory,
        int chunkSize)
    {
        var chunkFiles = SplitAndSortChunks(inputFilePath, tempDirectory, chunkSize);
        MergeSortedChunks(chunkFiles, outputFilePath);
    }
    
    /// <summary>
    /// Reads the input file in chunks, sorts each chunk in memory, and writes to a temporary
    /// file. Optionally, we can parallelize the sorting of chunks if we read them all first.
    /// </summary>
    private static List<string> SplitAndSortChunks(string inputFilePath,string tempDirectory,
        int chunkSize)
    {
        if (!File.Exists(inputFilePath))
            throw new FileNotFoundException("Input file does not exist.", inputFilePath);
        
        Directory.CreateDirectory(tempDirectory);
        
        var chunkFiles = new List<string>();
        var comparer = new PriorityQueueComparer();
        
        using var reader = new StreamReader(inputFilePath, Encoding.UTF8);
        var chunkIndex = 0;
        
        while (!reader.EndOfStream)
        {
            var lines = new List<string>(chunkSize);
            for (var i = 0; i < chunkSize && !reader.EndOfStream; i++)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }
            
            if (lines.Count == 0)
                break;
            
            // Sort in memory
            lines.Sort(comparer.CompareLines);
            
            // Write chunk to a temp file
            var chunkFile = Path.Combine(tempDirectory, $"chunk_{chunkIndex++}.txt");
            File.WriteAllLines(chunkFile, lines, Encoding.UTF8);
            chunkFiles.Add(chunkFile);
        }
        
        return chunkFiles;
    }
    
    /// <summary>
    /// Merges all sorted chunk files into a single sorted output file.
    /// </summary>
    private static void MergeSortedChunks(List<string> chunkFiles, string outputFilePath)
    {
        var comparer = new PriorityQueueComparer();
        var pq=
            new PriorityQueue<(string line,int fileIndex),(string line,int fileIndex)>(
                comparer);
        
        // Open one reader per chunk
        var readers = new StreamReader[chunkFiles.Count];
        try
        {
            for (var i = 0; i < chunkFiles.Count; i++)
            {
                readers[i] = new StreamReader(chunkFiles[i], Encoding.UTF8);
                if(readers[i].EndOfStream)
                {
                    continue;
                }
                var line = readers[i].ReadLine()!;
                pq.Enqueue((line, i), (line, i));
            }
            
            // Merge into output
            using var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);
            while (pq.Count > 0)
            {
                var (smallestLine, fileIndex) = pq.Dequeue();
                writer.WriteLine(smallestLine);
                
                // Read the next line from the same reader
                if(readers[fileIndex].EndOfStream)
                {
                    continue;
                }
                var line = readers[fileIndex].ReadLine()!;
                pq.Enqueue((line, fileIndex), (line, fileIndex));
            }
        }
        finally
        {
            // Close and delete
            foreach (var reader in readers)
            {
                reader?.Close();
            }
            
            // Cleanup
            foreach (var chunkFile in chunkFiles)
            {
                File.Delete(chunkFile);
            }
        }
    }
}