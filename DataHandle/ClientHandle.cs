using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CMS21MP.DataHandle;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CMS21MP.ClientSide
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

            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
            MainMod.isConnected = true;

            if (!MainMod.isHosting)
            {
                MainMod.localInventory.items.Clear();
                GlobalData.PlayerMoney = _serverMoney;
                playerManagement.moneyHandler = _serverMoney;
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
           if (PlayerManager.players.ContainsKey(_id) && GameObject.Find($"{PlayerManager.players[_id].username}") != null)
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
                if (GameObject.Find($"{PlayerManager.players[_id].username}") != null)
                {
                    PlayerManager.players[_id].transform.rotation = _rotation;
                }
            }
        }

        public static void PlayerInventory(Packet _packet)
        {
            int _playerId = _packet.ReadInt();
            string _itemID = _packet.ReadString();
            float _itemCondition = _packet.ReadFloat();
            int _itemQuality = _packet.ReadInt();
            long _itemUID = _packet.ReadLong();
            bool status = _packet.ReadBool();

            Item _item = new Item();
            _item.ID = _itemID;
            _item.Condition = _itemCondition;
            _item.Quality = _itemQuality;
            _item.UID = _itemUID;
            
           // MelonLogger.Msg($"Received ItemFromServer, ID:{_itemID}, UID:{_itemUID}, Type:{status}");

            if (status)
            {
                if (!playerManagement.ItemsUID.Contains(_itemUID) )
                {
                   // MelonLogger.Msg($"Adding Item with UID:{_itemUID}");
                    playerManagement.InventoryHandler.Add(_item);
                    playerManagement.ItemsUID.Add(_itemUID);
                    MainMod.localInventory.Add(_item);
                }
            }
            else
            {
                if (playerManagement.ItemsUID.Contains(_itemUID))
                {
                   // MelonLogger.Msg($"Removing Item with UID:{_itemUID}");
                    playerManagement.InventoryHandler.Remove(_item);
                    playerManagement.ItemsUID.Remove(_itemUID);
                    MainMod.localInventory.Delete(_item);
                }
            }
            
        }

        public static void PlayerDisconnect(Packet _packet)
        {
            int _id = _packet.ReadInt();

            if (PlayerManager.players.ContainsKey(_id) && GameObject.Find(PlayerManager.players[_id].username) != null)
            {
                Object.Destroy(GameObject.Find(PlayerManager.players[_id].username));
                PlayerManager.players.Remove(_id);
            }
            if(DataUpdating.MovUpdateQueue.ContainsKey(_id))
                DataUpdating.MovUpdateQueue.Remove(_id);
            if(DataUpdating.RotUpdateQueue.ContainsKey(_id))
                DataUpdating.RotUpdateQueue.Remove(_id);

        }

        public static void PlayerMoney(Packet _packet)
        {
            int money = _packet.ReadInt();
            bool status = _packet.ReadBool();
            if (status)
            {
                MelonLogger.Msg($"Adding Money!! +{money}");
                GlobalData.AddPlayerMoney(money);
                playerManagement.moneyHandler = GlobalData.PlayerMoney;
            }
            else
            {
                MelonLogger.Msg($"Removing Money!! {money}");
                GlobalData.AddPlayerMoney(money);
                playerManagement.moneyHandler = GlobalData.PlayerMoney;
            }
        }
        public static void PlayerScene(Packet _packet)
        {
            int _id = _packet.ReadInt();
            string _username = _packet.ReadString();
            string _scene = _packet.ReadString();
            
            MelonLogger.Msg($"received new scene from {_username}, scene: {_scene}");

            if (SceneManager.GetActiveScene().name == _scene && GameObject.Find($"{_username}") == null)
            {
                PlayerManager.instance.SpawnPlayer(_id, _username, new Vector3(0,0,0), new Quaternion(0,0,0,0));
            }
            else if(GameObject.Find($"{_username}") != null && _id != Client.instance.myId)
            {
                Object.Destroy(GameObject.Find($"{_username}"));
                PlayerManager.players.Remove(_id);
            }
        }

        public static void SpawnCars(Packet _packet)
        {
            carData data = _packet.ReadCarData();

            MelonLogger.Msg($"CL: Received new car info : ID[{data.carID}], carPos[{data.carPosition}], status[{data.status}");
            
            if (data.status)
            {
                if (!playerManagement.carHandler.Contains(data))
                {
                    playerManagement.carHandler.Add(data);
                }

                Color carColor = new Color(data.carColor.r,data.carColor.g,data.carColor.b,data.carColor.a);

                MainMod.carLoaders[data.carLoaderID].placeNo = data.carPosition;
                MainMod.carLoaders[data.carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[data.carLoaderID].color = carColor;
                MainMod.carLoaders[data.carLoaderID].gameObject.GetComponentInChildren<CarDebug>().LoadCar(data.carID);
            }
            else
            {
                if (playerManagement.carHandler.Contains(data))
                {
                    playerManagement.carHandler.Remove(data);
                }
                
                MainMod.carLoaders[data.carLoaderID].DeleteCar();
            }
            
        }
        
        public static void MoveCar(Packet _packet)
        {
            int _carPos = _packet.ReadInt();
            int _carLoaderID = _packet.ReadInt();
            
            MelonLogger.Msg("CL: Received a new carPos, moving car...");

            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                MainMod.carLoaders[_carLoaderID].placeNo = _carPos;
                MainMod.carLoaders[_carLoaderID].PlaceAtPosition();
                MainMod.carLoaders[_carLoaderID].ChangePosition(_carPos);
            }
            
        }
        
        public static void carParts(Packet _packet)
        {
            int _carLoaderID = _packet.ReadInt();
            List<C_PartScriptData> _carParts = _packet.ReadCarPartList();
            partType partType = (partType)_packet.ReadInt();
            
            MelonLogger.Msg($"CL: Received a new carParts:{_carParts[0].carPartName}, that contains {_carParts.Count} parts");
            if(partType == partType.part)
                MelonCoroutines.Start(delayPartUpdate(_carParts, _carLoaderID));
            else if(partType == partType.engine)
                MelonCoroutines.Start(delayEnginePartUpdate(_carParts, _carLoaderID));
            else if(partType == partType.suspensions)
                MelonCoroutines.Start(delaySuspensionPartUpdate(_carParts, _carLoaderID));
        }

        static IEnumerator delayPartUpdate(List<C_PartScriptData> _parts, int _carLoaderID)
        {
            yield return new WaitForSeconds(1.5f);
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {

                var PartInCl = MainMod.carLoaders[_carLoaderID].Parts._items[_parts[0].partID];
                var parts = PartInCl.p_handle.GetComponentsInChildren<PartScript>().ToList();
                
                foreach (C_PartScriptData _part in _parts)
                {
                    foreach (PartScript part in parts)
                    {
                        
                        if (part.id == _part.id)
                        {
                            Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                            
                            if (part.tunedID != _part.tunedID)
                                MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                            part.IsExamined = _part.isExamined;
                            part.IsPainted = _part.isPainted;
                            if (_part.isPainted)
                            {
                                part.CurrentPaintType = (PaintType)_part.paintType;
                                //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                part.SetColor(color);
                                //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                            }

                            part.Quality = _part.quality;
                            part.SetCondition(_part.condition);
                            part.UpdateDust(_part.dust, true);
                            // Handle Bolts
                            if (part.IsUnmounted)
                                part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                        }
                    }
                }
            }
        }
        static IEnumerator delayEnginePartUpdate(List<C_PartScriptData> _parts, int _carLoaderID)
        {
            yield return new WaitForSeconds(1.5f);
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {

                var PartInCl = MainMod.carLoaders[_carLoaderID].e_engine_h;
                var parts = PartInCl.GetComponentsInChildren<PartScript>().ToList();
                
                foreach (C_PartScriptData _part in _parts)
                {
                    foreach (PartScript part in parts)
                    {
                        
                        if (part.id == _part.id)
                        {
                            Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                            
                            if (part.tunedID != _part.tunedID)
                                MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                            part.IsExamined = _part.isExamined;
                            part.IsPainted = _part.isPainted;
                            if (_part.isPainted)
                            {
                                part.CurrentPaintType = (PaintType)_part.paintType;
                                //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                part.SetColor(color);
                                //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                            }

                            part.Quality = _part.quality;
                            part.SetCondition(_part.condition);
                            part.UpdateDust(_part.dust, true);
                            // Handle Bolts
                            if (part.IsUnmounted)
                                part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                        }
                    }
                }
            }
        }
        static IEnumerator delaySuspensionPartUpdate(List<C_PartScriptData> _parts, int _carLoaderID)
        {
            yield return new WaitForSeconds(1.5f);
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                
                List<PartScript> frontCenter = MainMod.carLoaders[_carLoaderID].s_frontCenter_h.GetComponentsInChildren<PartScript>().ToList(); // 0
                List<PartScript> frontLeft = MainMod.carLoaders[_carLoaderID].s_frontLeft_h.GetComponentsInChildren<PartScript>().ToList(); // 1
                List<PartScript> frontRight = MainMod.carLoaders[_carLoaderID].s_frontRight_h.GetComponentsInChildren<PartScript>().ToList(); // 2
                List<PartScript> rearCenter = MainMod.carLoaders[_carLoaderID].s_rearCenter_h.GetComponentsInChildren<PartScript>().ToList(); // 3
                List<PartScript> rearLeft = MainMod.carLoaders[_carLoaderID].s_rearLeft_h.GetComponentsInChildren<PartScript>().ToList(); // 4
                List<PartScript> rearRight = MainMod.carLoaders[_carLoaderID].s_rearRight_h.GetComponentsInChildren<PartScript>().ToList(); // 5
                

                foreach (C_PartScriptData _part in _parts)
                {
                    if (_part.s_indexer == 0)
                    {
                        foreach (PartScript part in frontCenter)
                        {
                            
                            if (part.id == _part.id)
                            {
                                Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                                
                                if (part.tunedID != _part.tunedID)
                                    MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                                part.IsExamined = _part.isExamined;
                                part.IsPainted = _part.isPainted;
                                if (_part.isPainted)
                                {
                                    part.CurrentPaintType = (PaintType)_part.paintType;
                                    //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                    part.SetColor(color);
                                    //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                    //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                                }

                                part.Quality = _part.quality;
                                part.SetCondition(_part.condition);
                                part.UpdateDust(_part.dust, true);
                                // Handle Bolts
                                part.IsUnmounted = _part.unmounted;
                                if (part.IsUnmounted)
                                    part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                            }
                        }
                    }
                    else if (_part.s_indexer == 1)
                    {
                        foreach (PartScript part in frontLeft)
                        {
                            if (part.id == _part.id)
                            {
                                Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                                
                                if (part.tunedID != _part.tunedID)
                                    MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                                part.IsExamined = _part.isExamined;
                                part.IsPainted = _part.isPainted;
                                if (_part.isPainted)
                                {
                                    part.CurrentPaintType = (PaintType)_part.paintType;
                                    //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                    part.SetColor(color);
                                    //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                    //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                                }

                                part.Quality = _part.quality;
                                part.SetCondition(_part.condition);
                                part.UpdateDust(_part.dust, true);
                                // Handle Bolts
                                if (part.IsUnmounted)
                                    part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                            }
                        }
                    }
                    else if (_part.s_indexer == 2)
                    {
                        foreach (PartScript part in frontRight)
                        {
                            
                            if (part.id == _part.id)
                            {
                                Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                                
                                if (part.tunedID != _part.tunedID)
                                    MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                                part.IsExamined = _part.isExamined;
                                part.IsPainted = _part.isPainted;
                                if (_part.isPainted)
                                {
                                    part.CurrentPaintType = (PaintType)_part.paintType;
                                    //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                    part.SetColor(color);
                                    //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                    //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                                }

                                part.Quality = _part.quality;
                                part.SetCondition(_part.condition);
                                part.UpdateDust(_part.dust, true);
                                // Handle Bolts
                                if (part.IsUnmounted)
                                    part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                            }
                        }
                    }
                    else if (_part.s_indexer == 3)
                    {
                        foreach (PartScript part in rearCenter)
                        {
                            
                            if (part.id == _part.id)
                            {
                                Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                                
                                if (part.tunedID != _part.tunedID)
                                    MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                                part.IsExamined = _part.isExamined;
                                part.IsPainted = _part.isPainted;
                                if (_part.isPainted)
                                {
                                    part.CurrentPaintType = (PaintType)_part.paintType;
                                    //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                    part.SetColor(color);
                                    //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                    //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                                }

                                part.Quality = _part.quality;
                                part.SetCondition(_part.condition);
                                part.UpdateDust(_part.dust, true);
                                // Handle Bolts
                                if (part.IsUnmounted)
                                    part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                            }
                        }
                    }
                    else if (_part.s_indexer == 4)
                    {
                        foreach (PartScript part in rearLeft)
                        {
                            
                            if (part.id == _part.id)
                            {
                                Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                                
                                if (part.tunedID != _part.tunedID)
                                    MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                                part.IsExamined = _part.isExamined;
                                part.IsPainted = _part.isPainted;
                                if (_part.isPainted)
                                {
                                    part.CurrentPaintType = (PaintType)_part.paintType;
                                    //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                    part.SetColor(color);
                                    //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                    //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                                }

                                part.Quality = _part.quality;
                                part.SetCondition(_part.condition);
                                part.UpdateDust(_part.dust, true);
                                // Handle Bolts
                                if (part.IsUnmounted)
                                    part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                            }
                        }
                    }
                    else if (_part.s_indexer == 5)
                    {
                        foreach (PartScript part in rearRight)
                        {
                            
                            if (part.id == _part.id)
                            {
                                Color color = new Color(_part.color.r, _part.color.g, _part.color.b, _part.color.a);
                                
                                if (part.tunedID != _part.tunedID)
                                    MainMod.carLoaders[_carLoaderID].TunePart(part.tunedID, _part.tunedID);
                                part.IsExamined = _part.isExamined;
                                part.IsPainted = _part.isPainted;
                                if (_part.isPainted)
                                {
                                    part.CurrentPaintType = (PaintType)_part.paintType;
                                    //part.CurrentPaintData = _part.paintData; Not Handled Yet
                                    part.SetColor(color);
                                    //if ((PaintType)_part.paintType == PaintType.Custom)  Not Handled Yet
                                    //PaintHelper.SetCustomPaintType(part.gameObject, _part.paintData, false);  Not Handled Yet
                                }

                                part.Quality = _part.quality;
                                part.SetCondition(_part.condition);
                                part.UpdateDust(_part.dust, true);
                                // Handle Bolts
                                if (part.IsUnmounted)
                                    part.HideBySavegame(false, MainMod.carLoaders[_carLoaderID]);
                            }
                        }
                    }
                }
            }
        }

            public static void bodyPart(Packet _packet)
            {
                int _carLoaderID = _packet.ReadInt();
                C_carPartsData _bodyPart = _packet.ReadBodyPart();

                MelonLogger.Msg($"CL: Received a new bodyPart! {_bodyPart.name}");
                MelonCoroutines.Start(delayCarPartUpdate(_bodyPart, _carLoaderID));
            }


        static IEnumerator delayCarPartUpdate(C_carPartsData _bodyPart, int _carLoaderID)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (!String.IsNullOrEmpty(MainMod.carLoaders[_carLoaderID].carToLoad))
            {
                Color color = new Color(_bodyPart.colors.r, _bodyPart.colors.g, _bodyPart.colors.b, _bodyPart.colors.a);
                Color tintColor = new Color(_bodyPart.TintColor.r, _bodyPart.TintColor.g, _bodyPart.TintColor.b, _bodyPart.TintColor.a);
              
                CarPart _part = MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID];

                _part.IsTinted = _bodyPart.isTinted;
                _part.TintColor = tintColor;
                _part.Color = color;
                _part.PaintType = (PaintType)_bodyPart.paintType;
                // _part.PaintData = _bodyPart.paintData;  Not Handled Yet.
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
                    //MainMod.carLoaders[_carLoaderID].SetCustomCarPaintType(_part, _bodyPart.paintData);   Not Handled Yet.
                    MainMod.carLoaders[_carLoaderID].SetCarColorAndPaintType(_part, color, (PaintType)_bodyPart.paintType);
                }
                MainMod.carLoaders[_carLoaderID].SetCarLivery(_part, _bodyPart.livery, _bodyPart.liveryStrength);
                if(!_part.Unmounted && _bodyPart.unmounted)
                    MainMod.carLoaders[_carLoaderID].TakeOffCarPartFromSave(_bodyPart.name);
                
                if (_part.Switched != _bodyPart.switched)
                    MainMod.carLoaders[_carLoaderID].SwitchCarPart(_part, true, true);
                
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
}