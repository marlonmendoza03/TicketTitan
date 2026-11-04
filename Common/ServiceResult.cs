namespace TicketTitan.Common
{
    public readonly record struct ServiceResult<T>
    {
        public bool IsSuccess { get; }

        public T? Value { get; }
        public string? ErrorMessage { get; }

        private ServiceResult(bool isSuccess, T? value, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult<T> Success(T value) => new ServiceResult<T>(true, value, null);
        public static ServiceResult<T> Failure(string errorMessage) => new ServiceResult<T>(false, default, errorMessage);

        public static implicit operator ServiceResult<T>(T value) => Success(value);

        public void Deconstruct(out bool isSuccess, out T? value, out string? errorMessage)
        {
            isSuccess = IsSuccess;
            value = Value;
            errorMessage = ErrorMessage;
        }
    }
}
