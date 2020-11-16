# GitHub Readme + WebTrends

To best understand which topics are popular and which subjects are trending in our developer communities, we can leverage [Web Trends](https://www.webtrends.com).

This is an automated tool created using [Azure Functions](https://docs.microsoft.com/azure/azure-functions/?WT.mc_id=mobile-0000-bramin) that double checks each Readme to ensure every repository is leveraging [Web Trends](https://www.webtrends.com).

[Web Trends](https://www.webtrends.com) does not collect any Personally Identifiable Information and cannot be traced back to any individual user.

## Architecture

![](https://user-images.githubusercontent.com/13558917/89959435-40dc4480-dbf1-11ea-8c30-a4811fe819e9.png)

## FAQ 

How do I opt-in to the tool? (Prerequisite: Only Microsoft employees are able to opt-in)
- Add a `yml` file to the [Cloud Developer Advocates GitHub repository](https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates) 
- This also adds you to the [Cloud Advacates Webpage](https://developer.microsoft.com/advocates/?WT.mc_id=mobile-0000-bramin)

How do I opt-out from the tool? 
- You may opt-out (or opt back in) any time at https://optout-githubreadmelinks.azurewebsites.net

The tool is using the wrong value for Area/Team - How do I fix it?
- In your [YAML File](https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates), edit `team` 
  - E.g. [Brandon Minnick's team is `mobile`](https://github.com/MicrosoftDocs/cloud-developer-advocates/blob/ade2e92a8edf3f017ec1bfbde077137a49328297/advocates/brandon-minnick.yml#L8)

The tool is using the wrong GitHub Username - How do I fix it?
- In your [YAML File](https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates), edit the GitHub `url`
  - E.g. [Brandon Minnick's GitHub URL is `https://github.com/brminnick`](https://github.com/MicrosoftDocs/cloud-developer-advocates/blob/ade2e92a8edf3f017ec1bfbde077137a49328297/advocates/brandon-minnick.yml#L39)
  
Can we extend this functionality beyond GitHub to other sites like Dev.to?
- Probably! As long as other sites have an API, we can likely expand this automated tool to update those resources as well!
- Feel free to open a feature request (ie open an Issue in this repo) and we'll chat about it!

How do I know if my GitHub Repo is still active?
- I've created an iOS + Android app to help with this, GitTrends: https://GitTrends.com
  - GitTrends shows you the number of Views & Clones that each of your GitHub repos are receiving, and it will send you a push notification when one of your GitHub Repos is trending!
  - [Download it on iOS](https://apps.apple.com/app/gittrends-github-insights/id1500300399)
  - [Download it on Android](https://play.google.com/store/apps/details?id=com.minnick.gittrends)
  
I received 30+ PRs this month. Will I receive 30+ PRs again next month?
- Nope! As long as you merge this month’s PRs, you will receive substantially less PRs next month

Why didn’t I get an email notifying me of the PR?
- GitHub only sends you a notification Email if you are “Watching” the repo
- Double check your repos to ensure that you are “Watching” it

Why do I still have PRs rolling in? I thought the tool was supposed to run once per month?
- This is mostly because of the [GitHub API Rate Limit](https://github.com/brminnick/GitHubApiStatus#github-api-rate-limits)
  - tl;dr The GitHub API only allows me to make 5,000 API Requests per Hour
  - Once the tool reaches the limit, I queue up the remaining work on a backlog that uses a Timer Trigger Function to run once per hour
- I published a NuGet package to help fellow .NET Devs work with and understand the GitHub API Rate Limits: https://github.com/brminnick/GitHubApiStatus
  - This NuGet package is being leveraged by this automated GitHub Readme + WebTrends tool 😄
