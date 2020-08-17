using System;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace VerifyGitHubReadmeLinks
{
    class YamlService
    {
        readonly static IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

        public CloudAdvocateGitHubUserModel? ParseAdvocateFromYaml(in string file, in ILogger logger)
        {
            const string gitHubDomain = "github.com/";

            try
            {
                var stringReaderFile = new StringReader(file);
                var json = new SerializerBuilder().JsonCompatible().Build().Serialize(file);

                var cloudAdvocate = _yamlDeserializer.Deserialize<CloudAdvocateYamlModel>(stringReaderFile);

                var fullName = cloudAdvocate.Name;

                var gitHubUrl = cloudAdvocate.Connect.FirstOrDefault(x => x.Title.Contains("GitHub", StringComparison.OrdinalIgnoreCase))?.Url;

                if (gitHubUrl is null || string.IsNullOrWhiteSpace(cloudAdvocate.Metadata.Alias))
                    return null;

                var gitHubUserName = parseGitHubUserNameFromUrl(gitHubUrl.ToString());

                return new CloudAdvocateGitHubUserModel(fullName, gitHubUserName, cloudAdvocate.Metadata.Alias);
            }
            catch (YamlException e)
            {
                logger.LogError(e, $"Invalid YAML File Found\n{file}");
                return null;
            }
            catch(Exception e)
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
