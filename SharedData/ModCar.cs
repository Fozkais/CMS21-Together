using System;
using System.Collections.Generic;
using CMS21MP.ClientSide.Data;
using CMS21MP.CustomData;
using Il2Cpp;

namespace CMS21MP.SharedData
{
    [Serializable]
    public class ModCar
    {
        public int carLoaderID;
        public string carID;
        public ModPartInfo partInfo;
        
        public int carVersion;
        public int carPosition;
        public string carScene;
        
        public int CarLifterState;

        public bool isCarLoaded;
        public bool isReferences;
        public bool isReady;

        public bool isFromServer;
        
        public ModCar(int _carLoaderID, int _carVersion, string  _carScene,int _carPosition=-1)
        {
            this.carLoaderID = _carLoaderID;
            this.carID = ClientData.carLoaders[_carLoaderID].carToLoad;
            this.partInfo = new ModPartInfo();
            this.carPosition = _carPosition;
            this.carVersion = _carVersion;
            this.carScene = _carScene;

            this.isFromServer = false;
        }

        public ModCar(ModCar _car)
        {
            this.carLoaderID = _car.carLoaderID;
            this.carID = _car.carID;
            this.partInfo = new ModPartInfo();
            this.carPosition = _car.carPosition;
            this.carVersion = _car.carVersion;
            this.carScene = _car.carScene;

            this.isFromServer = true;
            this.isCarLoaded = false;
            this.isReady = false;
            this.isReferences = false;
        }
    }

    [Serializable]
    public class ModPartInfo
    {
        public Dictionary<int, List<ModPartScript>> OtherParts = new Dictionary<int, List<ModPartScript>>();
        public Dictionary<int, List<ModPartScript>> SuspensionParts = new Dictionary<int, List<ModPartScript>>();
        public Dictionary<int, ModPartScript> EngineParts = new Dictionary<int, ModPartScript>();

        // public Dictionary<int, ModPartScript> BodyParts = new Dictionary<int, ModPartScript>(); TODO: Handle body Parts
        
        public Dictionary<int, List<PartScript>> OtherPartsReferences = new Dictionary<int, List<PartScript>>();
        public Dictionary<int, List<PartScript>> SuspensionPartsReferences  = new Dictionary<int, List<PartScript>>();
        public Dictionary<int, PartScript> EnginePartsReferences  = new Dictionary<int, PartScript>();
        
        // public Dictionary<int, PartScript> BodyPartsReferences  = new Dictionary<int, PartScript>(); TODO: Handle body Parts
        
        
    }
}