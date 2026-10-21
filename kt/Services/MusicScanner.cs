using kt.Models;

namespace kt.Services;

public static class MusicScanner
{
	public static List<TrackInfo> Scan(string folder)
	{
		var tracks = new List<TrackInfo>();

		string[] extensions =
		{
".mp3", ".flac", ".wav", ".m4a", ".ogg"
};

		foreach (string path in Directory.EnumerateFiles(
		folder, "*", SearchOption.AllDirectories))
		{
			if (!extensions.Contains(
			Path.GetExtension(path).ToLowerInvariant()))
				continue;

			try
			{
				using var file = TagLib.File.Create(path);

				var track = new TrackInfo
				{
					Path = path,
					Artist = file.Tag.FirstPerformer ?? "",
					Title = file.Tag.Title ?? "",
					Album = file.Tag.Album ?? "",
					Duration = (int)Math.Round(
				file.Properties.Duration.TotalSeconds),
					Size = new FileInfo(path).Length
				};

				track.TagScore =
				(string.IsNullOrWhiteSpace(track.Artist) ? 0 : 1) +
				(string.IsNullOrWhiteSpace(track.Title) ? 0 : 1) +
				(string.IsNullOrWhiteSpace(track.Album) ? 0 : 1);

				tracks.Add(track);

				Console.WriteLine(
				$"{track.Artist} - {track.Title} ({track.Duration} сек.)");
			}
			catch (Exception ex)
			{
				Console.WriteLine(
				$"Не удалось прочитать {Path.GetFileName(path)}: {ex.Message}");
			}
		}

		return tracks;
	}
}