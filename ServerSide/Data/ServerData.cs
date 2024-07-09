using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared.Data;

namespace CMS21Together.ServerSide.Data;

public class ServerData
{
    public static ServerData Instance;
    
    public Dictionary<int, UserData> ConnectedClients = new Dictionary<int, UserData>();
    public List<ModItem> items = new List<ModItem>();
    public List<ModGroupItem> groupItems = new List<ModGroupItem>();
}