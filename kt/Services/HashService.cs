using System.Security.Cryptography;

namespace kt.Services;

public static class HashService
{
	public static string CalculateSHA256(string filePath)
	{
		using FileStream stream = new FileStream(
		filePath,
		FileMode.Open,
		FileAccess.Read,
		FileShare.Read,
		bufferSize: 81920
		);

		using SHA256 sha256 = SHA256.Create();

		byte[] hashBytes = sha256.ComputeHash(stream);

		return Convert.ToHexString(hashBytes);
	}
}