using System;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VerifyGitHubReadmeLinks
{
    class YamlService
    {
        readonly static IDeserializer _yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        public CloudAdvocateGitHubUserModel? ParseAdvocateFromYaml(in string file)
        {
            const string gitHubDomain = "github.com/";

            try
            {

                var stringReaderFile = new StringReader(file);
                var cloudAdvocate = _yamlDeserializer.Deserialize<CloudAdvocateYamlModel>(stringReaderFile);

                var fullName = cloudAdvocate.Name;

                var gitHubUrl = cloudAdvocate.Connect.FirstOrDefault(x => x.Title.Contains("GitHub", StringComparison.OrdinalIgnoreCase))?.Url;
                if (gitHubUrl is null)
                    return null;

                var gitHubUserName = parseGitHubUserNameFromUrl(gitHubUrl.ToString());

                return new CloudAdvocateGitHubUserModel(fullName, gitHubUserName, cloudAdvocate.Alias);
            }
            catch (YamlException)
            {
                return null;
            }

            static string parseGitHubUserNameFromUrl(in string gitHubUrl)
            {
                var indexOfGitHubDomain = gitHubUrl.LastIndexOf(gitHubDomain);
                var indexOfGitHubUserName = indexOfGitHubDomain + gitHubDomain.Length;

                return gitHubUrl.Substring(indexOfGitHubUserName).Trim('/');
            }
        }
    }
}
