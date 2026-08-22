using System;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Network.Packets
{
    // int[] has reference equality, so it can't be used as a Dictionary key directly.
    // Both client and server must use this to key CarState.SubParts consistently.
    public static class CarSubPartIdentity
    {
        public static string BuildKey(int[] partIndexPath) => string.Join(".", partIndexPath);
    }

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

    // Body-level part (CarPart: hood, doors, bumpers...). When a CarPart is taken
    // off in-game, the game itself converts it into an Item with this exact field
    // shape (Condition/Dent/Color/TintColor/PaintType/PaintData/Livery/Quality/
    // WashFactor...), so ModItem is reused as-is instead of duplicating those fields.
    // PartIndex (not PartName) is the identifier used for resolution: CarPart names
    // are not guaranteed unique among siblings, but carLoader.carParts[] is built
    // deterministically from the same car model on every client.
    [Serializable]
    [NetworkPacket(PacketTypes.CarBodyPartUpdate)]
    public class CarBodyPartUpdatePacket : INetworkData
    {
        public int CarLoaderID;
        public int PartIndex;
        public string PartName; // debug/logging only, not used for resolution
        public bool Switched;
        public bool Unmounted;
        public string TunedID;
        public ModItem State;
    }

    // Mechanical sub-part (PartScript: pistons, belts, hoses...). Identified by a
    // sibling-index chain from the car root down to the target Transform, instead
    // of a name-based path: sibling PartScript objects are often identically named
    // (e.g. all pistons), which makes Transform.Find(namePath) ambiguous - this is
    // in fact a latent bug in the game's own native save path resolution
    // (PartScript.GetGameObjectPathWithoutRoot + Transform.Find). A pure index
    // chain resolved via Transform.GetChild(index) has no such ambiguity.
    [Serializable]
    [NetworkPacket(PacketTypes.CarSubPartUpdate)]
    public class CarSubPartUpdatePacket : INetworkData
    {
        public int CarLoaderID;
        public int[] PartIndexPath;
        public bool Unmounted;
        public float Condition;
        public int Quality;
        public bool IsExamined;
        public bool IsPainted;
        public ModColor Color;
        public ModPaintType PaintType;
        public ModPaintData PaintData;
        public float Dust;
    }
}
