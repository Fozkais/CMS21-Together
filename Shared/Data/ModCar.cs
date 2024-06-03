using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using Il2Cpp;
using UnityEngine.Serialization;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModCar
    {
        public int carLoaderID;
        public string carID;
        public ModPartInfo partInfo;
        
        public int carVersion;
        public int carPosition;
        
        public int CarLifterState;

        public bool isCarReady;
        public bool isReferenced;
        public bool isHandled;
        
        public bool isFromServer = false;
        public bool receivedOtherParts = true;
        public bool receivedEngineParts = true;
        public bool receivedDriveshaftParts = true;
        public bool receivedSuspensionParts = true;
        public bool receivedBodyParts = true;
        public bool CarFullyReceived 
        { 
            get => receivedDriveshaftParts && receivedOtherParts && 
                   receivedSuspensionParts && receivedEngineParts && receivedBodyParts;
        }
        


        public ModCar() {}

        public ModCar(int _carLoaderID, int _carVersion,int _carPosition=-1)
        {
            this.carLoaderID = _carLoaderID;
            this.carID = GameData.Instance.carLoaders[_carLoaderID].carToLoad;
            this.partInfo = new ModPartInfo();
            this.carPosition = _carPosition;
            this.carVersion = _carVersion;

            this.isFromServer = false;
        }
        
        public ModCar(int _carLoaderID, string _carID, int _carVersion,int _carPosition=-1)
        {
            this.carLoaderID = _carLoaderID;
            this.carID = _carID;
            this.partInfo = new ModPartInfo();
            this.carPosition = _carPosition;
            this.carVersion = _carVersion;

            this.isFromServer = false;
        }

        public ModCar(ModCar _car)
        {
            this.carLoaderID = _car.carLoaderID;
            this.carID = _car.carID;
            this.partInfo = new ModPartInfo();
            this.carPosition = _car.carPosition;
            this.carVersion = _car.carVersion;

            this.isFromServer = true;
            this.isCarReady = false;
            this.isHandled = false;
            this.isReferenced = false;
            this.receivedOtherParts = false;
            this.receivedEngineParts = false;
            this.receivedDriveshaftParts = false;
            this.receivedSuspensionParts = false;
            this.receivedBodyParts = false;
        }
    }

    [Serializable]
    public class ModPartInfo
    {
        public Dictionary<int, List<ModPartScript>> OtherParts = new Dictionary<int, List<ModPartScript>>();
        public Dictionary<int, List<ModPartScript>> SuspensionParts = new Dictionary<int, List<ModPartScript>>();
        public Dictionary<int, ModPartScript> EngineParts = new Dictionary<int, ModPartScript>();
        public Dictionary<int, ModPartScript> DriveshaftParts = new Dictionary<int, ModPartScript>();
        public Dictionary<int, ModCarPart> BodyParts = new Dictionary<int, ModCarPart>();
        
        public Dictionary<int, List<PartScript>> OtherPartsReferences = new Dictionary<int, List<PartScript>>();
        public Dictionary<int, List<PartScript>> SuspensionPartsReferences  = new Dictionary<int, List<PartScript>>();
        public Dictionary<int, PartScript> EnginePartsReferences  = new Dictionary<int, PartScript>();
        public Dictionary<int, PartScript> DriveshaftPartsReferences  = new Dictionary<int, PartScript>();
        public Dictionary<int, CarPart> BodyPartsReferences  = new Dictionary<int, CarPart>();
        
    }
}