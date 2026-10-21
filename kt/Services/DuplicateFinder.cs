using kt.Models;

namespace kt.Services;

public static class DuplicateFinder
{
	public static TrackInfo ChooseCanonical(
	IEnumerable<TrackInfo> tracks)
	{
		return tracks
		.OrderByDescending(t => t.TagScore)
		.ThenBy(t => Path.GetFileName(t.Path).Length)
		.ThenBy(t => t.Path, StringComparer.Ordinal)
		.First();
	}

	public static List<IGrouping<string, TrackInfo>> FindExactDuplicates(
	List<TrackInfo> tracks)
	{
		foreach (var track in tracks)
		{
			try
			{
				track.Hash = HashService.CalculateSHA256(track.Path);
			}
			catch (Exception ex)
			{
				Console.WriteLine(
				$"Ошибка хеширования {Path.GetFileName(track.Path)}: {ex.Message}");
			}
		}

		return tracks
		.Where(t => !string.IsNullOrEmpty(t.Hash))
		.GroupBy(t => t.Hash)
		.Where(g => g.Count() > 1)
		.ToList();
	}

	public static List<SimilarPair> FindSimilar(
	List<TrackInfo> tracks,
	double threshold)
	{
		var result = new List<SimilarPair>();

		for (int i = 0; i < tracks.Count; i++)
		{
			for (int j = i + 1; j < tracks.Count; j++)
			{
				var first = tracks[i];
				var second = tracks[j];

				if (string.IsNullOrWhiteSpace(first.Artist) ||
				string.IsNullOrWhiteSpace(second.Artist) ||
				string.IsNullOrWhiteSpace(first.Title) ||
				string.IsNullOrWhiteSpace(second.Title))
					continue;

				if (SimilarityService.NormalizeArtist(first.Artist) !=
				SimilarityService.NormalizeArtist(second.Artist))
					continue;

				if (Math.Abs(first.Duration - second.Duration) > 3)
					continue;

				double similarity = SimilarityService.Jaccard(
				SimilarityService.Normalize(first.Title),
				SimilarityService.Normalize(second.Title));

				if (similarity >= threshold)
				{
					result.Add(new SimilarPair
					{
						File1 = first.Path,
						File2 = second.Path,
						Similarity = Math.Round(similarity, 3)
					});
				}
			}
		}

		return result;
	}
}