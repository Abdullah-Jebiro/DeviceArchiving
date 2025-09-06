namespace DeviceArchiving.Data.Dto;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static BaseResponse<T> SuccessResult(T data, string message = "")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = string.IsNullOrEmpty(message) ? data?.ToString() ?? string.Empty : message,
            Data = data
        };
    }

    public static BaseResponse<T> Failure(string message)
    {
        return new BaseResponse<T> { Success = false, Message = message, Data = default };
    }
}