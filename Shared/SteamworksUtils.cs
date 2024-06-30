

using System;
using System.Runtime.InteropServices;
using Steamworks;

namespace CMS21Together.Shared;

public static class SteamworksUtils
{
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
        
    public static void FreeIntPtr(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static ulong ConvertLobbyID(string lobbyCode)
    {
        lobbyCode = lobbyCode.TrimStart('0');
        lobbyCode = lobbyCode.TrimEnd('0');
        
        ulong lobbyID = ulong.Parse(lobbyCode, System.Globalization.NumberStyles.Integer);
        return lobbyID;
    }
}