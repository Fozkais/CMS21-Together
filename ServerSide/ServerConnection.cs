using System.Net;
using System.Net.Sockets;
using CMS21Together.ClientSide.Data;
using CMS21Together.ServerSide.Data;
using CMS21Together.ServerSide.Transports;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ServerSide;

public class ServerConnection
{
    public int id;
    public bool isConnected;
    public NetworkType connectionType;
    
    public TCPConnection tcp;
    public UDPConnection udp;
    public SteamConnection steam;

    public ServerConnection(int i)
    {
        id = i;
        tcp = new TCPConnection(id);
        udp = new UDPConnection(id);
        steam = new SteamConnection(id);
    }

    public void Connect(TcpClient connection)
    {
        tcp.Connect(connection);
        connectionType = NetworkType.tcp;
        isConnected = true;
        
        ServerSend.ConnectPacket(id, "Connected to the server.");
    }
    
    public void Connect()
    {
        connectionType = NetworkType.steam;
        isConnected = true;
        
        ServerSend.ConnectPacket(id, "Connected to the server.");
    }
    
    public void Connect(IPEndPoint endpoint)
    {
        udp.Connect(endpoint);
        isConnected = true;
    }
    
    public void SendData(Packet packet, bool reliable)
    {
        if(connectionType == NetworkType.steam)
            steam.Send(packet, reliable);
        else if (connectionType == NetworkType.tcp)
        {
            if(reliable) 
                tcp.Send(packet);
            else 
                udp.Send(packet);
        }
    }

    public void Disconnect()
    {
        tcp.Disconnect();
        udp.Disconnect();
        steam.Disconnect();
            
        isConnected = false;
    }

    public void SendToLobby(string username)
    {
        ServerData.Instance.connectedClients[id] = new UserData(username, id);
        foreach (UserData data in  ServerData.Instance.connectedClients.Values)
        {
            ServerSend.UserDataPacket(data, id);
        }
        ServerSend.UserDataPacket(ServerData.Instance.connectedClients[id]);
        MelonLogger.Msg($"[ServerConnection->SendToLobby] Sent {username} to lobby!");
    }
}