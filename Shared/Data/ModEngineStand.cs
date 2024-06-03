using System.Collections.Generic;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    public class ModEngineStand
    {
        public float angle;
        public ModItem engine;
        public ModGroupItem Groupengine;
        public Vector3Serializable position;
        public QuaternionSerializable rotation;
        

        public bool isHandled;
        public bool isReferenced;
        public bool needToResync;
        public bool fromServer;
        
        public Dictionary<int, ModPartScript> engineStandParts = new Dictionary<int, ModPartScript>();
        public Dictionary<int, PartScript> engineStandPartsReferences = new Dictionary<int, PartScript>();
    }
}