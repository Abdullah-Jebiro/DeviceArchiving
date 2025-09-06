using DeviceArchiving.Data.Dto;
using System.Text.Json;

public class DbExceptionMiddleware(RequestDelegate next, ILogger<DbExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred during request execution.");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            string message =  ex.Message;

            var response = BaseResponse<object>.Failure(message);
            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }

}
