using API.Responses;
using Core.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ControllerResultExtensions
{
    public static IActionResult ToActionResult(
        this ControllerBase controller,
        ServiceResult result,
        string notFoundCode = "not_found",
        string notFoundMessage = "Resource was not found.")
    {
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => controller.NotFound(controller.ApiError(notFoundCode, notFoundMessage)),
            ServiceErrorType.Forbidden => controller.Forbid(),
            ServiceErrorType.Conflict => controller.Conflict(controller.ApiError("conflict", result.Error ?? "Conflict.")),
            ServiceErrorType.Validation => controller.BadRequest(controller.ApiError("validation_failed", result.Error ?? "The request is invalid.")),
            _ => controller.BadRequest(controller.ApiError("bad_request", result.Error ?? "The request is invalid."))
        };
    }

    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        ServiceResult<T> result,
        string notFoundCode = "not_found",
        string notFoundMessage = "Resource was not found.")
    {
        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => controller.NotFound(controller.ApiError(notFoundCode, notFoundMessage)),
            ServiceErrorType.Forbidden => controller.Forbid(),
            ServiceErrorType.Conflict => controller.Conflict(controller.ApiError("conflict", result.Error ?? "Conflict.")),
            ServiceErrorType.Validation => controller.BadRequest(controller.ApiError("validation_failed", result.Error ?? "The request is invalid.")),
            _ => controller.BadRequest(controller.ApiError("bad_request", result.Error ?? "The request is invalid."))
        };
    }

    public static ApiErrorResponse ApiError(
        this ControllerBase controller,
        string code,
        string message)
    {
        return new ApiErrorResponse
        {
            Code = code,
            Message = message,
            TraceId = controller.HttpContext.TraceIdentifier
        };
    }

    public static UnauthorizedObjectResult UnauthorizedIdentityProblem(this ControllerBase controller)
    {
        return controller.Unauthorized(controller.ApiError(
            "invalid_identity",
            "Missing or malformed user identity claim"));
    }
}
