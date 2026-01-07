using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CMS21Together.ServerSide;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Shared;

public static class SteamworksUtils
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

	public static ServerConnection GetClientFromConnection(Connection connection)
	{
		ServerConnection SV_client = Server.Instance.clients.First(s => s.Value.steam.connection.Id == connection.Id).Value;
		if(SV_client == null)
			MelonLogger.Warning($"[SteamworksUtils->GetClientFromConnection] Did not found a valid client.");
		return SV_client;
	}
	
	public static string GetServerID(ulong lobbyID)
	{
		int offset = Random.Next(0, 62);
		
		StringBuilder result = new StringBuilder();
		do
		{
			int index = (int)(lobbyID % 62);

			char newChar = Characters[(index + offset) % 62];
			result.Insert(0, newChar);
			lobbyID /= 62;
		} while (lobbyID > 0);

		// Ajouter le caractère correspondant au décalage à la fin de la chaîne
		result.Append(Characters[offset]);

		return result.ToString();
	}

	public static ulong ConvertServerID(string code)
	{
		char offsetChar = code[code.Length - 1];
		int offset = Characters.IndexOf(offsetChar);
		
		ulong result = 0;
		for (int i = 0; i < code.Length - 1; i++)
		{
			int index = Characters.IndexOf(code[i]);

			int originalIndex = (index - offset + 62) % 62;
			result = result * 62 + (ulong)originalIndex;
		}

		return result;
	}
}