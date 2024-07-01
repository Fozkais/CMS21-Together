namespace CMS21Together.ClientSide.Data;

public class UserData
{
    public string username;
    public string ip;
    public string lobbyID;
    public int playerID;

    public UserData()
    {
        username = "player";
        ip = "127.0.0.1";
        lobbyID = "";
        playerID = 1;
    }
}