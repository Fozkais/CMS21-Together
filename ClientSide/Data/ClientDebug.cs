using System.Linq;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ClientSide.Data;

public static class ClientDebug
{
    public static void AskForAllCarResync()
    {
        foreach (int carId in ClientData.Instance.LoadedCars.Keys)
        {
            ClientSend.SendResyncCar(carId);
        }
    }
    public static void AskForCarResync(int carId)
    {
        ClientSend.SendResyncCar(carId);
    }

    public static void AskServerForCarParts(int carID)
    {
        if(!ServerData.isRunning) {MelonLogger.Msg("Only host can use this."); return;}
        
        ServerSend.BodyParts(Client.Instance.Id,  ServerData.LoadedCars[carID].partInfo.BodyParts.Values.ToList(), carID, true);
    }

    public static void ShowAllCarPartInfoFromServer(int carID)
    {
        if(!ServerData.isRunning) {MelonLogger.Msg("Only host can use this."); return;}

        ModCar car = ServerData.LoadedCars[carID];

        MelonLogger.Msg($"------ Server carParts for car[{carID}] ------");
        foreach (ModCarPart carPart in car.partInfo.BodyParts.Values)
        {
            MelonLogger.Msg($"{carPart.name} , {carPart.unmounted}");
        }
        MelonLogger.Msg($"------ Server carParts for car[{carID}] end ------");
    }
}