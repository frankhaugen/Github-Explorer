namespace GitHubExplorer.Models
{
    public class Release
    {
        public string TagName { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public DateTime PublishedAt { get; set; }
        public Author Author { get; set; }
    }
}
