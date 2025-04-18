namespace GithubExplorer.Models
{
    public class ReleaseDiff
    {
        public string Url { get; set; }
        public int AheadBy { get; set; }
        public int BehindBy { get; set; }
        public int TotalCommits { get; set; }
        public List<Commit> Commits { get; set; }
        public List<FileChange> Files { get; set; }
    }

    public class Commit
    {
        public string Sha { get; set; }
        public string Message { get; set; }
        public Author Author { get; set; }
    }

    public class Author
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
    }

    public class FileChange
    {
        public string Filename { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public int Changes { get; set; }
        public string Status { get; set; }
        public string Patch { get; set; }
    }
}
