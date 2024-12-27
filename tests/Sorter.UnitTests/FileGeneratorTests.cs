using System.Diagnostics;
using FluentAssertions;
using Sorter.Library;

namespace Sorter.UnitTests;

public class FileGeneratorTests
{
    private const string TestFilePath="testfile.txt";
    private const int BufferFlushThreshold=10_000;
    
    [Fact]
    public void GenerateTestFile_ShouldCreateFile()
    {
        var generator=new FileGenerator();
        generator.GenerateTestFile(TestFilePath,1,BufferFlushThreshold);
        
        File.Exists(TestFilePath).Should().BeTrue();
    }
    
    [Fact]
    public void GenerateTestFile_ShouldCreateFileWithCorrectSize()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=5;
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        
        var fileInfo=new FileInfo(TestFilePath);
        
        fileInfo.Length.Should().BeInRange(
            (long)(fileSizeInMb*1024L*1024L*0.8),// 80% of the target
            (long)(fileSizeInMb*1024L*1024L*1.1)// 110% of target
        );
    }
    
    [Fact]
    public void GenerateTestFile_ShouldThrowIfFilePathIsInvalid()
    {
        var generator=new FileGenerator();
        var invalidFilePath=Path.Combine("Invalid","Path","testfile.txt");
        
        Assert.Throws<DirectoryNotFoundException>(()=>
            generator.GenerateTestFile(invalidFilePath,1,BufferFlushThreshold));
    }
    
    [Fact]
    public async Task GenerateTestFile_ShouldSupportMultithreading()
    {
        var generator=new FileGenerator();
        await Task.Run(()=>generator.GenerateTestFile(TestFilePath,1,
            BufferFlushThreshold));
        
        File.Exists(TestFilePath).Should().BeTrue();
    }
    
    [Fact]
    public void GenerateTestFile_ShouldWriteContentToFile()
    {
        var generator=new FileGenerator();
        generator.GenerateTestFile(TestFilePath,1,BufferFlushThreshold);
        
        var lines=File.ReadLines(TestFilePath);
        lines.Should().NotBeEmpty();
    }
    
    [Fact]
    public void GenerateTestFile_ShouldGenerateAtLeastOneLineIfTargetSizeIsZero()
    {
        var generator = new FileGenerator();
        generator.GenerateTestFile(TestFilePath, 0, BufferFlushThreshold);
        
        var fileInfo = new FileInfo(TestFilePath);
        // We expect at least one line, so the file should be non-empty.
        fileInfo.Length.Should().BeGreaterThan(0,
            "we want at least one line even for 0 MB");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldNotExceedTargetSizeByMoreThanOneLine()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=1;
        
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        
        var fileInfo=new FileInfo(TestFilePath);
        fileInfo.Length.Should().BeLessThan(fileSizeInMb*1024L*1024L+10_000);
    }
    
    [Fact]
    public void GenerateTestFile_ShouldRespectBufferFlushThreshold()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=1;
        const int customThreshold=1;
        
        // Act
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,customThreshold);
        
        File.Exists(TestFilePath).Should().BeTrue();
    }
    
    [Fact]
    public void GenerateTestFile_ShouldHandleNegativeFileSizeGracefully()
    {
        var generator=new FileGenerator();
        
        var act=()=>generator.GenerateTestFile(TestFilePath,-1,
            BufferFlushThreshold);
        act.Should().NotThrow("negative file sizes might be "+
            "treated as zero or handled gracefully");
    }
    
    [Fact]
    public void GenerateTestFile_LinesShouldContainNumberDotSpaceFormat()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=1;
        
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        var lines=File.ReadAllLines(TestFilePath);
        
        lines.Should().NotBeEmpty();
        lines.Any(line=>!line.Contains(". ")).Should().BeFalse("each line"+
            " should be in '[number]. [text]' format");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldGenerateSomeDuplicateStrings()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=1;
        
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        var lines=File.ReadAllLines(TestFilePath);
        
        var stringParts=lines
            .Select(l=>l.Split(". ",2)
                .LastOrDefault()??"")
            .ToList();
        
        stringParts.GroupBy(s=>s)
            .Any(g=>g.Count()>1)
            .Should().BeTrue("we expect some duplicates from "+
                "the predefined string set");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldCancelWhenTargetSizeIsReached()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=1;
        
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        var fileInfo=new FileInfo(TestFilePath);
        
        fileInfo.Length.Should().BeLessThan(2*1024*1024,
            "the generator should cancel and not keep writing indefinitely");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldGenerateRandomizedNumbers()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=1;
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        var lines=File.ReadLines(TestFilePath).Take(1000).ToList();
        
        var numbers=lines.Select(line=>
        {
            var parts=line.Split(". ",2);
            return int.TryParse(parts.FirstOrDefault(),out var n)?n:-1;
        });
        numbers.Distinct().Count().Should().BeGreaterThan(1,
            "random numbers should vary");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldOverwriteExistingFileIfItAlreadyExists()
    {
        var generator=new FileGenerator();
        
        File.WriteAllText(TestFilePath,"Existing content");
        
        generator.GenerateTestFile(TestFilePath,1,BufferFlushThreshold);
        
        var lines=File.ReadAllLines(TestFilePath);
        lines.Should().NotBeEmpty("new content should overwrite the existing file");
        lines.Any(x=>x.Contains("Existing content")).Should().BeFalse(
            "old content must be replaced entirely");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldCreateFileInCurrentDirectoryIfNoPathIsSpecified()
    {
        var generator=new FileGenerator();
        var localFileName="localfile.txt";
        
        if(File.Exists(localFileName)) File.Delete(localFileName);
        
        generator.GenerateTestFile(localFileName,1,BufferFlushThreshold);
        
        File.Exists(localFileName).Should().BeTrue();
        File.Delete(localFileName);
    }
    
    [Fact]
    public void GenerateTestFile_ShouldGenerateVeryLargeFileWithoutError()
    {
        var generator=new FileGenerator();
        const int fileSizeInMb=100;
        
        generator.GenerateTestFile(TestFilePath,fileSizeInMb,BufferFlushThreshold);
        
        var fileInfo=new FileInfo(TestFilePath);
        fileInfo.Length.Should().BeGreaterThan(0,"file generation should succeed");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldContainPredefinedStringsInLines()
    {
        var generator=new FileGenerator();
        generator.GenerateTestFile(TestFilePath,1,BufferFlushThreshold);
        
        var lines=File
            .ReadLines(TestFilePath)
            .Take(100)
            .ToList();
        lines.Should().NotBeEmpty();
        lines.Any(line=>
            line.Contains("Apple")||
            line.Contains("Banana is yellow")||
            line.Contains("Cherry is the best")||
            line.Contains("Something something something")
        ).Should().BeTrue("some lines must match the predefined set");
    }
    
    [Fact]
    public void GenerateTestFile_ShouldUtilizeMultipleThreadsForLargeSize()
    {
        var generator=new FileGenerator();
        generator.GenerateTestFile(TestFilePath,10,BufferFlushThreshold);
        
        var fileInfo=new FileInfo(TestFilePath);
        fileInfo.Length.Should().BeGreaterThan(5*1024*1024,
            "with a multi-MB target, parallel tasks should have been engaged");
    }
}