namespace GithubExplorer.Models
{
    public class Repository
    {
        public string Name { get; set; }
        public Release LatestRelease { get; set; }
        public Tag LatestTag { get; set; }
        public List<Workflow> Workflows { get; set; }
    }
}
