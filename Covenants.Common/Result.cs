namespace Covenants.Common
{
    public class Result<T>
    {
        public bool Success { get; private set; }
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }

        private Result() { }

        public static Result<T> Ok(T data)
        {
            return new Result<T> { Success = true, Data = data };
        }

        public static Result<T> Fail(string errorMessage)
        {
            return new Result<T> { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class Result
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }

        private Result() { }

        public static Result Ok()
        {
            return new Result { Success = true };
        }

        public static Result Fail(string errorMessage)
        {
            return new Result { Success = false, ErrorMessage = errorMessage };
        }
    }
}
