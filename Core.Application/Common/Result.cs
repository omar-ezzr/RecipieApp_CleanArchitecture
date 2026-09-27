namespace Core.Application.Common;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }

    public static Result Success()
    {
        return new Result { IsSuccess = true };
    }

    public static Result Failure(string error)
    {
        return new Result
        {
            IsSuccess = false,
            Error = error
        };
    }
}