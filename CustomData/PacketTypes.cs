namespace CMS21Together.SharedData
{
    public enum PacketTypes
    {
        #region Lobby and Connection
            empty = 0,
            welcome = 1,
            keepAlive = 2,
            disconnect,
            readyState,
            playerInfo,
            startGame,
            spawnPlayer,
            
        #endregion

        #region In-Game

            playerPosition,
            playerRotation,
            playerSceneChange,
            
            carInfo,
            carPosition,
            carPart,
            carParts,
            bodyPart,
            bodyParts,
            
            inventoryItem,
            inventoryGroupItem,
            
            lifterPos,
            tireChanger,
            tireChanger_ResetAction,
            wheelBalancer,
            wheelBalancer_UpdateWheel,
            wheelBalancer_ResetAction,
            carWash
            
        #endregion
    }
}