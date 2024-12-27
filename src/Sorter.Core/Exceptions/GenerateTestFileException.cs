namespace Sorter.Library.Exceptions
{

/// <summary>
/// Represents an exception that occurs during test file generation.
/// </summary>
[Serializable]
public sealed class GenerateTestFileException : ExceptionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateTestFileException"/> class.
    /// </summary>
    public GenerateTestFileException()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateTestFileException"/> class with
    /// a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public GenerateTestFileException(string message)
        : base(message)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateTestFileException"/> class with
    /// a specified error message and a reference to the inner exception that is the cause
    /// of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public GenerateTestFileException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
}