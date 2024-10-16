using System.Collections;
using CMS.UI.Logic.Upgrades;
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
	public TireChangerLogic tireChanger;
	public GarageAndToolsTab upgradeTools;
	public WheelBalancerLogic wheelBalancer;

	public GameData()
	{
		localPlayer = Object.FindObjectOfType<FPSInputController>().gameObject;
		localInventory = GameScript.Get().GetComponent<Inventory>();
		upgradeTools = Object.FindObjectOfType<GarageLevelManager>().garageAndToolsTab;
		orderGenerator = Object.FindObjectOfType<OrderGenerator>();
		springClampLogic = Object.FindObjectOfType<SpringClampLogic>();
		tireChanger = Object.FindObjectOfType<TireChangerLogic>();
		wheelBalancer = Object.FindObjectOfType<WheelBalancerLogic>();
		carLoaders = new[]
		{
			GameScript.Get().carOnScene[0],
			GameScript.Get().carOnScene[3],
			GameScript.Get().carOnScene[4],
			GameScript.Get().carOnScene[1],
			GameScript.Get().carOnScene[2]
		};

		isReady = true;
	}

	public static IEnumerator GameReady()
	{
		while (!isReady)
			yield return new WaitForSeconds(0.2f);
	}
}