using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide.Functionnality;
using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide
{
    [RegisterTypeInIl2Cpp]
    public class MPGameManager : MonoBehaviour
    {
        
        public static Dictionary<int,Dictionary<int, List<PartScript_Info>>> OriginalParts = new Dictionary<int,Dictionary<int, List<PartScript_Info>>>();
        public static Dictionary<int, Dictionary<int, List<PartScriptInfo>>> PartsHandle = new Dictionary<int, Dictionary<int, List<PartScriptInfo>>>();
        
        public static Dictionary<int, Dictionary<int, PartScript_Info>> OriginalEngineParts = new Dictionary<int, Dictionary<int, PartScript_Info>>();
        public static Dictionary<int, Dictionary<int, PartScriptInfo>> EnginePartsHandle = new Dictionary<int, Dictionary<int, PartScriptInfo>>();
        
        public static Dictionary<int,Dictionary<int, List<PartScript_Info>>> OriginalSuspensionParts = new Dictionary<int,Dictionary<int, List<PartScript_Info>>>();
        public static Dictionary<int,Dictionary<int, List<PartScriptInfo>>> SuspensionPartsHandle = new Dictionary<int,Dictionary<int, List<PartScriptInfo>>>();

        public static bool hasFinishedUpdatingCar = true;

        public void InfoUpdate()
        {
            Movement_Handling.HandleMovement();
            Inventory_Handling.HandleInventory();
            Stats_Handling.HandleStats();
            CarSpawn_Handling.HandleCar();
            SceneSwaping_Handling.UpdatePlayerScene();

            if(hasFinishedUpdatingCar)
                MelonCoroutines.Start(delayCarUpdating());
            
            MoveCar();
            
        }
        public IEnumerator delayCarUpdating()
        {
            hasFinishedUpdatingCar = false;
            yield return new WaitForSeconds(1);
            CarPart_PreHandling.AddAllPartToHandle();
            ExternalCarPart_Handling.HandleCarParts();
            CarPart_Handling.HandleAllParts();
            hasFinishedUpdatingCar = true;
        }
        public void MoveCar()
        {
            foreach (carData carData in  CarSpawn_Handling.carHandler)
            {
                if (carData.carPosition != MainMod.carLoaders[carData.carLoaderID].placeNo)
                {
                    carData.carPosition = MainMod.carLoaders[carData.carLoaderID].placeNo;
                    ClientSend.MoveCar(carData.carPosition, carData.carLoaderID);
                } 
            }
        }
        
    }
}