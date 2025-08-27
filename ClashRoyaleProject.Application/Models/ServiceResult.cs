namespace ClashRoyaleProject.Application.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;


        public static ServiceResult Successful(string message = "") => new ServiceResult { Success = true, Message = message };
        public static ServiceResult Failure(string message) => new ServiceResult { Success = false, Message = message };
    }
}
