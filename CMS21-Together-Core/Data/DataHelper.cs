using System.IO;
using System.Reflection;

namespace CMS21_Together_Core.Data;

public static class DataHelper
{
	public static Stream LoadContent(string assemblyPath)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly.GetManifestResourceStream(assemblyPath);

		return stream;
	}
}