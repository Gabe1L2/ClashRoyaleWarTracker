namespace ClashRoyaleProject.Application.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult Successful(string message = "") => new ServiceResult { Success = true, Message = message };
        public static ServiceResult Failure(string message) => new ServiceResult { Success = false, Message = message };
    }

    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ServiceResult<T> Successful(T data, string message = "") => new ServiceResult<T> { Success = true, Message = message, Data = data };
        public static ServiceResult<T> Failure(string message) => new ServiceResult<T> { Success = false, Message = message };
    }
}
