using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;

namespace CMS21Together.ServerSide.Data;

public class ServerData
{
    public static ServerData Instance;
    
    public Dictionary<int, UserData> connectedClients = new Dictionary<int, UserData>();
    public List<ModItem> items = new List<ModItem>();
    public List<ModGroupItem> groupItems = new List<ModGroupItem>();
    public int money, scrap;

    public ModNewCarData CarData;

    public void SendCar(int loaderID)
    {
        ServerSend.LoadCarPacket(2, CarData, loaderID);
    }
}