# How to contribute

Thanks for taking the time to submit a pull request. Here are a few tips to help you out. Try and break up your pull request in to several smaller commits rather than one big commit. Try and separate out refactoring and reformatting changes into their own commits. Unit tests and integration tests are always appreciated!

# Code Formatting

## Tabs

We use the Visual Studio default for C# code: 
* smart indentation
* tab size: 4
* indent size: 4
* insert spaces

We are aware that there is a mixture of tabs and spaces in the code, so feel free to reformat the code to this standard. But if you do so, please put it in a separate commit.

If you are making changes to the rake files (written in Ruby) then the default tab size is 2.

## Line Endings in Git

To avoid problems with line endings we set autocrlf to true in Git. Merging or rebasing code with lf endings is very unpleasant. To set the autocrlf option for your local FluentMigrator Git repository, cd (change directory) to the FluentMigrator folder and use the following command:

<pre><code>git config core.autocrlf true</code></pre>
