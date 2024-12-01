using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Shared.Data;

[Serializable]
public class PlayerInfo
{
	public string id;
	public Vector3Serializable position;
	public QuaternionSerializable rotation;

	public int playerExp = 0;
	public int playerLevel = 0;

	public Dictionary<string, List<bool>> skillsInfo = new Dictionary<string, List<bool>>();

	public PlayerInfo(string _id, Vector3 _position, Quaternion _rotation, int _exp, int _lvl)
	{
		id = _id;
		position = new Vector3Serializable(_position);
		rotation = new QuaternionSerializable(_rotation);

		playerExp = _exp;
		playerLevel = _lvl;
	}
	
	public void UpdateStats(Vector3Serializable _position, QuaternionSerializable _rotation, int _exp, int _lvl)
	{
		playerExp = _exp;
		playerLevel = _lvl;
		position = _position;
		rotation = _rotation;
	}
	public void UpdateSkill(string skill_ID, List<bool> skill)
	{
		skillsInfo[skill_ID] = skill;
		
		SavesManager.SaveModSave(SavesManager.currentSaveIndex);
	}
}