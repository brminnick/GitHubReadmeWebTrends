using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public class YamlService
    {
        readonly static IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

        public CloudAdvocateYamlModel? ParseCloudAdvocateModelFromYaml(in string file, in ILogger logger)
        {
            try
            {
                var stringReaderFile = new StringReader(file);
                return _yamlDeserializer.Deserialize<CloudAdvocateYamlModel>(stringReaderFile);
            }
            catch (YamlException e)
            {
                logger.LogError(e, $"Invalid YAML File Found\n{file}");
                return null;
            }
        }

        public CloudAdvocateGitHubUserModel? ParseCloudAdvocateGitHubUserModelFromYaml(in string file, in ILogger logger)
        {
            const string gitHubDomain = "github.com/";

            try
            {
                var cloudAdvocate = ParseCloudAdvocateModelFromYaml(file, logger);
                if (cloudAdvocate is null)
                    return null;

                var fullName = cloudAdvocate.Name;

                var gitHubUrl = cloudAdvocate.Connect.FirstOrDefault(x => x.Title.Contains("GitHub", StringComparison.OrdinalIgnoreCase))?.Url;

                if (gitHubUrl is null || string.IsNullOrWhiteSpace(cloudAdvocate.Metadata.Alias))
                    return null;

                var gitHubUserName = parseGitHubUserNameFromUrl(gitHubUrl.ToString());

                return new CloudAdvocateGitHubUserModel(fullName, gitHubUserName, cloudAdvocate.Metadata.Alias, cloudAdvocate.Metadata.Team);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unknown Error, {e}\n{file}");
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
