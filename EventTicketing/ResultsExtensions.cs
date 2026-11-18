using EventTicketing.Domain;

namespace EventTicketing;

public static class ResultsExtensions
{
    public static IResult ToProblem(this Result result)
    {
        var error = result.Error
            ?? throw new InvalidOperationException("Result is not an error");

        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(statusCode: statusCode, title: error.Code, detail: error.Description);
    }
}