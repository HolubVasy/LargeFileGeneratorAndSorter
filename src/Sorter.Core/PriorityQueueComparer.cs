using Sorter.Library.Extensions;

namespace Sorter.Library;

/// <summary>
/// A custom comparer for PriorityQueue that uses the same logic as CompareLines
/// to determine the 'priority' of each item.
/// </summary>
internal sealed class PriorityQueueComparer : IComparer<(string line, int fileIndex)>
{
    /// <summary>
    /// Compares two tuples containing a line and its file index, based on the line content.
    /// </summary>
    /// <param name="x">The first tuple to compare.</param>
    /// <param name="y">The second tuple to compare.</param>
    /// <returns>An integer indicating the relative order of the tuples based on the line content.</returns>
    public int Compare((string line, int fileIndex) x, (string line, int fileIndex) y)
    {
        return CompareLines(x.line, y.line);
    }
    
    /// <summary>
    /// Compares two lines in the format "[number]. [string part]", sorting by
    /// [string part] first, then [number].
    /// </summary>
    /// <param name="a">The first line to compare.</param>
    /// <param name="b">The second line to compare.</param>
    /// <returns>An integer indicating the relative order of the lines.</returns>
    internal int CompareLines(string a, string b)
    {
        // Use your extension method or inline parse logic
        var (numA, textA) = a.ParseLine();
        var (numB, textB) = b.ParseLine();
        
        // Compare by string part first (ascending), then by number (ascending).
        var stringComparison = string.Compare(textA, textB, StringComparison.Ordinal);
        return stringComparison != 0
            ?stringComparison
            :numA.CompareTo(numB);
        
    }
}