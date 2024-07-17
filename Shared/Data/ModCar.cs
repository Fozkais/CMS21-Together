using System;
using System.Collections.Generic;
using Il2Cpp;

namespace CMS21Together.Shared.Data;

[Serializable]
public class ModCar
{
    public int carLoaderID;
    public string carID;
    public int configVersion;
    public int carPosition;
    public bool customerCar;

    [NonSerialized]public ModPartInfo partInfo;
    
    public bool isReady;
    public bool isFromServer;
    
    public ModCar(int _carLoaderID, string _carID, int _configVersion,int _carPosition=-1, bool _customerCar=false)
    {
        this.carLoaderID = _carLoaderID;
        this.carID = _carID;
        this.partInfo = new ModPartInfo();
        this.carPosition = _carPosition;
        this.configVersion = _configVersion;
        this.customerCar = _customerCar;

        this.isFromServer = false;
        this.isReady = false;
    }
}

public class ModPartInfo
{
    public Dictionary<int, List<PartScript>> OtherPartsReferences = new Dictionary<int, List<PartScript>>();
    public Dictionary<int, List<PartScript>> SuspensionPartsReferences  = new Dictionary<int, List<PartScript>>();
    public Dictionary<int, PartScript> EnginePartsReferences  = new Dictionary<int, PartScript>();
    public Dictionary<int, PartScript> DriveshaftPartsReferences  = new Dictionary<int, PartScript>();
    public Dictionary<int, CarPart> BodyPartsReferences  = new Dictionary<int, CarPart>();
        
}