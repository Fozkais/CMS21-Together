using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CMS21Together.Shared;

[Serializable]
public class Packet : IDisposable
{
	private List<byte> buffer;
	private bool disposed;
	private byte[] readableBuffer;
	private int readPos;

	public Packet()
	{
		buffer = new List<byte>(); // Intitialize buffer
		readPos = 0; // Set readPos to 0
	}

	public Packet(int id)
	{
		buffer = new List<byte>();
		readPos = 0;

		Write(id);
	}

	public Packet(byte[] data)
	{
		buffer = new List<byte>(); // Intitialize buffer
		readPos = 0; // Set readPos to 0

		SetBytes(data);
	}

	#region functions

	public void SetBytes(byte[] data)
	{
		Write(data);
		readableBuffer = buffer.ToArray();
	}

	public void WriteLength()
	{
		buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
	}

	public void InsertInt(int value)
	{
		buffer.InsertRange(0, BitConverter.GetBytes(value));
	}

	public byte[] ToArray()
	{
		readableBuffer = buffer.ToArray();
		return readableBuffer;
	}

	public int Length()
	{
		return buffer.Count;
	}

	public int UnreadLength()
	{
		return Length() - readPos;
	}

	public void Reset(bool shouldReset = true)
	{
		if (shouldReset)
		{
			buffer.Clear();
			readableBuffer = null;
			readPos = 0;
		}
		else
		{
			readPos -= 4;
		}
	}

	private byte[] ObjectToByteArray(object obj)
	{
		if (obj == null)
			return null;
		var bf = new BinaryFormatter();
		var ms = new MemoryStream();
		bf.Serialize(ms, obj);

		return ms.ToArray();
	}

	public object ByteArrayToObject(byte[] arrBytes)
	{
		try
		{
			var memStream = new MemoryStream();
			var binForm = new BinaryFormatter();
			memStream.Write(arrBytes, 0, arrBytes.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			var obj = binForm.Deserialize(memStream);

			return obj;
		}
		catch (Exception e)
		{
			throw new Exception($"Could not read value: {e}");
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				buffer = null;
				readableBuffer = null;
				readPos = 0;
			}

			disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion

	#region Write/Read

	public void Write(byte[] value)
	{
		buffer.AddRange(value);
	}

	public void Write(int value)
	{
		buffer.AddRange(BitConverter.GetBytes(value));
	}

	public void Write<T>(T value)
	{
		var array = ObjectToByteArray(value);
		Write(array.Length);
		Write(array);
	}

	public byte[] ReadBytes(int length, bool moveReadPos = true)
	{
		if (buffer.Count > readPos)
		{
			var value = buffer.GetRange(readPos, length).ToArray();
			if (moveReadPos) readPos += length;
			return value;
		}

		throw new Exception("Could not read value of type 'byte[]'!");
	}

	public int ReadInt(bool moveReadPos = true)
	{
		if (buffer.Count > readPos)
		{
			var value = BitConverter.ToInt32(readableBuffer, readPos);
			if (moveReadPos) readPos += 4;
			return value;
		}

		throw new Exception("Could not read value of type 'int'!");
	}

	public T Read<T>()
	{
		var lenght = ReadInt(); // Get the length of the byte array
		var array = ReadBytes(lenght); // Return the bytes
		return (T)ByteArrayToObject(array);
	}

	#endregion
}