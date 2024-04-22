namespace CMS21Together.Shared
{
    public enum PacketTypes
    {
        #region Lobby and Connection
            empty = 0,
            welcome = 1,
            keepAlive = 2,
            keepAliveConfirmed = 3,
            carLoadInfo = 4,
            disconnect,
            readyState,
            playersInfo,
            playerInfo,
            startGame,
            spawnPlayer,
            
        #endregion

        #region In-Game

            playerInitialPos,
            playerPosition,
            playerRotation,
            playerSceneChange,
            stats,
            
            carResync,
            carInfo,
            carPosition,
            carPart,
            carParts,
            bodyPart,
            bodyParts,
            
            inventoryItem,
            inventoryGroupItem,
            
            lifter,
            tireChanger,
            wheelBalancer,
            engineStandAngle
            
        #endregion

    }
}