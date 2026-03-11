namespace Tianai.Captcha.Core.Common;

public class CodeDefinition
{
    public int Code { get; }
    public string Message { get; }

    public CodeDefinition(int code, string message)
    {
        Code = code;
        Message = message;
    }
}

public static class ApiResponseStatusConstant
{
    public static readonly CodeDefinition Success = new(200, "OK");
    public static readonly CodeDefinition NotValidParam = new(403, "Invalid parameter");
    public static readonly CodeDefinition InternalServerError = new(500, "Internal server error");
    public static readonly CodeDefinition Expired = new(4000, "Expired");
    public static readonly CodeDefinition BasicCheckFail = new(4001, "Basic check failed");
}

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string? Msg { get; set; }
    public T? Data { get; set; }

    public ApiResponse()
    {
        Code = ApiResponseStatusConstant.Success.Code;
        Msg = ApiResponseStatusConstant.Success.Message;
    }

    public ApiResponse(int code, string? msg, T? data)
    {
        Code = code;
        Msg = msg;
        Data = data;
    }

    public ApiResponse(CodeDefinition definition, T? data)
    {
        Code = definition.Code;
        Msg = definition.Message;
        Data = data;
    }

    public bool IsSuccess() => Code == ApiResponseStatusConstant.Success.Code;

    public ApiResponse<TResult> Convert<TResult>()
    {
        return new ApiResponse<TResult>(Code, Msg, default);
    }

    public static ApiResponse<T> Of(int code, string msg, T? data) => new(code, msg, data);
    public static ApiResponse<T> Of(CodeDefinition definition, T? data) => new(definition, data);
    public static ApiResponse<T> OfMessage(CodeDefinition definition) => new(definition, default);
    public static ApiResponse<T> OfError(string message) => new(ApiResponseStatusConstant.InternalServerError.Code, message, default);
    public static ApiResponse<T> OfCheckError(string message) => new(ApiResponseStatusConstant.NotValidParam.Code, message, default);
    public static ApiResponse<T> OfSuccess(T? data) => new(ApiResponseStatusConstant.Success, data);
    public static ApiResponse<T> OfSuccess() => new();
}
