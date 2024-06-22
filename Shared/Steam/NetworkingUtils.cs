using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CMS21Together.ServerSide;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Shared.Steam
{
    public static class NetworkingUtils
    {

        public static ServerClient GetClientFromConnection(Connection connection)
        {
            return Server.clients.First(s => s.Value.steam.connection.Id == connection.Id).Value;
        }
        
        
        public static string ConvertServerID(SteamId lobbyID)
        {
            string code = lobbyID.Value.ToBase36();
            
            code = code.PadLeft(5, '0');
            code = code.PadRight(6, '0');
            
            return code;
        }
        
        public static ulong ConvertCode(string code)
        {
            // Supprimer les zéros non significatifs
            code = code.TrimStart('0');
            code = code.TrimEnd('0');

            // Convertir le code de la base 36 en ulong
            ulong lobbyID = ulong.Parse(code, System.Globalization.NumberStyles.Integer);

            MelonLogger.Msg("Initial code : " + code);
            MelonLogger.Msg("Converted code : " + lobbyID);
            return lobbyID;
        }
        
        public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int size)
        {
            byte[] byteArray = new byte[size];
            Marshal.Copy(ptr, byteArray, 0, size);
            NetworkingUtils.FreeIntPtr(ptr);
            return byteArray;
        }
        
        public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        {
            // Alloue de la mémoire non managée
            IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

            // Copie les données du tableau de bytes dans la mémoire non managée
            Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
            NetworkingUtils.FreeIntPtr(ptr);
            return ptr;
        }
        
        public async static void FreeIntPtr(IntPtr ptr)
        {
            await Task.Delay(1000);
            
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        
    }
    
    
}