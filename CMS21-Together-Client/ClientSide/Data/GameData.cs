using System.Collections;
using CMS.Managers;
using CMS.UI.Logic.Upgrades;
using CMS21Together.ClientSide.Data.Garage;
using CMS21Together.ServerSide;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data;

public class GameData
{
	public static GameData Instance;
	public static bool isReady;
	public CarLoader[] carLoaders;
	public EngineStandLogic engineStandLogic;
	public EngineStandLogic engineStandLogic2;
	public Inventory localInventory;

	public GameObject localPlayer;
	public OrderGenerator orderGenerator;
	public PaintshopManager paintshopManager;
	public SpringClampLogic springClampLogic;
	public TireChangerLogic tireChanger;
	public ToolsMoveManager toolsMoveManager;
	public GarageAndToolsTab upgradeTools;
	public WelderLogic welderLogic;
	public WheelBalancerLogic wheelBalancer;

	public GameData()
	{
		localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
		localInventory = GameScript.Get().GetComponent<Inventory>();
		upgradeTools = Object.FindObjectOfType<GarageLevelManager>().garageAndToolsTab;
		toolsMoveManager = Object.FindObjectOfType<ToolsMoveManager>();
		orderGenerator = Object.FindObjectOfType<OrderGenerator>();
		engineStandLogic = Object.FindObjectOfType<EngineStandLogic>();
		springClampLogic = Object.FindObjectOfType<SpringClampLogic>();
		tireChanger = Object.FindObjectOfType<TireChangerLogic>();
		wheelBalancer = Object.FindObjectOfType<WheelBalancerLogic>();
		welderLogic = Object.FindObjectOfType<WelderLogic>();
		paintshopManager = Object.FindObjectOfType<PaintshopManager>();
		carLoaders = new[]
		{
			GameScript.Get().carOnScene[0],
			GameScript.Get().carOnScene[3],
			GameScript.Get().carOnScene[4],
			GameScript.Get().carOnScene[1],
			GameScript.Get().carOnScene[2]
		};
		LoadEngineStand();
		isReady = true;
		if (!Server.Instance.isRunning)
			MelonCoroutines.Start(GarageResync.ResyncGarage());
		MelonLogger.Msg("[GameData->Initialize] GameData ready.");
	}

	public void LoadEngineStand()
	{
		engineStandLogic2 = Object.Instantiate(engineStandLogic.gameObject,
			new Vector3(-13.7864f, 0, -3.23f), Quaternion.identity).GetComponent<EngineStandLogic>();
		engineStandLogic2.gameObject.name = "Engine_stand_2";
		engineStandLogic2.EngineStand = engineStandLogic2.transform.GetChild(1).transform.GetChild(3).transform;

		var bundle = AssetBundle.LoadFromStream(DataHelper.DeepCopy(DataHelper.LoadContent("CMS21Together.Assets.engineStand.assets")));
		if (bundle == null)
		{
			MelonLogger.Warning("Impossible de charger l'AssetBundle !");
			return;
		}

		GameObject newObj = null;
		var mesh = bundle.LoadAsset<Mesh>("assets/assetbundles/enginestand.fbx");
		if (mesh == null)
		{
			MelonLogger.Warning("Impossible de charger le Mesh !");
		}
		else
		{
			newObj = new GameObject("EngineStand_A");
			var mf = newObj.AddComponent<MeshFilter>();
			var mr = newObj.AddComponent<MeshRenderer>();
			newObj.transform.position = new Vector3(3.5745f, 0, 0);

			mf.sharedMesh = mesh;
			mr.material = engineStandLogic.transform.GetChild(2).GetComponent<MeshRenderer>().material;
		}

		if (newObj != null) newObj.transform.SetParent(engineStandLogic2.transform, true);


		bundle.Unload(false);
		MelonLogger.Msg("Loaded stand successfully !");
	}

	public static IEnumerator GameReady()
	{
		while (!isReady)
			yield return new WaitForSeconds(0.2f);
	}
}