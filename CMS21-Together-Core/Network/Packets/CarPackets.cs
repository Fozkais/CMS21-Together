using System;

namespace CMS21_Together_Core.Network.Packets
{
    [Serializable]
    [NetworkPacket(PacketTypes.CarSpawnRequest)]
    public class CarSpawnRequestPacket : INetworkData
    {
        public int CarLoaderID;
        public string CarToLoad;
        public int ConfigVersion;
        public int PlaceNo;
        // Used to determine if this is a job car, showroom car, etc.
        public bool IsJob;
        public int JobID;
    }

    [Serializable]
    [NetworkPacket(PacketTypes.CarSpawnResponse)]
    public class CarSpawnResponsePacket : INetworkData
    {
        public int CarLoaderID;
        public string CarToLoad;
        public int ConfigVersion;
        public int PlaceNo;
        public bool IsJob;
        public int JobID;
    }

    [Serializable]
    [NetworkPacket(PacketTypes.CarSpawnDelete)]
    public class CarSpawnDeletePacket : INetworkData
    {
        public int CarLoaderID;
    }

    // Sent back to the requesting client only, when a CarSpawnRequest fails
    // server-side validation. The sender already ran LoadCar natively (hybrid
    // design), so the client must undo it locally to stay in sync with the server.
    [Serializable]
    [NetworkPacket(PacketTypes.CarSpawnRejected)]
    public class CarSpawnRejectedPacket : INetworkData
    {
        public int CarLoaderID;
        public string Reason;
    }
}
