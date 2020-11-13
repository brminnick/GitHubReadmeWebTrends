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
- Edit `team` in your [YAML File](https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates)
- E.g. [Brandon Minnick's team is `mobile`](https://github.com/MicrosoftDocs/cloud-developer-advocates/blob/ade2e92a8edf3f017ec1bfbde077137a49328297/advocates/brandon-minnick.yml#L8)

The tool is using the wrong GitHub Username - How do I fix it?
- Edit the GitHub `url` in your [YAML File](https://github.com/MicrosoftDocs/cloud-developer-advocates/tree/live/advocates)
- E.g. [Brandon Minnick's GitHub URL is `https://github.com/brminnick`](https://github.com/MicrosoftDocs/cloud-developer-advocates/blob/ade2e92a8edf3f017ec1bfbde077137a49328297/advocates/brandon-minnick.yml#L39)
