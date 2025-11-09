namespace Framework.ApiResponse
{
    public class Error(string message, string code = "General")
    {
        public string Code { get; set; } = code;
        public string Message { get; set; } = message;
    }
}