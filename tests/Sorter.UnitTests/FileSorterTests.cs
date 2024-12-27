using FluentAssertions;
using Sorter.Library;

namespace Sorter.UnitTests;

public class FileSorterTests
{
    private const string InputFilePath = "unsortedfile.txt";
    private const string OutputFilePath = "sortedfile.txt";
    private const string TempDirectory = "temp";
    private const int ChunkSize=100_000;
    
    [Fact]
    public void MergeSort_ShouldSortFile()
    {
        // Arrange
        Directory.CreateDirectory(TempDirectory);
        File.WriteAllLines(InputFilePath,["3. Banana", "1. Apple", "2. Cherry"]);
        
        var sorter = new FileSorter();
        
        // Act
        sorter.MergeSort(InputFilePath, OutputFilePath, TempDirectory, ChunkSize);
        
        // Assert
        var sortedLines = File.ReadAllLines(OutputFilePath);
        
        // Custom expected order
        sortedLines.Should().Equal(new[]
        {
            "1. Apple",
            "3. Banana",
            "2. Cherry",
        });
    }

    
    [Fact]
    public void MergeSort_ShouldCreateOutputFile()
    {
        Directory.CreateDirectory(TempDirectory);
        File.WriteAllLines(InputFilePath,["3. Banana", "1. Apple", "2. Cherry"]);
        
        var sorter = new FileSorter();
        sorter.MergeSort(InputFilePath, OutputFilePath, TempDirectory, ChunkSize);
        
        File.Exists(OutputFilePath).Should().BeTrue();
    }
    
    [Fact]
    public void MergeSort_ShouldHandleEmptyInputFile()
    {
        Directory.CreateDirectory(TempDirectory);
        File.WriteAllText(InputFilePath, string.Empty);
        
        var sorter = new FileSorter();
        sorter.MergeSort(InputFilePath, OutputFilePath, TempDirectory, ChunkSize);
        
        var outputLines = File.ReadAllLines(OutputFilePath);
        outputLines.Should().BeEmpty();
    }
    
    [Fact]
    public void MergeSort_ShouldThrowIfInputFileDoesNotExist()
    {
        var sorter = new FileSorter();
        
        var act=()=>sorter.MergeSort("nonexistentfile.txt",
            OutputFilePath,
            TempDirectory,
            ChunkSize);
        
        act.Should().Throw<FileNotFoundException>();
    }
    
    [Fact]
    public void MergeSort_ShouldCleanUpTemporaryFiles()
    {
        Directory.CreateDirectory(TempDirectory);
        File.WriteAllLines(InputFilePath,["3. Banana", "1. Apple", "2. Cherry"]);
        
        var sorter = new FileSorter();
        sorter.MergeSort(InputFilePath, OutputFilePath, TempDirectory, ChunkSize);
        
        Directory.GetFiles(TempDirectory).Should().BeEmpty();
    }
}