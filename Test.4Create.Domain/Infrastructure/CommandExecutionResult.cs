namespace Test._4Create.Domain.Infrastructure;

public class CommandExecutionResult
{
    public bool Success { get; set; }
    public IEnumerable<Error> Errors { get; set; }
        
    public static CommandExecutionResult Ok() => new() { Success = true };

    public static CommandExecutionResult WithError(int errorCode, string error) =>
        WithErrors(new KeyValuePair<int, string>(errorCode, error));

    public static CommandExecutionResult WithErrors(params KeyValuePair<int, string>[] errors)
    {
        return new()
        {
            Success = false,
            Errors = errors.ToList().Select(x => new Error { Code = x.Key, Message = x.Value })
        };
    }
}