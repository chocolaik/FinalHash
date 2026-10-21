namespace kt.Models;

public class TrackInfo
{
	public string Path { get; set; } = "";
	public string Artist { get; set; } = "";
	public string Title { get; set; } = "";
	public string Album { get; set; } = "";
	public int Duration { get; set; }
	public long Size { get; set; }
	public string Hash { get; set; } = "";
	public int TagScore { get; set; }
}
