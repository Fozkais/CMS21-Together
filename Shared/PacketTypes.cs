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
            contentInfo = 5,
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
            
            carResyncs,
            carResync,
            carSpawn,
            carInfo,
            carfluidsData,
            carPosition,
            carPart,
            carParts,
            bodyPart,
            bodyParts,
            
            inventoryItem,
            inventoryGroupItem,
            
            toolMove,
            lifter,
            tireChanger,
            wheelBalancer,
            engineStandAngle,
            setEngineOnStand,
            EngineStandResync,
            setGroupEngineOnStand,
            takeOffEngineFromStand,
            engineCrane,
            oilBin,
            springClampGroup,
            springClampClear
            
        #endregion

    }
}