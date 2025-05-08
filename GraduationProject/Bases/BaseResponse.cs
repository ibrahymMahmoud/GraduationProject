namespace GraduationProject.Bases;

public class BaseResponse<T>
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string>? Errors { get; init; }
    public T? Data { get; init; }

    public BaseResponse()
    {
    }

    public BaseResponse(T data, string message = "Success")
    {
        Message = message;
        Succeeded = true;
        Data = data;
    }

    public BaseResponse(string message)
    {
        Message = message;
        Succeeded = false;
    }

    public BaseResponse(List<string> errors, string message = "Failure")
    {
        Succeeded = false;
        Message = message;
        Errors = errors;
    }
}