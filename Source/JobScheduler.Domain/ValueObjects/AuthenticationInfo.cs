namespace JobScheduler.Domain.ValueObjects
{
    public class AuthenticationInfo
    {
        public string? Token { get; private set; }
        public string? ApiKey { get; private set; }
        public string? Username { get; private set; }
        public string? Password { get; private set; }

        protected AuthenticationInfo()
        {
        }

        public AuthenticationInfo(string? token = null, string? apiKey = null, string? username = null, string? password = null)
        {
            Token = token;
            ApiKey = apiKey;
            Username = username;
            Password = password;
        }

        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(Token) &&
            string.IsNullOrWhiteSpace(ApiKey) &&
            string.IsNullOrWhiteSpace(Username) &&
            string.IsNullOrWhiteSpace(Password);

        public static AuthenticationInfo None => new AuthenticationInfo();

        public AuthenticationInfo WithToken(string token) =>
            new AuthenticationInfo(token, ApiKey, Username, Password);

        public AuthenticationInfo WithApiKey(string apiKey) =>
            new AuthenticationInfo(Token, apiKey, Username, Password);

        public AuthenticationInfo WithUserPass(string username, string password) =>
            new AuthenticationInfo(Token, ApiKey, username, password);
    }
}