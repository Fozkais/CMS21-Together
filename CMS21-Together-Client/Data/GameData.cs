using CMS.UI.Logic.Upgrades;
using UnityEngine;

namespace CMS21Together.Logic;

public class GameData
{
	public static GameData Instance { get; private set; }
	
	public CharacterMotor LocalPlayer { get; private set; }
	public GarageAndToolsTab GarageTools { get; private set; }


	public GameData()
	{
		Instance = this;
		LocalPlayer = Object.FindObjectOfType<CharacterMotor>();
		GarageTools = Object.FindObjectOfType<GarageLevelManager>().garageAndToolsTab;
	}
}