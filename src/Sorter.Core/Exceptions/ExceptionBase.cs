namespace Sorter.Library.Exceptions
{
    /// <summary>
    /// Base class for all exceptions in the application.
    /// </summary>
    public abstract class ExceptionBase : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase"/> class.
        /// </summary>
        protected ExceptionBase()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the
        /// exception.</param>
        protected ExceptionBase(string message) : base(message)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.
        /// </param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        protected ExceptionBase(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
