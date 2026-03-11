using Tianai.Captcha.Core.Common;

namespace Tianai.Captcha.Core.Tests.Common;

public class ApiResponseTests
{
    [Fact]
    public void OfSuccess_HasSuccessCode()
    {
        var resp = ApiResponse<string>.OfSuccess();
        Assert.True(resp.IsSuccess());
        Assert.Equal(200, resp.Code);
    }

    [Fact]
    public void OfSuccess_WithData_ContainsData()
    {
        var resp = ApiResponse<string>.OfSuccess("test");
        Assert.True(resp.IsSuccess());
        Assert.Equal("test", resp.Data);
    }

    [Fact]
    public void OfMessage_HasCorrectCodeAndMessage()
    {
        var resp = ApiResponse<object>.OfMessage(ApiResponseStatusConstant.BasicCheckFail);
        Assert.False(resp.IsSuccess());
        Assert.Equal(4001, resp.Code);
        Assert.Equal("Basic check failed", resp.Msg);
    }

    [Fact]
    public void OfError_HasInternalServerErrorCode()
    {
        var resp = ApiResponse<object>.OfError("something went wrong");
        Assert.Equal(500, resp.Code);
        Assert.Equal("something went wrong", resp.Msg);
    }

    [Fact]
    public void Convert_ChangesType_KeepsCodeAndMessage()
    {
        var resp = ApiResponse<string>.OfSuccess("hello");
        var converted = resp.Convert<int>();
        Assert.Equal(200, converted.Code);
        Assert.Equal(default, converted.Data);
    }
}
