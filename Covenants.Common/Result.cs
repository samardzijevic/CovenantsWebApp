namespace Covenants.Common
{
    // -----------------------------------------------------------------------
    // RESULT PATTERN
    // -----------------------------------------------------------------------
    // Instead of throwing exceptions for expected failures (e.g. "covenant not
    // found", "validation error"), every service method returns a Result object.
    // The caller checks result.Success before using result.Data.
    //
    // Why not just throw exceptions?
    //   - Exceptions are expensive and meant for unexpected situations.
    //   - A result object makes it explicit that failure is a normal possibility.
    //   - The UI can display result.ErrorMessage directly to the user.
    //
    // There are two versions:
    //   Result<T>  — for operations that return data (e.g. Create returns the new Id)
    //   Result     — for operations that return nothing (e.g. Update, Delete)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Wraps the outcome of a service operation that returns a value of type T.
    /// Example: Result&lt;int&gt; from CovenantService.Create() carries the new covenant Id.
    /// </summary>
    public class Result<T>
    {
        // True when the operation completed without errors.
        public bool Success { get; private set; }

        // The returned value — only meaningful when Success == true.
        public T Data { get; private set; }

        // Human-readable reason for failure — only set when Success == false.
        public string ErrorMessage { get; private set; }

        // Private constructor forces callers to use Ok() or Fail() factory methods.
        // This prevents creating a Result in an invalid state (e.g. Success=true but no Data).
        private Result() { }

        /// <summary>Creates a successful result carrying the given data.</summary>
        public static Result<T> Ok(T data)
        {
            return new Result<T> { Success = true, Data = data };
        }

        /// <summary>Creates a failed result with an error message.</summary>
        public static Result<T> Fail(string errorMessage)
        {
            return new Result<T> { Success = false, ErrorMessage = errorMessage };
        }
    }

    /// <summary>
    /// Wraps the outcome of a service operation that returns no value.
    /// Example: Result from CovenantService.Update() — we only care if it succeeded.
    /// </summary>
    public class Result
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }

        private Result() { }

        /// <summary>Creates a successful result.</summary>
        public static Result Ok()
        {
            return new Result { Success = true };
        }

        /// <summary>Creates a failed result with an error message.</summary>
        public static Result Fail(string errorMessage)
        {
            return new Result { Success = false, ErrorMessage = errorMessage };
        }
    }
}
