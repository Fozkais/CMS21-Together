using System.Collections.Generic;
using CMS21Together.ClientSide.Data;

namespace CMS21Together.ServerSide.Data;

public class ServerData
{
    public static ServerData Instance;
    public Dictionary<int, UserData> ConnectedClients = new Dictionary<int, UserData>();
}