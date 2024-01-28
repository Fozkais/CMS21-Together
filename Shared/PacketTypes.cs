namespace CMS21Together.Shared
{
    public enum PacketTypes
    {
        #region Lobby and Connection
            empty = 0,
            welcome = 1,
            keepAlive = 2,
            disconnect,
            readyState,
            playersInfo,
            playerInfo,
            startGame,
            spawnPlayer,
            
        #endregion

        #region In-Game

            playerPosition,
            playerRotation,
            playerSceneChange,
            stats,
            
            carInfo,
            carPosition,
            carPart,
            carParts,
            bodyPart,
            bodyParts,
            carList,
            
            inventoryItem,
            inventoryGroupItem,
            
            lifter,
            tireChanger,
            wheelBalancer,
            carWash
            
        #endregion

    }
}