namespace JiraWorklogSubmitter.Data
{
    /// <summary>
    /// Used to store the appsettings values
    /// </summary>
    public class JiraSettings
    {
        private string _baseUrl;
        private string _email;
        private string _token;
        private string _apiUrl;

        /// <summary>
        /// The base url of the companies atlassian account.
        /// Ex: https://COMPANY.atlassian.net
        /// </summary>
        public string BaseUrl
        {
            get => _baseUrl;
            set => _baseUrl = value?.Trim();
        }

        /// <summary>
        /// Meant to be used after <see cref="BaseUrl"/>
        /// </summary>
        public string ApiUrl
        {
            get => _apiUrl;
            set => _apiUrl = value?.Trim();
        }

        /// <summary>
        /// The email of the user to sign in as
        /// </summary>
        public string Email
        {
            get => _email;
            set => _email = value?.Trim();
        }

        /// <summary>
        /// The API token to use
        /// </summary>
        /// <remarks>
        /// Learn how to setup a token here: https://confluence.atlassian.com/cloud/api-tokens-938839638.html
        /// </remarks>
        public string Token
        {
            get => _token;
            set => _token = value?.Trim();
        }
    }
}
