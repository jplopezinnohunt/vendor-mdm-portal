using Microsoft.AspNetCore.Mvc;

namespace VendorMdm.Core.Framework.Primitives;

/// <summary>
/// Extension methods for Result types to simplify controller response handling.
/// Implements error handling patterns from moderngoldenrules.md Section 4 (result-pattern-standard).
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IActionResult.
    /// Success: Returns 200 OK or custom success result.
    /// Failure: Returns 400 BadRequest with error message.
    /// </summary>
    public static IActionResult ToActionResult(this Result result, Func<IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke() ?? new OkResult();
        }

        return new BadRequestObjectResult(new { error = result.Error });
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to an IActionResult.
    /// Success: Returns 200 OK with value, or applies custom transform.
    /// Failure: Returns 400 BadRequest with error message.
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            if (onSuccess != null)
            {
                return onSuccess(result.Value);
            }
            return new OkObjectResult(result.Value);
        }

        return new BadRequestObjectResult(new { error = result.Error });
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to an IActionResult with a specific DTO transform.
    /// Success: Returns 200 OK with transformed DTO.
    /// Failure: Returns 400 BadRequest with error message.
    /// </summary>
    public static IActionResult ToActionResult<T, TDto>(this Result<T> result, Func<T, TDto> transform)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(transform(result.Value));
        }

        return new BadRequestObjectResult(new { error = result.Error });
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a CreatedAtAction result for POST endpoints.
    /// Success: Returns 201 Created with location header.
    /// Failure: Returns 400 BadRequest with error message.
    /// </summary>
    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        Func<T, object> routeValuesFactory,
        Func<T, object>? dtoTransform = null)
    {
        if (result.IsSuccess)
        {
            var value = dtoTransform != null ? dtoTransform(result.Value) : result.Value;
            return new CreatedAtActionResult(actionName, null, routeValuesFactory(result.Value), value);
        }

        return new BadRequestObjectResult(new { error = result.Error });
    }

    /// <summary>
    /// Converts a Result to a NotFound result for missing entities.
    /// Success: Returns custom success result.
    /// Failure: Returns 404 NotFound with error message.
    /// </summary>
    public static IActionResult ToNotFoundResult(this Result result, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess();
        }

        return new NotFoundObjectResult(new { error = result.Error });
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to a NotFound result for missing entities.
    /// Success: Returns 200 OK with value.
    /// Failure: Returns 404 NotFound with error message.
    /// </summary>
    public static IActionResult ToNotFoundResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            if (onSuccess != null)
            {
                return onSuccess(result.Value);
            }
            return new OkObjectResult(result.Value);
        }

        return new NotFoundObjectResult(new { error = result.Error });
    }

    /// <summary>
    /// Pattern matching for Result types.
    /// </summary>
    public static TResult Match<TResult>(
        this Result result,
        Func<TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }

    /// <summary>
    /// Pattern matching for Result&lt;T&gt; types.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }

    /// <summary>
    /// Combines multiple Results into a single Result.
    /// Returns Fail if any result failed, Ok otherwise.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        if (failures.Any())
        {
            var errors = string.Join("; ", failures.Select(f => f.Error));
            return Result.Fail(errors);
        }
        return Result.Ok();
    }

    /// <summary>
    /// Maps a Result&lt;T&gt; to a Result&lt;TNew&gt; using a transform function.
    /// </summary>
    public static Result<TNew> Map<T, TNew>(this Result<T> result, Func<T, TNew> map)
    {
        return result.IsSuccess
            ? Result.Ok(map(result.Value))
            : Result.Fail<TNew>(result.Error);
    }

    /// <summary>
    /// Binds a Result&lt;T&gt; to another Result&lt;TNew&gt; using a function that returns a Result.
    /// </summary>
    public static Result<TNew> Bind<T, TNew>(this Result<T> result, Func<T, Result<TNew>> bind)
    {
        return result.IsSuccess
            ? bind(result.Value)
            : Result.Fail<TNew>(result.Error);
    }

    /// <summary>
    /// Executes an action if the Result is successful.
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
        {
            action();
        }
        return result;
    }

    /// <summary>
    /// Executes an action with the value if the Result&lt;T&gt; is successful.
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }
        return result;
    }

    /// <summary>
    /// Executes an action if the Result failed.
    /// </summary>
    public static Result OnFailure(this Result result, Action<string> action)
    {
        if (result.IsFailure)
        {
            action(result.Error);
        }
        return result;
    }
}
