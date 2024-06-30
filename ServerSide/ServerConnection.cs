using System.Net;
using System.Net.Sockets;
using CMS21Together.ServerSide.Transports;
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
    }

    public void Connect(NetworkType connectType, object obj)
    {
        if (connectType == NetworkType.tcp)
        {
            tcp.Connect((TcpClient)obj);
            connectionType = connectType;
        }

        if (connectionType == NetworkType.udp)
        {
            udp.Connect((IPEndPoint)obj);
            connectionType = NetworkType.tcp;
        }
        
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
}