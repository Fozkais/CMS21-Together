using CMS21Together.Shared;

namespace CMS21Together.ClientSide.Data;

public class ClientData
{
    public static ClientData Instance;
    public static UserData UserData;

    public ClientData()
    {
        UserData = TogetherModManager.LoadUserData();
    }
}