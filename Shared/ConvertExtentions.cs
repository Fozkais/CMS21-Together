using System.Text;

namespace CMS21Together.Shared;

public static class ConvertExtensions
{
	public static string ToBase36(this ulong value)
	{
		var symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		var result = new StringBuilder();

		while (value > 0)
		{
			result.Insert(0, symbols[(int)(value % 36)]); // Cast to int for remainder
			value /= 36;
		}

		return result.ToString();
	}
}