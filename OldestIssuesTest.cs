using System.Text;
using System.Text.Json;

namespace PlaywrightTestTask
{
    public class OldestIssuesTests
    {
        private const string GitHubUrl = "https://api.github.com/graphql";
        private const string accessToken = "___"; // insert your access token here

        [Test]
        public async Task GetOldestOpenIssues_ShouldReturn10Results()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = new
            {
                query = @"{
                  repository(owner: ""nodejs"", name: ""node"") {
                    issues(first: 10, orderBy: {field: CREATED_AT, direction: ASC}, states: OPEN) {
                      edges {
                        node {
                          title
                          createdAt
                          url
                        }
                      }
                    }
                  }
                }"
            };

            var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(response.IsSuccessStatusCode, "Failed to fetch data from GitHub API");
            Assert.IsTrue(responseBody.Contains("issues"), "Response does not contain issues field");

            //Console.WriteLine(responseBody);
        }

        [Test]
        public async Task GetOldestOpenIssues_ShouldContainValidData()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = @"{
                repository(owner: ""nodejs"", name: ""node"") {
                  issues(first: 10, orderBy: {field: CREATED_AT, direction: ASC}, states: OPEN) {
                    edges {
                      node { title createdAt url }
                    }
                  }
                }
              }";

            var content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            var issues = parsedJson.GetProperty("data").GetProperty("repository").GetProperty("issues").GetProperty("edges");

            foreach (var issue in issues.EnumerateArray())
            {
                var node = issue.GetProperty("node");
                Assert.IsNotEmpty(node.GetProperty("title").GetString(), "Issue title should not be empty.");
                Assert.IsNotEmpty(node.GetProperty("url").GetString(), "Issue URL should not be empty.");
            }
        }

        [Test]
        public async Task GetOldestOpenIssues_ShouldBeSortedByOldestFirst()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = @"{
                repository(owner: ""nodejs"", name: ""node"") {
                  issues(first: 10, orderBy: {field: CREATED_AT, direction: ASC}, states: OPEN) {
                    edges {
                      node { title createdAt url }
                    }
                  }
                }
              }";

            var content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            var issues = parsedJson.GetProperty("data").GetProperty("repository").GetProperty("issues").GetProperty("edges");

            List<DateTime> issueDates = new List<DateTime>();

            foreach (var issue in issues.EnumerateArray())
            {
                var createdAt = issue.GetProperty("node").GetProperty("createdAt").GetString();
                issueDates.Add(DateTime.Parse(createdAt));
            }

            for (int i = 1; i < issueDates.Count; i++)
            {
                Assert.LessOrEqual(issueDates[i - 1], issueDates[i], "Issues should be sorted in ascending order.");
            }
        }

        [Test]
        public async Task GetOldestOpenIssues_AllIssuesShouldBeOpen()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = @"{
                repository(owner: ""nodejs"", name: ""node"") {
                  issues(first: 10, orderBy: {field: CREATED_AT, direction: ASC}, states: OPEN) {
                    edges {
                      node { title createdAt url }
                    }
                  }
                }
              }";

            var content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            var issues = parsedJson.GetProperty("data").GetProperty("repository").GetProperty("issues").GetProperty("edges");

            foreach (var issue in issues.EnumerateArray())
            {
                var issueTitle = issue.GetProperty("node").GetProperty("title").GetString();
                Assert.IsNotEmpty(issueTitle, "Each issue should have a valid title.");
            }
        }
    }
}
