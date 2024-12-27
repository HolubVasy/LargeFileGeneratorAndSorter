namespace Sorter.Library.Extensions;

/// <summary>
/// Provides extension methods for working with strings (lines) in a specific format.
/// </summary>
public static class LineExtensions
{
    /// <summary>
    /// Parses a line in the format "[number]. [text]" into its numeric and text components.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>A tuple containing the numeric part and the text part of the line.</returns>
    /// <exception cref="FormatException">Thrown when the line is not in the expected format or
    /// contains an invalid number.</exception>
    public static (int number, string text) ParseLine(this string line)
    {
        // Simple parse logic:
        //   "[number]. [some text]"
        // Could handle edge cases more carefully.
        var dotIndex = line.IndexOf(". ", StringComparison.Ordinal);
        if (dotIndex < 1)
        {
            return (0, line); // fallback or throw
        }
        
        var numPart = line[..dotIndex];
        var textPart = line[(dotIndex + 2)..];
        
        return int.TryParse(numPart, out var number)
            ?(number, textPart)
            :(0, line);
    }
}