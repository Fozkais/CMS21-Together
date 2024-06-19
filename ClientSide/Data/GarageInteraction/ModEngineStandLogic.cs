using System;
using System.Collections;
using System.Linq;
using CMS21Together.ClientSide.Data.Car;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.GarageInteraction
{
    [HarmonyPatch]
    public static class ModEngineStandLogic
    {
        public static bool stopCoroutine;
        public static bool listenToEngineStandLogic = true;
        public static bool engineUpdating = false;

        private static bool skippedInitial = false;
        private static bool skipGroupset = false;
        
      //  [HarmonyPatch(typeof(EngineStandLogic), "IncreaseEngineStandAngle")]
    //    [HarmonyPrefix]
        public static void SetEngineAnglePatch(float val)
        {
            if(!Client.Instance.isConnected) return;

            if (listenToEngineStandLogic)
            {
                ClientData.Instance.engineStand.angle = val;
               // ClientSend.SendEngineAngle(val);
            }
        }

     //   [HarmonyPatch(typeof(EngineStandLogic), "SetEngineOnEngineStand")]
      //  [HarmonyPrefix]
        public static void SetEngineGroupPatch(Item engine, EngineStandLogic __instance)
        {
            if(!Client.Instance.isConnected) return;
            
            
            if(!ServerData.isRunning)
                if(!skippedInitial) { skippedInitial = true; return;}
            
            
            if (listenToEngineStandLogic)
            {
                if(__instance.EngineStandIsEmpty()) return;
                
                if (ClientData.Instance.engineStand.needToResync)
                {
                    ClientData.Instance.engineStand.needToResync = false;
                    ClientSend.SendEngineStandResync();
                    return;
                }

                stopCoroutine = true;
                if (engine != null)
                {
                    ModItem item = new ModItem(engine);
                   // ClientSend.SendSetEngineOnStand(item);
                    ClientData.Instance.engineStand.isReferenced = false;
                    ClientData.Instance.engineStand.isHandled = false;
                    ClientData.Instance.engineStand.engineStandParts.Clear();
                    ClientData.Instance.engineStand.engineStandPartsReferences.Clear();
                    ClientData.Instance.engineStand.engine = item;
                    engineUpdating = false;
                    stopCoroutine = false;
                    skipGroupset = true;
                }
                else
                {
                    stopCoroutine = false;
                    MelonLogger.Msg("EngineStand is null!");
                }
            }
        }

        [HarmonyPatch(typeof(EngineStandLogic), nameof(EngineStandLogic.SetGroupOnEngineStand))]
        [HarmonyPrefix]
        public static void SetGroupOnEngineStand(GroupItem groupItem, bool withFade = true, EngineStandLogic __instance=null)
        {
            if(!Client.Instance.isConnected) return;

            MelonCoroutines.Start(SetGroupOnEngineStandCoroutine(groupItem, __instance));

        }

        private static IEnumerator SetGroupOnEngineStandCoroutine(GroupItem groupItem, EngineStandLogic __instance)
        {
            while (!GameData.DataInitialized)
                yield return new WaitForSeconds(0.2f);

            yield return new WaitForEndOfFrame();
            
            if(!ServerData.isRunning)
                if(!skippedInitial) { skippedInitial = true; yield break;}
            
            MelonLogger.Msg("pass1");
            if(skipGroupset) { skipGroupset = false; yield break;}
            
            MelonLogger.Msg("pass2");
            if (listenToEngineStandLogic)
            {
                /*MelonLogger.Msg("pass3");
                if(skippedInitial)
                    if(__instance.EngineStandIsEmpty()) yield break;*/
                
                MelonLogger.Msg("pass4");
                
                if (ClientData.Instance.engineStand.needToResync)
                {
                    ClientData.Instance.engineStand.needToResync = false;
                    ClientSend.SendEngineStandResync();
                    yield break;
                }
                MelonLogger.Msg("pass5");
                
                stopCoroutine = true;
                if (groupItem != null)
                {
                    yield return new WaitForEndOfFrame();
                    
                    
                    yield return new WaitForSeconds(1);
                    
                    ModGroupItem item = new ModGroupItem(groupItem);
                    var position = new Vector3Serializable(GameData.Instance.engineStand.engineGameObject.transform.position);
                    var rotation = new QuaternionSerializable(GameData.Instance.engineStand.engineGameObject.transform.rotation);
                  //  ClientSend.SendSetGroupEngineOnStand(item, position, rotation);
                    
                    ClientData.Instance.engineStand.isReferenced = false;
                    ClientData.Instance.engineStand.isHandled = false;
                    ClientData.Instance.engineStand.engineStandParts.Clear();
                    ClientData.Instance.engineStand.engineStandPartsReferences.Clear();
                    ClientData.Instance.engineStand.Groupengine = item;
                    ClientData.Instance.engineStand.position = position;
                    ClientData.Instance.engineStand.rotation = rotation;
                    engineUpdating = false;
                    stopCoroutine = false;
                }
                else
                {
                    stopCoroutine = false;
                    MelonLogger.Msg("EngineStand is null!");
                }
            }
            else
            {
                listenToEngineStandLogic = true;
            }
        }
        
        [HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_35")]
        [HarmonyPrefix]
        public static void TakeOffEngineFromStandPatch()
        {
            if(!Client.Instance.isConnected) return;
            
            if (listenToEngineStandLogic)
            {
              //  ClientSend.SendEngineTakeOffFromStand();
            }
        }

        public static IEnumerator HandleEngineStand()
        {
            if(!Client.Instance.isConnected) yield break;
            
            if(engineUpdating) yield break;
            
            if(stopCoroutine) yield break;
            
            if(ClientData.Instance.engineStand.engine == null && ClientData.Instance.engineStand.Groupengine == null) yield break;
            
            yield return new WaitForEndOfFrame();
            
            
            engineUpdating = true;
            if (ClientData.Instance != null && ClientData.Instance.engineStand != null)
            {
                if (!ClientData.Instance.engineStand.isReferenced)
                {
                    if(stopCoroutine) yield break;
                    var referenceEngineStand = MelonCoroutines.Start(GetEnginePartReferences());
                    yield return referenceEngineStand;
                }
                else
                {
                    if(stopCoroutine) yield break;
                    if (ClientData.Instance.engineStand.isReferenced)
                    {
                        var handleEngineStand = MelonCoroutines.Start(HandleEngineParts());
                        
                       yield return handleEngineStand;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            engineUpdating = false;
        }

        public static IEnumerator GetEnginePartReferences()
        {
            if(stopCoroutine) yield break;
            
            yield return new WaitForSeconds(.5f);
            yield return new WaitForEndOfFrame();
            
            var engine = GameData.Instance.engineStand.engineGameObject;
            if (engine == null)
            {
                yield return new WaitForSeconds(1);
                yield return new WaitForEndOfFrame();
                engine = GameData.Instance.engineStand.engineGameObject;
            }
            
            if (engine == null) yield break;
            
            var engineParts = engine.GetComponentsInChildren<PartScript>().ToList();

            var reference = ClientData.Instance.engineStand.engineStandPartsReferences;

            for (int i = 0; i < engineParts.Count; i++) 
            {
                if(!reference.ContainsKey(i))
                    reference.Add(i, engineParts[i]);
            }

            
            if (!ClientData.Instance.engineStand.isReferenced)
            {
                yield return new WaitForEndOfFrame();
                ClientData.Instance.engineStand.isReferenced = true;
                MelonLogger.Msg("EngineStand is referenced!");
            }
        }

        public static IEnumerator HandleEngineParts()
        {
            if(stopCoroutine) yield break;
            try
            {
                var references =  ClientData.Instance.engineStand.engineStandPartsReferences;
                var handle = ClientData.Instance.engineStand.engineStandParts;

                for (int i = 0; i < references.Count; i++)
                {
                    if (!ClientData.Instance.engineStand.isHandled)
                    {
                        ModPartScript newPart = new ModPartScript(references[i], i, -1, ModPartType.engine);

                        if (!handle.ContainsKey(i))
                        {
                            handle.Add(i, newPart);
                            if(!ClientData.Instance.engineStand.fromServer)
                                ClientSend.SendCarPart(-1, handle[i]);
                        }
                    }
                    else
                    {
                        if (CheckDifferences(handle[i], references[i]))
                        {
                          //  MelonLogger.Msg("Found Difference on EngineStand");
                            handle[i] =  new ModPartScript(references[i], i, -1, ModPartType.engine);
                            ClientSend.SendCarPart(-1, handle[i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Could not Handle EngineStand : " + e);
            }

            if (!ClientData.Instance.engineStand.isHandled)
            {
                yield return new WaitForEndOfFrame();
                ClientData.Instance.engineStand.isHandled = true;
                ClientData.Instance.engineStand.fromServer = false;
                MelonLogger.Msg("EngineStand is handled!");
            }
        }
        
        private static bool CheckDifferences(ModPartScript handled, PartScript toHandle)
        {
            if (handled.unmounted != toHandle.IsUnmounted)
                return true;
            else if (Math.Abs(handled.dust - toHandle.Dust) > 0.1f)
                return true;
            else if (Math.Abs(handled.condition - toHandle.Condition) > 0.1f)
                return true;
            else if (handled.paintType != (int)toHandle.CurrentPaintType)
                return true;
            else if (handled.paintData.ToGame() != toHandle.CurrentPaintData)
                return true;
            else if (handled.color.IsDifferent(toHandle.currentColor))
                return true;

            return false;
        }

        public static IEnumerator HandleNewPart(ModPartScript carPart)
        {
            if(ModSceneManager.currentScene() != GameScene.garage) yield break;
            
            while (!GameData.DataInitialized)
                yield return new WaitForSeconds(0.4f);
            
            int _count = 0;
            while (!(ClientData.Instance.engineStand.isReferenced && ClientData.Instance.engineStand.isHandled) && _count < 50)
            {
                _count += 1;
                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitForSeconds(.3f);
            yield return new WaitForEndOfFrame();
            
            ClientData.Instance.engineStand.engineStandParts[carPart.partID] = carPart;
            CarUpdater.UpdatePart(-1, carPart, ClientData.Instance.engineStand.engineStandPartsReferences[carPart.partID]);
        }
    }
}