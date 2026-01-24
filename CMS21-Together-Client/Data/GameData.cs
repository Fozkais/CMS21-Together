using UnityEngine;

namespace CMS21Together.Data;

public class GameData
{
	public static GameData Instance { get; private set; }
	
	public CharacterMotor LocalPlayer { get; private set; }


	public GameData()
	{
		if (Instance != null) return;

		Instance = this;
		LocalPlayer = Object.FindObjectOfType<CharacterMotor>();
	}
}