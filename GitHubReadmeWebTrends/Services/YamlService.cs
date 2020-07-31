using System;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VerifyGitHubReadmeLinks
{
    class YamlService
    {
        readonly static IDeserializer _yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        public GitHubUserModel? ParseAdvocateFromYaml(in string file)
        {
            const string gitHubDomain = "github.com/";

            var stringReaderFile = new StringReader(file);
            var cloudAdvocate = _yamlDeserializer.Deserialize<CloudAdvocateYamlModel>(stringReaderFile);

            var fullName = cloudAdvocate.Name;

            var gitHubUrl = cloudAdvocate.Connect.First(x => x.Title.Contains("GitHub", StringComparison.OrdinalIgnoreCase)).Url;
            if (gitHubUrl is null)
                return null;

            var gitHubUserName = parseGitHubUserNameFromUrl(gitHubUrl.ToString());

            return new GitHubUserModel(fullName, gitHubUserName);

            static string parseGitHubUserNameFromUrl(in string gitHubUrl)
            {
                var indexOfGitHubDomain = gitHubUrl.LastIndexOf(gitHubDomain);
                var indexOfGitHubUserName = indexOfGitHubDomain + gitHubDomain.Length;

                return gitHubUrl.Substring(indexOfGitHubUserName).Trim('/');
            }
        }
    }
}
