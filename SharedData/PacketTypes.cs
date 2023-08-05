namespace CMS21MP.SharedData
{
    public enum PacketTypes
    {
        #region Lobby and Connection
            welcome = 1,
            disconnect,
            readyState,
            playerInfo,
            startGame,
            spawnPlayer,
            
        #endregion

        #region 

        playerPosition,
        playerRotation

        #endregion
    }
}