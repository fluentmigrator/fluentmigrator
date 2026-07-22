using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var token = GetRequiredInput("github_token");
var repository = GetRequiredInput("repository");
var pullRequestNumber = int.Parse(GetRequiredInput("pull_request_number"));
var header = GetRequiredInput("header");
var bodyPath = GetRequiredInput("body_path");

if (!File.Exists(bodyPath))
{
    throw new FileNotFoundException($"Comment body file not found: {bodyPath}");
}

var marker = $"<!-- fluentmigrator-sticky:{header} -->";
var commentBody = $"{marker}{Environment.NewLine}{File.ReadAllText(bodyPath)}";

var repoParts = repository.Split('/');
if (repoParts.Length != 2 || string.IsNullOrWhiteSpace(repoParts[0]) || string.IsNullOrWhiteSpace(repoParts[1]))
{
    throw new InvalidOperationException($"Invalid repository format '{repository}'. Expected owner/name.");
}

var owner = repoParts[0];
var repo = repoParts[1];

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.github.com/"),
};
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("fluentmigrator-sticky-pr-comment-action");
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var existingCommentId = await FindExistingCommentIdAsync(httpClient, owner, repo, pullRequestNumber, marker);
var finalCommentId = await UpsertCommentAsync(httpClient, owner, repo, pullRequestNumber, existingCommentId, commentBody);

WriteOutput("comment_id", finalCommentId.ToString());

static string GetRequiredInput(string name)
{
    var value = Environment.GetEnvironmentVariable($"INPUT_{name.ToUpperInvariant()}");
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Missing required input: {name}");
    }

    return value;
}

static async Task<long?> FindExistingCommentIdAsync(HttpClient client, string owner, string repo, int pullRequestNumber, string marker)
{
    for (var page = 1; page <= 10; page++)
    {
        var response = await client.GetAsync($"repos/{owner}/{repo}/issues/{pullRequestNumber}/comments?per_page=100&page={page}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var comments = JsonSerializer.Deserialize<List<IssueComment>>(content) ?? new List<IssueComment>();

        var existing = comments.FirstOrDefault(c => c.Body?.Contains(marker, StringComparison.Ordinal) == true);
        if (existing is not null)
        {
            return existing.Id;
        }

        if (comments.Count < 100)
        {
            return null;
        }
    }

    return null;
}

static async Task<long> UpsertCommentAsync(HttpClient client, string owner, string repo, int pullRequestNumber, long? existingCommentId, string body)
{
    var payload = JsonSerializer.Serialize(new { body });
    using var content = new StringContent(payload, Encoding.UTF8, "application/json");

    HttpResponseMessage response;
    if (existingCommentId is long commentId)
    {
        response = await client.PatchAsync($"repos/{owner}/{repo}/issues/comments/{commentId}", content);
    }
    else
    {
        response = await client.PostAsync($"repos/{owner}/{repo}/issues/{pullRequestNumber}/comments", content);
    }

    response.EnsureSuccessStatusCode();

    var responseContent = await response.Content.ReadAsStringAsync();
    var updatedComment = JsonSerializer.Deserialize<IssueComment>(responseContent)
        ?? throw new InvalidOperationException("Unable to parse GitHub comment response.");

    return updatedComment.Id;
}

static void WriteOutput(string name, string value)
{
    var outputPath = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
    if (string.IsNullOrWhiteSpace(outputPath))
    {
        return;
    }

    File.AppendAllText(outputPath, $"{name}={value}{Environment.NewLine}");
}

internal sealed class IssueComment
{
    public long Id { get; set; }
    public string? Body { get; set; }
}
