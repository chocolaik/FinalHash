namespace kt.Models;

public class TrackGroup
{
	public string CanonicalFile { get; set; } = "";
	public List<string> ExactDuplicates { get; set; } = new();
	public List<string> SimilarVersions { get; set; } = new();
}
