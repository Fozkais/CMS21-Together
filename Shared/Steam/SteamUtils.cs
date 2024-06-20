using System;
using System.Runtime.InteropServices;
using System.Text;
using MelonLoader;

namespace CMS21Together.Shared.Steam
{
    public static class SteamUtils
    {
        public static string ConvertLobbyID(ulong lobbyID)
        {
            string code = lobbyID.ToBase36();
            
            code = code.PadLeft(5, '0');
            code = code.PadRight(6, '0');

            
            MelonLogger.Msg("Lobby Code : " + code);
            return code;
        }
        
        public static ulong ConvertCode(string code)
        {
            // Supprimer les zéros non significatifs
            code = code.TrimStart('0');
            code = code.TrimEnd('0');

            // Convertir le code de la base 36 en ulong
            ulong lobbyID = ulong.Parse(code, System.Globalization.NumberStyles.Integer);

            MelonLogger.Msg("Converted code : " + lobbyID);
            return lobbyID;
        }
        
        public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int size)
        {
            byte[] byteArray = new byte[size];
            Marshal.Copy(ptr, byteArray, 0, size);
            return byteArray;
        }
        
        public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
        {
            // Alloue de la mémoire non managée
            IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);

            // Copie les données du tableau de bytes dans la mémoire non managée
            Marshal.Copy(byteArray, 0, ptr, byteArray.Length);

            return ptr;
        }
        
    }
    
    
}