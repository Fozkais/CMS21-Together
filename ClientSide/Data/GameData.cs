using System.Collections;
using CMS.Managers;
using CMS.UI.Logic.Upgrades;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class GameData
{
	public static GameData Instance;
	public static bool isReady;
	public CarLoader[] carLoaders;
	public Inventory localInventory;

	public GameObject localPlayer;
	public OrderGenerator orderGenerator;
	public SpringClampLogic springClampLogic;
	public EngineStandLogic engineStandLogic;
	public EngineStandLogic engineStandLogic2;
	public TireChangerLogic tireChanger;
	public GarageAndToolsTab upgradeTools;
	public ToolsMoveManager toolsMoveManager;
	public WheelBalancerLogic wheelBalancer;
	public PaintshopManager paintshopManager;

	public GameData()
	{
		localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
		localInventory = GameScript.Get().GetComponent<Inventory>();
		upgradeTools = Object.FindObjectOfType<GarageLevelManager>().garageAndToolsTab;
		toolsMoveManager = Object.FindObjectOfType<ToolsMoveManager>();
		orderGenerator = Object.FindObjectOfType<OrderGenerator>();
		engineStandLogic = Object.FindObjectOfType<EngineStandLogic>();
		engineStandLogic2 = Object.Instantiate(engineStandLogic.gameObject,
				new Vector3(-16, 0, -3.1f), Quaternion.identity).GetComponent<EngineStandLogic>();
		springClampLogic = Object.FindObjectOfType<SpringClampLogic>();
		tireChanger = Object.FindObjectOfType<TireChangerLogic>();
		wheelBalancer = Object.FindObjectOfType<WheelBalancerLogic>();
		paintshopManager = Object.FindObjectOfType<PaintshopManager>();
		carLoaders = new[]
		{
			GameScript.Get().carOnScene[0],
			GameScript.Get().carOnScene[3],
			GameScript.Get().carOnScene[4],
			GameScript.Get().carOnScene[1],
			GameScript.Get().carOnScene[2]
		};

		isReady = true;
		MelonLogger.Msg("[GameData->Initialize] GameData ready.");
	}
	
	public static IEnumerator GameReady()
	{
		while (!isReady)
			yield return new WaitForSeconds(0.2f);
	}
}