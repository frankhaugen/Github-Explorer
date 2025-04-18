using System.Diagnostics;
using System.Text.Json;
using GitHubExplorer.Models;
using GitHubExplorer.Models.Responses;

namespace GitHubExplorer.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubService> _logger;

        public string Owner { get; set; } = "frankhaugen"; // Set your GitHub username here
        public string RepositoryName { get; set; }

        public GitHubService(IHttpClientFactory httpClientFactory, ILogger<GitHubService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("GitHub");
            _logger = logger;
        }

        public async Task<List<Repository>> GetRepositoriesAsync()
        {
            // If Bearer token is not set, throw an exception
            if (_httpClient.DefaultRequestHeaders.Authorization == null || 
                string.IsNullOrEmpty(_httpClient.DefaultRequestHeaders.Authorization.Parameter))
            {
                Debugger.Break();
                throw new InvalidOperationException("Bearer token is not set.");
            }

            var response = await _httpClient.GetAsync("user/repos");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch repositories: {response.StatusCode}");
                return new List<Repository>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var repositoryRespnses = JsonSerializer.Deserialize<List<RepositoryRespnse>>(content);

            if (repositoryRespnses == null)
            {
                _logger.LogError("Failed to deserialize repository responses.");
                return new List<Repository>();
            }

            // Map the repository responses to the Repository model
            var repositories = repositoryRespnses.Select(repo => new Repository
            {
                Name = repo.Name,
                LatestRelease = new Release(),
                LatestTag = new Tag(),
                Workflows = new List<Workflow>()
            });

            foreach (var repo in repositories ?? Enumerable.Empty<Repository>())
            {
                // Fetch latest release, latest tag, and workflows for each repository
                repo.LatestRelease = await GetLatestReleaseAsync(repo.Name) ?? new Release();
                repo.LatestTag = await GetLatestTagAsync(repo.Name) ?? new Tag();
                repo.Workflows = await GetWorkflowsAsync(repo.Name);
            }

            return repositories?.ToList() ?? new List<Repository>();
        }

        public async Task<Release?> GetLatestReleaseAsync(string repoName)
        {
            var url = $"repos/{Owner}/{repoName}/releases/latest";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch latest release for {repoName}: {response.StatusCode}");
                return null; // No releases found
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Release>(content);
        }

        public async Task<Tag?> GetLatestTagAsync(string repoName)
        {
            var url = $"repos/{Owner}/{repoName}/tags";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch latest tag for {repoName}: {response.StatusCode}");
                return null; // No tags found
            }

            var content = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<Tag>>(content);
            return tags?.FirstOrDefault();
        }

        public async Task<List<Workflow>> GetWorkflowsAsync(string repoName)
        {
            var url = $"repos/{Owner}/{repoName}/actions/workflows";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch workflows for {repoName}: {response.StatusCode}");
                return new List<Workflow>();
            }
            var content = await response.Content.ReadAsStringAsync();
            var workflows = JsonSerializer.Deserialize<WorkflowsResponse>(content);
            return workflows?.Workflows ?? new List<Workflow>();
        }

        public async Task<ReleaseDiff?> GetReleaseDiffAsync(string repoName, string tag1, string tag2)
        {
            var url = $"repos/{Owner}/{repoName}/compare/{tag1}...{tag2}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch release diff for {repoName}: {response.StatusCode}");
                return null; // No releases found
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ReleaseDiff>(content);
        }
    }

    public class WorkflowsResponse
    {
        public List<Workflow> Workflows { get; set; }
    }
}
