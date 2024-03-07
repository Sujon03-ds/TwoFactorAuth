namespace TwoFactorAuth.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProjectSection> Sections{ get; set; }
    }

    public class ProjectSection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public  Project Project { get; set; }
    }
}
