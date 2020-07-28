using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class GenerateGitHubOAuthToken
    {
        readonly static string _clientSecret = Environment.GetEnvironmentVariable("ReadmeRetrieverClientSecret") ?? string.Empty;
        readonly static string _clientId = Environment.GetEnvironmentVariable("ReadmeRetrieverClientId") ?? string.Empty;

        readonly GitHubAuthService _gitHubAuthService;

        public GenerateGitHubOAuthToken(GitHubAuthService gitHubAuthService) => _gitHubAuthService = gitHubAuthService;

        [FunctionName(nameof(GenerateGitHubOAuthToken))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest httpRequest, ILogger log)
        {
            log.LogInformation("Received request for OAuth Token");

            using var reader = new StreamReader(httpRequest.Body);
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            var generateTokenDTO = JsonConvert.DeserializeObject<GenerateTokenModel>(body);

            var token = await _gitHubAuthService.GetGitHubToken(_clientId, _clientSecret, generateTokenDTO.LoginCode, generateTokenDTO.State).ConfigureAwait(false);

            log.LogInformation("Token Retrieved");

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(token),
                StatusCode = (int)HttpStatusCode.OK,
                ContentType = "application/json"
            };
        }
    }
}
