name: Update TargetFramework  
  
on:  
  schedule:  
    - cron: '0 0 * * 0' # Runs weekly  
  workflow_dispatch:  
  
jobs:  
  update-targetframework:  
    runs-on: ubuntu-latest  
    steps:  
      - name: Checkout repository  
        uses: actions/checkout@v2  
  
      - name: Get Latest .NET Version  
        id: get-latest-version  
        run: |  
          latest_version=$(curl -s https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json | jq -r '.releases-index[0].latest-release')  
          echo "Latest .NET Version: $latest_version"  
          echo "::set-output name=latest_version::$latest_version"  
  
      - name: Update TargetFramework  
        run: |  
          latest_version=${{ steps.get-latest-version.outputs.latest_version }}  
          echo "Updating TargetFramework to net$latest_version"  
          find . -name '*.csproj' -exec sed -i "s/<TargetFramework>net[0-9.]*<\/TargetFramework>/<TargetFramework>net$latest_version<\/TargetFramework>/g" {} +  
          git config --global user.name 'github-actions[bot]'  
          git config --global user.email 'github-actions[bot]@users.noreply.github.com'  
          git checkout -b update-targetframework-net$latest_version  
          git commit -am "Update TargetFramework to net$latest_version"  
          git push origin update-targetframework-net$latest_version  
  
      - name: Create Pull Request  
        uses: peter-evans/create-pull-request@v3  
        with:  
          branch: update-targetframework-net${{ steps.get-latest-version.outputs.latest_version }}  
          title: Update TargetFramework to net${{ steps.get-latest-version.outputs.latest_version }}  
          body: |  
            This pull request updates the TargetFramework to net${{ steps.get-latest-version.outputs.latest_version }}.  
