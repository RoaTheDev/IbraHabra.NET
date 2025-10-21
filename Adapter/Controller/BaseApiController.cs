using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using Microsoft.AspNetCore.Mvc;

namespace IbraHabra.NET.Adapter.Controller;

public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromApiResult<T>(ApiResult<T> result)
    {
        var apiRes = ApiResponseBuilder.Build(HttpContext, result);
        return result.IsSuccess
            ? Ok(apiRes)
            : StatusCode((int)result.Error!.Type, apiRes);
    }
    protected IActionResult FromApiResult(ApiResult result)
    {
        var apiRes = ApiResponseBuilder.Build(HttpContext, result);
        return result.IsSuccess
            ? Ok(apiRes)
            : StatusCode((int)result.Error!.Type, apiRes);
    }
    protected IActionResult FromCreatedResult<T>(
        ApiResult<T> result,
        string actionName,
        object routeValues)
    {
        var apiRes = ApiResponseBuilder.Build(HttpContext, result);
        return result.IsSuccess
            ? CreatedAtAction(actionName, routeValues, apiRes)
            : StatusCode((int)result.Error!.Type, apiRes);
    }
}