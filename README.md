Multiple lines can share the same string.

2. **Sorter** – Reads such a file, then outputs a sorted file using these criteria:
1. Sort by **string part** in ascending order.
2. If the string parts are identical, sort by **number** in ascending order.

The repository uses a standard C# solution structure with multiple projects.  
Both the generator and sorter include considerations for very large files (e.g., external merge sort, chunk-based approach).

## Table of Contents
- [Requirements](#requirements)
- [How It Works](#how-it-works)
- [Generator Overview](#generator-overview)
- [Sorter Overview](#sorter-overview)
- [How to Build](#how-to-build)
- [How to Run](#how-to-run)
- [Running the Generator](#running-the-generator)
- [Running the Sorter](#running-the-sorter)
- [Testing](#testing)
- [License](#license)

---

## Requirements

- [.NET 6 or higher](https://dotnet.microsoft.com/en-us/download/dotnet)
- Sufficient disk space for the generated file (can be very large).
- Enough memory to handle chunk-based sorting (configurable in code).

---

## How It Works

### Generator Overview

- **FileGenerator.cs**:  
- Uses multiple threads to generate random lines in the format "`[number]. [string]`".
- Writes them to disk until the specified file size threshold is reached or exceeded.
- Ensures some lines share the same string so that duplicates are tested.

**Key Implementation Points**:
1. **Multi-threading**: Leverages parallel tasks to generate content more quickly.
2. **Locking**: Synchronizes write operations to avoid corrupted output.
3. **Approximate Size**: The file’s exact size can slightly exceed the target MB, or in some configurations, can be slightly under (depending on cancellation logic).

### Sorter Overview

- **FileSorter.cs**:  
- Splits the large input file into sorted chunks (in-memory sort).  
- Uses a custom comparer (in `PriorityQueueComparer.cs` or within the same file) to compare lines by string first, then by number.  
- Merges sorted chunks using an external merge approach with a priority queue.

**Key Implementation Points**:
1. **Chunking**: Reads a block of lines (configurable chunk size), sorts them, writes them to a temporary file.  
2. **External Merge**: Uses a min-heap (priority queue) to merge the chunk files.  
3. **Cleanup**: Deletes temporary chunk files after the final sorted output is written.

---

## How to Build

1. **Clone the Repository**:
```bash
git clone https://github.com/YourUser/SorterSolution.git
cd SorterSolution
