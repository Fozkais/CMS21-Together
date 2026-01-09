using System;
using System.Runtime.InteropServices;
using System.Text;
using Steamworks.Data;

namespace CMS21_Together_Core.Network;

public static class SteamNetworkUtils
{
	private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	private static readonly Random Random = new Random();
	
	public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int size)
	{
		byte[] byteArray = new byte[size];
		Marshal.Copy(ptr, byteArray, 0, size);
		return byteArray;
	}
        
	public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
	{
		IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);
		Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
		return ptr;
	}
	
	public static string EncodeServerID(ulong serverID)
	{
		int offset = Random.Next(0, 62);
		
		StringBuilder result = new StringBuilder();
		do
		{
			int index = (int)(serverID % 62);

			char newChar = Characters[(index + offset) % 62];
			result.Insert(0, newChar);
			serverID /= 62;
		} while (serverID > 0);
		
		result.Append(Characters[offset]);

		return result.ToString();
	}

	public static ulong DecodeServerID(string encodedID)
	{
		char offsetChar = encodedID[encodedID.Length - 1];
		int offset = Characters.IndexOf(offsetChar);
		
		ulong result = 0;
		for (int i = 0; i < encodedID.Length - 1; i++)
		{
			int index = Characters.IndexOf(encodedID[i]);

			int originalIndex = (index - offset + 62) % 62;
			result = result * 62 + (ulong)originalIndex;
		}

		return result;
	}
}