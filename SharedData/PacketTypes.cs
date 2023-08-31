namespace CMS21MP.SharedData
{
    public enum PacketTypes
    {
        #region Lobby and Connection
            empty = 0,
            welcome = 1,
            disconnect,
            readyState,
            playerInfo,
            startGame,
            spawnPlayer,
            
        #endregion

        #region In-Game

            playerPosition,
            playerRotation,
            
            carInfo,
            carPosition,
            carPart,
            bodyPart,
            
            inventoryItem,
            inventoryGroupItem
            
        #endregion
        
    }
}