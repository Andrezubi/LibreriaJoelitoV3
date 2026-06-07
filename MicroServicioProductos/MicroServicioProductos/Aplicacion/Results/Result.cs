namespace MicroServicioProductos.Aplicacion.Results
{
    public class ErrorValidacion
    {
        public string Campo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

        public ErrorValidacion()
        {
        }

        public ErrorValidacion(string campo, string mensaje)
        {
            Campo = campo;
            Mensaje = mensaje;
        }
    }

    //public class Result
    //{
    //    public bool IsSuccess { get; }
    //    public bool IsFailure => !IsSuccess;
    //    public List<string> Errors { get; }

    //    protected Result(bool isSuccess, List<string> errors)
    //    {
    //        IsSuccess = isSuccess;
    //        Errors = errors;
    //    }

    //    public static Result Success()
    //    {
    //        return new Result(true, new List<string>()); ;
    //    }

    //    public static Result Failure(List<string> errors)
    //    {
    //        return new Result(false, errors);
    //    }

    //    public static Result Failure(string error)
    //    {
    //        return new Result(false, new List<string> { error });
    //    }
    //}

    //public class Result<T> : Result
    //{
    //    public T Value { get; private set; }

    //    private Result(bool isSuccess, List<string> errors, T value) : base(isSuccess, errors)
    //    {
    //        Value = value;
    //    }

    //    public static Result<T> Success(T value)
    //    {
    //        return new Result<T>(true, new List<string>(), value);
    //    }

    //    public static new Result<T> Failure(List<string> errors)
    //    {
    //        return new Result<T>(false, errors, default);
    //    }

    //    public static new Result<T> Failure(string error)
    //    {
    //        return new Result<T>(false, new List<string> { error }, default);
    //    }
    //}

    public class Result
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public List<ErrorValidacion> Errors { get; }

        protected Result(bool isSuccess, List<ErrorValidacion> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success()
        {
            return new Result(true, new List<ErrorValidacion>());
        }

        public static Result Failure(List<ErrorValidacion> errors)
        {
            return new Result(false, errors);
        }

        public static Result Failure(string campo, string mensaje)
        {
            return new Result(
                false,
                new List<ErrorValidacion>
                {
                    new ErrorValidacion(campo, mensaje)
                });
        }

        public static Result Failure(string error)
        {
            return new Result(
                false,
                new List<ErrorValidacion>
                {
            new ErrorValidacion("", error)
                });
        }

        public static Result Failure(List<string> errors)
        {
            return new Result(
                false,
                errors.Select(x =>
                    new ErrorValidacion("", x))
                .ToList());
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; private set; }

        private Result(
            bool isSuccess,
            List<ErrorValidacion> errors,
            T value)
            : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(
                true,
                new List<ErrorValidacion>(),
                value);
        }

        public static new Result<T> Failure(
            List<ErrorValidacion> errors)
        {
            return new Result<T>(
                false,
                errors,
                default);
        }

        public static Result<T> Failure(
            string campo,
            string mensaje)
        {
            return new Result<T>(
                false,
                new List<ErrorValidacion>
                {
                    new ErrorValidacion(campo, mensaje)
                },
                default);
        }

        public static new Result<T> Failure(string error)
        {
            return new Result<T>(
                false,
                new List<ErrorValidacion>
                {
            new ErrorValidacion("", error)
                },
                default);
        }

        public static new Result<T> Failure(List<string> errors)
        {
            return new Result<T>(
                false,
                errors.Select(x =>
                    new ErrorValidacion("", x))
                .ToList(),
                default);
        }
    }
}
