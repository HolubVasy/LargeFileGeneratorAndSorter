using Sorter.Library;

namespace Sorter;

static class Program
{
    static void Main()
    {
        Console.Write("Enter the root folder path: ");
        var rootFolder = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(rootFolder))
        {
            Console.WriteLine("Root folder path cannot be empty.");
            return;
        }
        
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }
        
        Console.Write("Enter the target file size in MB: ");
        var fileSizeAsString = Console.ReadLine()?.Trim();
        if (!int.TryParse(fileSizeAsString, out var fileSizeMb) || fileSizeMb <= 0)
        {
            Console.WriteLine("Target file size must be a positive integer.");
            return;
        }
        
        var inputFilePath = Path.Combine(rootFolder, "LargeFile.txt");
        var sortedFilePath = Path.Combine(rootFolder, "SortedFile.txt");
        var tempDirectory = Path.Combine(rootFolder, "temp");
        
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }
        
        Console.WriteLine("Starting file generation...");
        var fileGenerator = new FileGenerator();
        const int bufferFlushThreshold = 10_000; // Could also prompt user for the fine turning.
        fileGenerator.GenerateTestFile(inputFilePath, fileSizeMb, bufferFlushThreshold);
        Console.WriteLine("Test file generation complete.");
        
        Console.WriteLine("Starting sorting...");
        const int chunkSize=100_000; // Could also prompt user for the fine turning.
        var fileSorter = new FileSorter();
        fileSorter.MergeSort(inputFilePath, sortedFilePath, tempDirectory, chunkSize);
        Console.WriteLine("Sorting complete.");
    }
}