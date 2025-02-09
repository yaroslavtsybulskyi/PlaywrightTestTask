using System.Text;
using System.Text.Json;

namespace PlaywrightTestTask
{
    public class MostStarredPackagesTests
    {
        private const string GitHubUrl = "https://api.github.com/graphql";
        private const string accessToken = ""; // Insert your access token here
        
        [Test]
        public async Task GetMostStarredPackages_ShouldReturn20Results()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = new
            {
                query = @"
                {
                  search(query: ""stars:>10000"", type: REPOSITORY, first: 20) {
                    edges {
                      node {
                        ... on Repository {
                          name
                          owner {
                            login
                          }
                          stargazers {
                            totalCount
                          }
                        }
                      }
                    }
                  }
                }"
            };

            var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(response.IsSuccessStatusCode, $"Failed to fetch data from GitHub API: {response.StatusCode}");
            Assert.IsTrue(responseBody.Contains("stargazers"), "Response does not contain stargazers field");

            //Console.WriteLine(responseBody);
        }

        [Test]
        public async Task GetMostStarredPackages_ValidateRepositoryCount()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = new
            {
                query = @"{
                  search(query: ""stars:>10000 sort:stars-desc"", type: REPOSITORY, first: 20) {
                    repositoryCount
                  }
                }"
            };

            var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            int repoCount = parsedJson.GetProperty("data").GetProperty("search").GetProperty("repositoryCount").GetInt32();

            Assert.GreaterOrEqual(repoCount, 20, "There should be at least 20 repositories in response.");
        }

        [Test]
        public async Task GetMostStarredPackages_RepositoriesShouldContainRequiredFields()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = @"{
                search(query: ""stars:>10000 sort:stars-desc"", type: REPOSITORY, first: 20) {
                        edges {
                        node {
                      ... on Repository {
                        name
                        owner { login }
                        stargazers { totalCount }
                      }
                    }
                  }
                }
              }";

            var content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            var repositories = parsedJson.GetProperty("data").GetProperty("search").GetProperty("edges");

            foreach (var repo in repositories.EnumerateArray())
            {
                var node = repo.GetProperty("node");
                Assert.IsNotEmpty(node.GetProperty("name").GetString(), "Repository name should not be empty.");
                Assert.IsNotEmpty(node.GetProperty("owner").GetProperty("login").GetString(), "Repository owner should not be empty.");
                Assert.Greater(node.GetProperty("stargazers").GetProperty("totalCount").GetInt32(), 0, "Repository should have some stars.");
            }
        }

        [Test]
        public async Task GetMostStarredPackages_RepositoriesShouldHaveMinimumStars()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PlaywrightTest");

            var query = @"{
                search(query: ""stars:>10000 sort:stars-desc"", type: REPOSITORY, first: 20) {
                  edges {
                    node {
                      ... on Repository {
                        stargazers { totalCount }
                      }
                    }
                  }
                }
              }";

            var content = new StringContent(JsonSerializer.Serialize(new { query }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GitHubUrl, content);
            var json = await response.Content.ReadAsStringAsync();

            //Console.WriteLine("API Response: " + json);

            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            var repositories = parsedJson.GetProperty("data").GetProperty("search").GetProperty("edges");

            foreach (var repo in repositories.EnumerateArray())
            {
                Assert.Greater(repo.GetProperty("node").GetProperty("stargazers").GetProperty("totalCount").GetInt32(), 10000, "Repository should have at least 10,000 stars.");
            }
        }
    }
}
