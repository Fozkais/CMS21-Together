using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using Il2Cpp;

namespace CMS21Together.BothSide
{
    [Serializable]
    public class ModCar
    {
        public int carLoaderID;
        public string carID;
        public ModPartInfo partInfo;
        
        public int carVersion;
        public int carPosition;
        public GameScene carScene;
        
        public int CarLifterState;

        public bool isCarLoaded;
        public bool isReferenced;
        public bool isReady;
        public bool isUpdated;


        public bool asBeenSent;
        

        public bool isFromServer;
        
        public List<ModCarPart> tempBody = new List<ModCarPart>();
        public List<ModPartScript> tempOther = new List<ModPartScript>();
        public List<ModPartScript> tempEngine = new List<ModPartScript>();
        public List<ModPartScript> tempSuspension = new List<ModPartScript>();

        public ModCar() {}

        public ModCar(int _carLoaderID, int _carVersion, GameScene  _carScene,int _carPosition=-1)
        {
            this.carLoaderID = _carLoaderID;
            this.carID = GameData.carLoaders[_carLoaderID].carToLoad;
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
            this.isReferenced = false;
            this.isUpdated = false;
        }
    }

    [Serializable]
    public class ModPartInfo
    {
        public Dictionary<int, List<ModPartScript>> OtherParts = new Dictionary<int, List<ModPartScript>>();
        public Dictionary<int, List<ModPartScript>> SuspensionParts = new Dictionary<int, List<ModPartScript>>();
        public Dictionary<int, ModPartScript> EngineParts = new Dictionary<int, ModPartScript>();
        public Dictionary<int, ModCarPart> BodyParts = new Dictionary<int, ModCarPart>();

        public List<ModPartScript> PartScriptsBuffer = new List<ModPartScript>();
        public List<ModCarPart> CarPartsBuffer = new List<ModCarPart>();
        
        public Dictionary<int, List<PartScript>> OtherPartsReferences = new Dictionary<int, List<PartScript>>();
        public Dictionary<int, List<PartScript>> SuspensionPartsReferences  = new Dictionary<int, List<PartScript>>();
        public Dictionary<int, PartScript> EnginePartsReferences  = new Dictionary<int, PartScript>();
        public Dictionary<int, CarPart> BodyPartsReferences  = new Dictionary<int, CarPart>();
        
        
        
        
    }
    
    [Serializable]
    public enum GameScene
    {
        unknow,
        garage,
        junkyard,
        auto_salon,
        barn

    }
}