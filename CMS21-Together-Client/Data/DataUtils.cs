

using System;
using Il2CppSystem.IO;
using MemoryStream = System.IO.MemoryStream;
using Stream = System.IO.Stream;

namespace CMS21Together.Data;

public static class DataUtils
{
	public static Il2CppSystem.IO.Stream ConvertStreamToIL2CPP(Stream sourceStream)
	{
		if (sourceStream == null)
			throw new ArgumentNullException(nameof(sourceStream));
		
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