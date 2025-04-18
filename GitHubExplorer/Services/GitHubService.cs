using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using GitHubExplorer.Models;

namespace GitHubExplorer.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;
        public string Owner { get; set; }
        public string RepositoryName { get; set; }

        public GitHubService(HttpClient httpClient, string token)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<Repository>> GetRepositoriesAsync()
        {
            var response = await _httpClient.GetAsync("user/repos");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var repositories = JsonSerializer.Deserialize<List<Repository>>(content);

            foreach (var repo in repositories)
            {
                repo.LatestRelease = await GetLatestReleaseAsync(repo.Name);
                repo.LatestTag = await GetLatestTagAsync(repo.Name);
                repo.Workflows = await GetWorkflowsAsync(repo.Name);
            }

            return repositories;
        }

        public async Task<Release> GetLatestReleaseAsync(string repoName)
        {
            var response = await _httpClient.GetAsync($"repos/{Owner}/{repoName}/releases/latest");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Release>(content);
        }

        public async Task<Tag> GetLatestTagAsync(string repoName)
        {
            var response = await _httpClient.GetAsync($"repos/{Owner}/{repoName}/tags");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<Tag>>(content);
            return tags.FirstOrDefault();
        }

        public async Task<List<Workflow>> GetWorkflowsAsync(string repoName)
        {
            var response = await _httpClient.GetAsync($"repos/{Owner}/{repoName}/actions/workflows");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var workflows = JsonSerializer.Deserialize<WorkflowsResponse>(content);
            return workflows.Workflows;
        }

        public async Task<ReleaseDiff> GetReleaseDiffAsync(string repoName, string tag1, string tag2)
        {
            var response = await _httpClient.GetAsync($"repos/{Owner}/{repoName}/compare/{tag1}...{tag2}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ReleaseDiff>(content);
        }
    }

    public class WorkflowsResponse
    {
        public List<Workflow> Workflows { get; set; }
    }
}
