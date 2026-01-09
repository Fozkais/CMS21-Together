using System;
using System.Runtime.InteropServices;

namespace CMS21_Together_Core;

public static class SteamNative
{
	[DllImport("steam_api64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_SteamGameServer_v013")]
	public static extern IntPtr GetSteamGameServerPointer();
        
	[DllImport("steam_api64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamGameServer_GetSteamID")]
	public static extern ulong GetSteamID_Native(IntPtr instancePtr);
	
	[DllImport("steam_api64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamGameServer_LogOn")]
	public static extern void LogOn_Native(IntPtr instancePtr, IntPtr pszToken);
}