using System.Reflection;
using Il2CppSystem.IO;
using MelonLoader;
using MemoryStream = System.IO.MemoryStream;
using Stream = System.IO.Stream;

namespace CMS21Together.Logic;

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
			MelonLogger.Error("[ConvertStreamToIL2CPP] parameter: sourceStream cannot be null.");
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