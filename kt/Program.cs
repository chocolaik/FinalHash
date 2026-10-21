using System.Text;
using System.Text.Json;
using kt.Models;
using kt.Services;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("Умный менеджер плейлистов");
Console.WriteLine();

Console.Write("Введите путь к папке с музыкой: ");
string folder = (Console.ReadLine() ?? "").Trim().Trim('"');

if (!Directory.Exists(folder))
{
	Console.WriteLine("Папка не найдена.");
	return;
}

double threshold = 0.85;

Console.WriteLine();
Console.WriteLine("Сканирование файлов");

List<TrackInfo> tracks;

try
{
	tracks = MusicScanner.Scan(folder);
}
catch (Exception ex)
{
	Console.WriteLine($"Ошибка сканирования: {ex.Message}");
	return;
}

Console.WriteLine();
Console.WriteLine($"Найдено файлов: {tracks.Count}");

if (tracks.Count == 0)
{
	Console.WriteLine("Музыкальные файлы не найдены.");
	return;
}

var metadataGroups = tracks
.GroupBy(t => (
Artist: SimilarityService.NormalizeArtist(t.Artist),
Title: SimilarityService.Normalize(t.Title),
Duration: t.Duration
))
.ToList();

Console.WriteLine();
Console.WriteLine("Группировка по метаданным");

foreach (var group in metadataGroups)
{
	Console.WriteLine();
	Console.WriteLine(
	$"{group.Key.Artist} - {group.Key.Title} ({group.Key.Duration} сек.)");

	foreach (var track in group)
	{
		Console.WriteLine($" {Path.GetFileName(track.Path)}");
	}
}

Console.WriteLine();
Console.WriteLine("Поиск точных дубликатов");

var duplicateGroups = DuplicateFinder.FindExactDuplicates(tracks);

long savedBytes = 0;
int duplicateCount = 0;

foreach (var group in duplicateGroups)
{
	var canonical = DuplicateFinder.ChooseCanonical(group);

	Console.WriteLine();
	Console.WriteLine(
	$"Основной файл: {Path.GetFileName(canonical.Path)}");

	foreach (var duplicate in group.Where(
	t => t.Path != canonical.Path))
	{
		Console.WriteLine(
		$" Дубликат: {Path.GetFileName(duplicate.Path)}");

		duplicateCount++;
		savedBytes += duplicate.Size;
	}
}

Console.WriteLine();
Console.WriteLine($"Найдено точных дубликатов: {duplicateCount}");

var hashGroups = tracks
.Where(t => !string.IsNullOrEmpty(t.Hash))
.GroupBy(t => t.Hash)
.ToList();

var representatives = hashGroups
.Select(g => DuplicateFinder.ChooseCanonical(g))
.Concat(tracks.Where(t => string.IsNullOrEmpty(t.Hash)))
.ToList();

Console.WriteLine();
Console.WriteLine("Поиск похожих треков");

var similarPairs = DuplicateFinder.FindSimilar(
representatives, threshold);

foreach (var pair in similarPairs)
{
	Console.WriteLine();
	Console.WriteLine(Path.GetFileName(pair.File1));
	Console.WriteLine(Path.GetFileName(pair.File2));
	Console.WriteLine($"Сходство: {pair.Similarity:P1}");
}

var similarMap = representatives.ToDictionary(
t => t.Path,
t => new HashSet<string>());

foreach (var pair in similarPairs)
{
	similarMap[pair.File1].Add(pair.File2);
	similarMap[pair.File2].Add(pair.File1);
}

var reportGroups = new List<TrackGroup>();

foreach (var group in hashGroups)
{
	var canonical = DuplicateFinder.ChooseCanonical(group);

	reportGroups.Add(new TrackGroup
	{
		CanonicalFile = canonical.Path,
		ExactDuplicates = group
	.Where(t => t.Path != canonical.Path)
	.Select(t => t.Path)
	.ToList(),
		SimilarVersions = similarMap[canonical.Path]
	.OrderBy(p => p)
	.ToList()
	});
}

foreach (var track in tracks.Where(
t => string.IsNullOrEmpty(t.Hash)))
{
	reportGroups.Add(new TrackGroup
	{
		CanonicalFile = track.Path,
		SimilarVersions = similarMap[track.Path]
	.OrderBy(p => p)
	.ToList()
	});
}

var report = new
{
	TotalFiles = tracks.Count,
	MetadataGroups = metadataGroups.Count,
	ExactDuplicateGroups = duplicateGroups.Count,
	ExactDuplicateFiles = duplicateCount,
	SimilarPairs = similarPairs,
	PotentialSavingsMB = Math.Round(
savedBytes / 1024.0 / 1024.0, 2),
	Groups = reportGroups
};

string reportPath = Path.Combine(
Environment.CurrentDirectory,
"report.json");

try
{
	string json = JsonSerializer.Serialize(
	report,
	new JsonSerializerOptions
	{
		WriteIndented = true
	});

	File.WriteAllText(reportPath, json, Encoding.UTF8);

	Console.WriteLine();
	Console.WriteLine("Результаты анализа");
	Console.WriteLine($"Всего файлов: {tracks.Count}");
	Console.WriteLine($"Групп по метаданным: {metadataGroups.Count}");
	Console.WriteLine($"Точных дубликатов: {duplicateCount}");
	Console.WriteLine($"Похожих пар: {similarPairs.Count}");
	Console.WriteLine(
	$"Возможная экономия: {savedBytes / 1024.0 / 1024.0:F2} МБ");
	Console.WriteLine($"Отчёт сохранён: {reportPath}");
}
catch (Exception ex)
{
	Console.WriteLine($"Ошибка сохранения отчёта: {ex.Message}");
}
