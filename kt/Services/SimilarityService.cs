using System.Text.RegularExpressions;

namespace kt.Services;

public static class SimilarityService
{
	public static string Normalize(string text)
	{
		text = text.ToLowerInvariant();
		text = Regex.Replace(text, @"[^\p{L}\p{N}]+", " ");
		return Regex.Replace(text.Trim(), @"\s+", " ");
	}

	public static string NormalizeArtist(string artist)
	{
		string result = Normalize(artist);

		if (result.StartsWith("the "))
			result = result[4..];

		if (result.EndsWith(" the"))
			result = result[..^4];

		return result.Replace(" ", "");
	}

	public static HashSet<string> GetShingles(string text, int n = 3)
	{
		var shingles = new HashSet<string>();

		if (text.Length < n)
		{
			shingles.Add(text);
			return shingles;
		}

		for (int i = 0; i <= text.Length - n; i++)
		{
			shingles.Add(text.Substring(i, n));
		}

		return shingles;
	}

	public static double Jaccard(string first, string second)
	{
		if (first == second)
			return 1.0;

		if (first.Length == 0 || second.Length == 0)
			return 0.0;

		var firstShingles = GetShingles(first);
		var secondShingles = GetShingles(second);

		int intersection = firstShingles
		.Intersect(secondShingles)
		.Count();

		int union = firstShingles
		.Union(secondShingles)
		.Count();

		return union == 0
		? 0
		: (double)intersection / union;
	}
}