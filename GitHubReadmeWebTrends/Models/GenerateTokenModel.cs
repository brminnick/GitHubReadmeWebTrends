namespace VerifyGitHubReadmeLinks
{
    public class GenerateTokenModel
    {
        public GenerateTokenModel(string loginCode, string state) => (LoginCode, State) = (loginCode, state);

        public string LoginCode { get; }
        public string State { get; }
    }
}
