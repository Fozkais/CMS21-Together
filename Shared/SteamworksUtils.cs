using System;
using System.Linq;
using System.Runtime.InteropServices;
using CMS21Together.ServerSide;
using MelonLoader;
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
    
   /* public static string ConvertServerID(SteamId lobbyID)
    {
        string code = lobbyID.Value.ToBase36();
            
        code = code.PadLeft(5, '0');
        code = code.PadRight(6, '0');
            
        return code;
    }

    public static ServerConnection GetClientFromConnection(Connection connection)
    {
        ServerConnection SV_client = Server.Instance.clients.First(s => s.Value.steam.connection.Id == connection.Id).Value;
        if(SV_client == null)
            MelonLogger.Warning($"[SteamworksUtils->GetClientFromConnection] Did not found a valid client.");
        return SV_client;
    }*/
}