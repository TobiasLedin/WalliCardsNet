namespace WalliCardsNet.API.Models
{
    public class ActionResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ActionResult<T> SuccessResult(T data, string message = "Action succeeded") =>
            new ActionResult<T> { Success = true, Message = message, Data = data };

        public static ActionResult<T> FailureResult(string message, List<string>? errors = null) =>
            new ActionResult<T> { Success = false, Message = message, Errors = errors ?? new List<string>() };
    }
}
