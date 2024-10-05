1. Create a Draft copy of Release Notes.
    1. Use "vX.Y.Z" for tag name.
	2. Use "Version X.Y.Z" for release title.
    3. Note: It is important not to click Publish until the Release Notes are ready.  Drafts can only be seen by others within the GitHub organization.
	4. Note: GitHub will not create a tag for the release until we click Publish.
2. For each Pull Request merged,
    1. Update Draft copy of Release Notes in the following format:
        1. PR # : Summary (@contributor) [Fixes Issue # (@originalPosterOfIssue)]
	2. Azure DevOps Pipeline will listen for changes on the master branch.
	    1. Pipeline uses GitVersion to assign a version number, which is used by our Nuget feeds.
3. When ready to Release, go to the Draft copy of Release Notes, and click Publish.
    1. Note: This will automatically publish the Nuget packages to nuget.org, as well as create a GitHub Release.
