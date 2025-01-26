namespace Test._4Create.Domain.Infrastructure
{
    public class QueryExecutionResult<T>
    {
        public bool Success { get; set; }
        public IEnumerable<Error> Errors { get; set; }
        public T Data { get; set; }

        public static QueryExecutionResult<T> Ok(T data) => new() { Success = true, Data = data };

        public static QueryExecutionResult<T> WithError(int errorCode, string error) =>
            WithErrors(new KeyValuePair<int, string>(errorCode, error));
        
        public static QueryExecutionResult<T> WithErrors(params KeyValuePair<int, string>[] errors)
        {
            return new()
            {
                Success = false,
                Errors = errors.ToList().Select(x => new Error { Code = x.Key, Message = x.Value })
            };
        }
    }
}
