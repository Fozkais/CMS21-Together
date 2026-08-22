using System.Reflection;
using CMS21_Together_Core.Logging;
using Il2CppSystem.IO;
using MemoryStream = System.IO.MemoryStream;
using Stream = System.IO.Stream;

namespace CMS21Together.Utils;

public static class DataUtils
{
	public static Stream LoadContent(string assemblyPath)
	{
		var assembly = Assembly.GetExecutingAssembly();
		
		var stream = assembly.GetManifestResourceStream(assemblyPath);

		return stream;
	}
	
	public static Il2CppSystem.IO.Stream ConvertStreamToIL2CPP(Stream sourceStream)
	{
		if (sourceStream == null)
		{
			Log.Error("[ConvertStreamToIL2CPP] parameter: sourceStream cannot be null.");
			return null;
		}
		
		byte[] serializedData;
		var memoryStream = new MemoryStream();
		sourceStream.CopyTo(memoryStream);
		serializedData = memoryStream.ToArray();

		Il2CppSystem.IO.Stream newStream = new Il2CppSystem.IO.MemoryStream();
		var writer = new BinaryWriter(newStream);
		writer.Write(serializedData);
		writer.Flush();
		
		newStream.Seek(0, SeekOrigin.Begin);
		return newStream;
	}
}