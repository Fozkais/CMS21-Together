using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Functionnality;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using CarPart = Il2Cpp.CarPart;
using Inventory = CMS21MP.ClientSide.Functionnality.Inventory;
using Object = UnityEngine.Object;

namespace CMS21MP.DataHandle
{
    public class ClientHandle : MonoBehaviour
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();
            int _serverMoney = _packet.ReadInt();
            
            MelonLogger.Msg($"Message from Server:{_msg}");
            Client.instance.myId = _myId;
            ClientSend.WelcomeReceived();
            //ClientSend.PlayerScene(SceneManager.GetActiveScene().name);
            
            MainMod.isConnected = true;

            if (!MainMod.isHosting)
            {
                MainMod.localInventory.items.Clear();
                MainMod.localInventory.groups.Clear();
                GlobalData.PlayerMoney = _serverMoney;
                Stats.moneyHandler = _serverMoney;
            }
        }

        public static void DLC(Packet _packet)
        {
            ReadOnlyDictionary<string, bool> _dlc = _packet.ReadDLCState();
            Client.instance.Disconnect();
            Client.forceDisconnected = true;
            foreach (KeyValuePair<string, bool> dlc in _dlc)
            {
                if (dlc.Value)
                {
                    MelonLogger.Msg($"{dlc.Value} need to be removed");
                }
                else
                {
                    MelonLogger.Msg($"{dlc.Value} need to be installed");
                }
            }
        }

        public static void PlayerConnected(Packet _packet)
        {
            int _connected = _packet.ReadInt();
            int _maxConnected = _packet.ReadInt();

            MainMod.playerConnected = _connected;
            MainMod.maxPlayer = _maxConnected;
        }
        
        public static void SpawnPlayer(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            Vector3 _position = _packet.ReadVector3();
            Quaternion _rotation = _packet.ReadQuaternion();

            if (!PlayerManager.players.ContainsKey(_id))
            {
                PlayerManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
            }
        }


        public static void PlayerPosition(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Vector3 _position = _packet.ReadVector3();

           // MelonLogger.Msg($"received new pos for player{_id} !");
           if (PlayerManager.players.ContainsKey(_id) && GameObject.Find(PlayerManager.players[_id].username) != null)
           {
               PlayerManager.players[_id].transform.position = _position;
           }
        }
        public static void PlayerRotation(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Quaternion _rotation = _packet.ReadQuaternion();
            
            if (PlayerManager.players.ContainsKey(_id))
            {
                if (GameObject.Find(PlayerManager.players[_id].username) != null)
                {
                    PlayerManager.players[_id].transform.rotation = _rotation;
                }
            }
        }

        public static void ItemReceive(Packet _packet)
        {
            int _playerId = _packet.ReadInt();
            ModItem _Moditem = _packet.ReadModItem();
            bool status = _packet.ReadBool();

            ModItem item = new ModItem();
            Item _item = new Item();
            item.ToGame(_Moditem, _item);
            
           // MelonLogger.Msg($"Received ItemFromServer, ID:{_itemID}, UID:{_itemUID}, Type:{status}");

            if (status)
            {
                if (!Inventory.ItemsUID.Contains(_item.UID) )
                {
                   // MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                   Inventory.ItemsHandler.Add(_item);
                   Inventory.ItemsUID.Add(_item.UID);
                    MainMod.localInventory.Add(_item);
                }
            }
            else
            {
                if (Inventory.ItemsUID.Contains(_item.UID))
                {
                   // MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                   Inventory.ItemsHandler.Remove(_item);
                   Inventory.ItemsUID.Remove(_item.UID);
                    MainMod.localInventory.Delete(_item);
                }
            }
            
        }
        
        public static void GroupItemReceive(Packet _packet)
        {
            int _playerId = _packet.ReadInt();
            ModItemGroup _Moditem = _packet.ReadModItemGroup();
            bool status = _packet.ReadBool();

            ModItemGroup itemGroup = new ModItemGroup();
            GroupItem _item = new GroupItem(); 
            itemGroup.ToGame(_Moditem, _item);
            
            if (status)
            {
                if (!Inventory.ItemsUID.Contains(_item.UID) )
                {
                    // MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                    Inventory.GroupItemsHandler.Add(_item);
                    Inventory.ItemsUID.Add(_item.UID);
                    MainMod.localInventory.AddGroup(_item);
                }
            }
            else
            {
                if (Inventory.ItemsUID.Contains(_item.UID))
                {
                    // MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                    Inventory.GroupItemsHandler.Remove(_item);
                    Inventory.ItemsUID.Remove(_item.UID);
                    MainMod.localInventory.DeleteGroup(_item.UID);
                }
            }
        }

        public static void PlayerDisconnect(Packet _packet)
        {
            int _id = _packet.ReadInt();

            if (Client.instance.myId != _id)
            {
                if (PlayerManager.players.ContainsKey(_id) && GameObject.Find(PlayerManager.players[_id].username) != null)
                {
                    Object.Destroy(GameObject.Find(PlayerManager.players[_id].username));
                    PlayerManager.players.Remove(_id);
                }
                if(Movement.MovUpdateQueue.ContainsKey(_id))
                    Movement.MovUpdateQueue.Remove(_id);
                if(Movement.RotUpdateQueue.ContainsKey(_id))
                    Movement.RotUpdateQueue.Remove(_id);

                MainMod.playerConnected -= 1;
            }
            else
            {
                Client.instance.Disconnect();
                foreach (KeyValuePair<int, PlayerInfo> element in PlayerManager.players)
                {
                    if (element.Value != MainMod.localPlayer.GetComponent<PlayerInfo>())
                    {
                        Destroy(element.Value.gameObject);
                    }
                    else
                    {
                        Destroy(MainMod.localPlayer.GetComponent<PlayerInfo>());
                        Destroy(MainMod.localPlayer.GetComponent<MPGameManager>());
                    }
                }

                MainMod.isPrefabSet = false;
                PlayerManager.players.Clear();
            }


        }

        public static void PlayerMoney(Packet _packet)
        {
            int money = _packet.ReadInt();
            bool status = _packet.ReadBool();
            if (status)
            {
                MelonLogger.Msg($"Adding Money!! +{money}");
                GlobalData.AddPlayerMoney(money);
                Stats.moneyHandler = GlobalData.PlayerMoney;
            }
            else
            {
                MelonLogger.Msg($"Removing Money!! {money}");
                GlobalData.AddPlayerMoney(money);
                Stats.moneyHandler = GlobalData.PlayerMoney;
            }
        }
        public static void PlayerScene(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            string _scene = _packet.ReadString();
            
            MelonLogger.Msg($"received new scene from {_username}, scene: {_scene}");

            PlayerManager.players[_id].activeScene = _scene;
        }

        public async static void SpawnCars(Packet _packet)
        {
            carData data = _packet.ReadCarData();

            MelonLogger.Msg($"CL: Received new car info : ID[{data.carID}], carPos[{data.carPosition}], status[{data.status}");
            
            if (data.status)
            {
                if (!CarSpawn.CarHandle.ContainsKey(data.carLoaderID))
                {
                    CarSpawn.CarHandle.Add(data.carLoaderID, data);
                }

                MainMod.carLoaders[data.carLoaderID].placeNo = data.carPosition;
                MainMod.carLoaders[data.carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[data.carLoaderID].color = new ModColor().Convert(data.carColor);
                MainMod.carLoaders[data.carLoaderID].gameObject.GetComponentInChildren<CarDebug>().LoadCar(data.carID, data.configNumber);

                await Task.Delay(1000);
                PreCarPart.AddAllPartToHandleAlt(data.carLoaderID);
            }
            else
            {
                if (CarSpawn.CarHandle.ContainsKey(data.carLoaderID))
                {
                    CarSpawn.CarHandle.Remove(data.carLoaderID);
                    CarSpawn.RemoveCar(data.carLoaderID);
                }
                
                
                MainMod.carLoaders[data.carLoaderID].DeleteCar();
            }
            
        }
        
        public static void MoveCar(Packet _packet)
        {
            int _carPos = _packet.ReadInt();
            int _carLoaderID = _packet.ReadInt();
            
           // MelonLogger.Msg("CL: Received a new carPos, moving car...");

            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                MainMod.carLoaders[_carLoaderID].placeNo = _carPos;
                MainMod.carLoaders[_carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[_carLoaderID].ChangePosition(_carPos);
            }
            
        }
        
        public static void carParts(Packet _packet)
        {
            bool partType = _packet.ReadBool();
            if (!partType)
            {
                PartScriptInfo _part = _packet.ReadPartScriptInfo();
                MelonCoroutines.Start(delayPartUpdate(_part));
                
            }
            else
            {
                MelonLogger.Msg("Received PartList");
                List<PartScriptInfo> _parts = _packet.ReadPartScriptInfoList();
                foreach (PartScriptInfo part in _parts)
                {
                    MelonCoroutines.Start(delayPartUpdate(part));
                }
            }
        }

        public static void UpdatePart(PartScriptInfo _part)
        {
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_part._carLoaderID].carToLoad))
            {
                if (_part._type == partType.part)
                {

                    MPGameManager.OriginalParts[_part._carLoaderID][_part._partItemID][_part._partCountID]._UniqueID = _part._UniqueID;
                    MPGameManager.PartsHandle[_part._carLoaderID][_part._partItemID][_part._partCountID] = _part;
                    partUpdate(MPGameManager.OriginalParts[_part._carLoaderID][_part._partItemID][_part._partCountID], _part);

                }
                else if (_part._type == partType.engine)
                {
                    MPGameManager.OriginalEngineParts[_part._carLoaderID][_part._partCountID]._UniqueID = _part._UniqueID;
                    MPGameManager.EnginePartsHandle[_part._carLoaderID][_part._partCountID] = _part;
                    partUpdate(MPGameManager.OriginalEngineParts[_part._carLoaderID][_part._partCountID], _part);
                }
                else if(_part._type == partType.suspensions)
                { 
                    MPGameManager.OriginalSuspensionParts[_part._carLoaderID][_part._partItemID][_part._partCountID]._UniqueID = _part._UniqueID;
                    MPGameManager.SuspensionPartsHandle[_part._carLoaderID][_part._partItemID][_part._partCountID] = _part;
                    partUpdate(MPGameManager.OriginalSuspensionParts[_part._carLoaderID][_part._partItemID][_part._partCountID], _part);
                }
            }
        }

        public static void partUpdate(ModPartScript_Info Originalpart, PartScriptInfo Newpart)
        {
          //  MelonLogger.Msg($"handled new part: {Newpart._partScriptData.id}");
            PartScript originalPart = Originalpart._partScript;
            PartScriptData newPart = Newpart._partScriptData;

            if (MainMod.carLoaders[Newpart._carLoaderID] != null)
            {
                if (!String.IsNullOrEmpty(newPart.tunedID))
                {
                    originalPart.tunedID = newPart.tunedID;
                    // if (originalPart.tunedID != newPart.tunedID)
                    // MainMod.carLoaders[Newpart._carLoaderID].TunePart(originalPart.tunedID, newPart.tunedID); 
                }
            }
            originalPart.IsExamined = newPart.isExamined;
            if (newPart.unmounted == false)
            {
                originalPart.IsPainted = newPart.isPainted;
                if (newPart.isPainted)
                {
                    originalPart.CurrentPaintType = (PaintType)newPart.paintType;
                    originalPart.CurrentPaintData = new ModPaintData().Convert(newPart.paintData);
                    originalPart.SetColor(new ModColor().Convert(newPart.color));
                    if ((PaintType)newPart.paintType == PaintType.Custom)
                        PaintHelper.SetCustomPaintType(originalPart.gameObject, originalPart.CurrentPaintData, false);
                }

                originalPart.Quality = newPart.quality;
                originalPart.SetCondition(newPart.condition);
                originalPart.UpdateDust(newPart.dust, true);
                // Handle Bolts

                if (originalPart.IsUnmounted)
                {
                    originalPart.ShowBySaveGame();
                    originalPart.ShowMountAnimation();
                    originalPart.FastMount();
                }
            }
            else
            {
                originalPart.Quality = newPart.quality;
                originalPart.Condition = newPart.condition;
                originalPart.Dust = newPart.dust;
                if (originalPart.IsUnmounted == false)
                {
                    originalPart.HideBySavegame(false, MainMod.carLoaders[Newpart._carLoaderID]);
                }
            }
            originalPart.Update();
        }

        static IEnumerator delayPartUpdate(PartScriptInfo _part)
        {
            if(!CarSpawn.CarHandle[_part._carLoaderID].FinishedPreHandlingEngine || 
               !CarSpawn.CarHandle[_part._carLoaderID].FinishedPreHandlingPart || 
               !CarSpawn.CarHandle[_part._carLoaderID].FinishedPreHandlingSuspension)
                yield return new WaitForSeconds(2f);
            
            if (_part._type == partType.part)
            {
                if (!MPGameManager.OriginalParts.ContainsKey(_part._carLoaderID))
                {
                    PreCarPart.AddPartHandle(_part._carLoaderID);
                    yield return new WaitForSeconds(3f);
                }

                if (!MPGameManager.PartsHandle.ContainsKey(_part._carLoaderID))
                {
                    MPGameManager.PartsHandle.Add(_part._carLoaderID, new Dictionary<int, List<PartScriptInfo>>());
                }
                if (!MPGameManager.PartsHandle[_part._carLoaderID].ContainsKey(_part._partItemID))
                {
                    MPGameManager.PartsHandle[_part._carLoaderID].Add(_part._partItemID, new List<PartScriptInfo>());
                }
                MPGameManager.PartsHandle[_part._carLoaderID][_part._partItemID].Add(_part);
            }
            else if(_part._type == partType.engine)
            {
                if (!MPGameManager.OriginalEngineParts.ContainsKey(_part._carLoaderID))
                {
                    PreCarPart.AddEnginePartHandle(_part._carLoaderID);
                    yield return new WaitForSeconds(3f);
                }

                if (!MPGameManager.EnginePartsHandle.ContainsKey(_part._carLoaderID))
                {
                    MPGameManager.EnginePartsHandle.Add(_part._carLoaderID, new Dictionary<int, PartScriptInfo>());
                }

                if (!MPGameManager.EnginePartsHandle[_part._carLoaderID].ContainsKey(_part._partCountID))
                {
                    MPGameManager.EnginePartsHandle[_part._carLoaderID].Add(_part._partCountID, _part);
                }
            }
            else if (_part._type == partType.suspensions)
            {
                if (!MPGameManager.OriginalSuspensionParts.ContainsKey(_part._carLoaderID))
                {
                    PreCarPart.AddSuspensionPartHandle(_part._carLoaderID);
                    yield return new WaitForSeconds(3f);
                }

                if (!MPGameManager.SuspensionPartsHandle.ContainsKey(_part._carLoaderID))
                {
                    MPGameManager.SuspensionPartsHandle.Add(_part._carLoaderID, new Dictionary<int, List<PartScriptInfo>>());
                }
                if (!MPGameManager.SuspensionPartsHandle[_part._carLoaderID].ContainsKey(_part._partItemID))
                {
                    MPGameManager.SuspensionPartsHandle[_part._carLoaderID].Add(_part._partItemID, new List<PartScriptInfo>());
                }
                MPGameManager.SuspensionPartsHandle[_part._carLoaderID][_part._partItemID].Add(_part);
            }

            UpdatePart(_part);
        }

        public static void bodyPart(Packet _packet)
        {
            carPartsData _bodyPart = _packet.ReadBodyPart();
            
            //MelonLogger.Msg($"CL: Received a new bodyPart! {_bodyPart.name}");
            MelonCoroutines.Start(delayCarPartUpdate(_bodyPart, _bodyPart.carLoaderID));
            
        }


        static IEnumerator delayCarPartUpdate(carPartsData _bodyPart, int _carLoaderID)
        {
            if(!CarSpawn.CarHandle[_carLoaderID].FinishedPreHandlingCarPart)
                yield return new WaitForSeconds(1.5f);
            
            if (!MPGameManager.OriginalEngineParts.ContainsKey(_bodyPart.carLoaderID))
            {
                ExternalCarPart.PreHandleCarParts(_bodyPart.carLoaderID);
                yield return new WaitForSeconds(3f);
            }

            if (!ExternalCarPart.CarPartsHandle.ContainsKey(_bodyPart.carLoaderID))
            {
                ExternalCarPart.CarPartsHandle.Add(_bodyPart.carLoaderID, new Dictionary<int, carPartsData>());
            }

            if (!ExternalCarPart.CarPartsHandle[_bodyPart.carLoaderID].ContainsKey(_bodyPart.carPartID))
            {
                ExternalCarPart.CarPartsHandle[_bodyPart.carLoaderID].Add(_bodyPart.carPartID, _bodyPart);
            }
            
            ExternalCarPart.OriginalCarParts[_bodyPart.carLoaderID][_bodyPart.carPartID]._UniqueID = _bodyPart.UniqueID;
            ExternalCarPart.CarPartsHandle[_bodyPart.carLoaderID][_bodyPart.carPartID] = _bodyPart;
            
            if (MainMod.carLoaders != null && MainMod.carLoaders[_carLoaderID] != null)
            {
                
                if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
                {
                    Color color = new ModColor().Convert(_bodyPart.colors);
                    Color tintColor = new ModColor().Convert(_bodyPart.TintColor);

                    CarPart _part = ExternalCarPart.OriginalCarParts[_bodyPart.carLoaderID][_bodyPart.carPartID]._originalPart;

                    _part.IsTinted = _bodyPart.isTinted;
                    _part.TintColor = tintColor;
                    _part.Color = color;
                    _part.PaintType = (PaintType)_bodyPart.paintType;
                    _part.OutsideRustEnabled = _bodyPart.outsaidRustEnabled;
                    _part.AdditionalString = _bodyPart.additionalString;
                    _part.Quality = _bodyPart.quality;
                    _part.WashFactor = _bodyPart.washFactor;
                    
                    if(_part.TunedID != _bodyPart.tunedID)
                        MainMod.carLoaders[_carLoaderID].TunePart(_part.name, _bodyPart.tunedID);
                    
                    MainMod.carLoaders[_carLoaderID].SetDent(_part, _bodyPart.dent);
                    MainMod.carLoaders[_carLoaderID].EnableDust(_part, _bodyPart.Dust);
                    MainMod.carLoaders[_carLoaderID].SetCondition(_part, _bodyPart.condition);
                    
                    if (!_part.Unmounted && !_part.name.StartsWith("license_plate"))
                    {
                        //MainMod.carLoaders[_carLoaderID].SetCustomCarPaintType(_part, new ModPaintData().Convert(_bodyPart.paintData));  
                       // MainMod.carLoaders[_carLoaderID].SetCarColorAndPaintType(_part, color, (PaintType)_bodyPart.paintType);
                    }
                    MainMod.carLoaders[_carLoaderID].SetCarLivery(_part, _bodyPart.livery, _bodyPart.liveryStrength);
                    
                    if(!_part.Unmounted && _bodyPart.unmounted)
                        MainMod.carLoaders[_carLoaderID].TakeOffCarPartFromSave(_bodyPart.name);
                    if (_part.Unmounted && !_bodyPart.unmounted)
                    {
                        MainMod.carLoaders[_carLoaderID].TakeOnCarPartFromSave(_bodyPart.name);
                    }
                                      
                                 
                    
                    if (_part.Switched != _bodyPart.switched)
                        MainMod.carLoaders[_carLoaderID].SwitchCarPart(_part, false, _bodyPart.switched);


                    if(_bodyPart.isTinted)
                        PaintHelper.SetWindowProperties(_part.handle, (int)(_bodyPart.TintColor.a * 255), _part.TintColor);
                    
                    //MelonLogger.Msg($"Parts[{_bodyPart.carPartID}] updated!, {_bodyPart.name}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name}, {_bodyPart.unmounted}");
                }
                else
                {
                    MelonLogger.Msg($"Loss of data from bodyPart ! {_bodyPart.name} on car with CarLoaderID{_carLoaderID}");
                }
            }

        }

        public static void lifterPos(Packet _packet)
        {
            CarLifterState _state = _packet.ReadCarLifterState();
            int _carLoaderID = _packet.ReadInt();

            MelonLogger.Msg($"received new Pos to Server : {_state.ToString()}");
            var carLifterState = MainMod.carLoaders[_carLoaderID].lifter.currentState;
            var carLifter = MainMod.carLoaders[_carLoaderID].lifter;
            if (carLifterState == CarLifterState.Up && _state == CarLifterState.OnFloor)
            {
                carLifter.Action(1);
                carLifter.Action(1);
            }
            else if (carLifterState == CarLifterState.Up && _state == CarLifterState.Middle)
            {
                carLifter.Action(1);
            }
            else if (carLifterState == CarLifterState.Middle && _state == CarLifterState.Up)
            {
                carLifter.Action(0);
            }
            else if (carLifterState == CarLifterState.Middle && _state == CarLifterState.OnFloor)
            {
                carLifter.Action(1);
            }
            else if (carLifterState == CarLifterState.OnFloor && _state == CarLifterState.Up)
            {
                carLifter.Action(0);
                carLifter.Action(0);
            }
            else if (carLifterState == CarLifterState.OnFloor && _state == CarLifterState.Middle)
            {
                carLifter.Action(0);
            }
        }
        
    }
}