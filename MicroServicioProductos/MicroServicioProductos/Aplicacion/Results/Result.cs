namespace MicroServicioProductos.Aplicacion.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public List<string> Errors { get; }

        protected Result(bool isSuccess, List<string> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success()
        {
            return new Result(true, new List<string>()); ;
        }

        public static Result Failure(List<string> errors)
        {
            return new Result(false, errors);
        }

        public static Result Failure(string error)
        {
            return new Result(false, new List<string> { error });
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; private set; }

        private Result(bool isSuccess, List<string> errors, T value) : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, new List<string>(), value);
        }

        public static new Result<T> Failure(List<string> errors)
        {
            return new Result<T>(false, errors, default);
        }

        public static new Result<T> Failure(string error)
        {
            return new Result<T>(false, new List<string> { error }, default);
        }
    }
}
