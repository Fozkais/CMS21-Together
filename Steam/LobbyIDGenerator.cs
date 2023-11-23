using Steamworks;

namespace CMS21MP
{
    public static class LobbyIDGenerator
    {
        public static string GenerateShortCode(SteamId steamID) {//TODO: Do not work

            // Convertir le CSteamID en string
            string fullSteamID = steamID.ToString(); 
  
            // Récupérer par exemple les 4-7 derniers caractères
            string shortCode = fullSteamID.Substring(fullSteamID.Length - 7, 7);

            return shortCode;
        }
        
        public static SteamId DecodeSteamID(string shortCode) { //TODO: Do not work

            // On génère tous les CSteamID possibles
            for (uint i = 0; i < uint.MaxValue; i++) {

                //SteamId steamID = new SteamId().Value = i;
    
                 //Convertir en string complet
               // string fullSteamID = steamID.ToString();

                // Vérifier si les derniers chars correspondent
                //if(fullSteamID.EndsWith(shortCode)) {
                    
                   // return steamID;
               // }
           }
            //Pas trouvé
           // return SteamId; 
           return new SteamId();
        }

    }
}