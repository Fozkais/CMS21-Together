using System.Collections.Generic;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Data;

public class PlayerState
{
	public Dictionary<int, Vector3Serializable> Positions = new Dictionary<int, Vector3Serializable>();
	public Dictionary<int, Vector3Serializable> Velocities = new Dictionary<int, Vector3Serializable>();
	public Dictionary<int, QuaternionSerializable> Rotations = new Dictionary<int, QuaternionSerializable>();
	public Dictionary<int, float> Pitches = new Dictionary<int, float>();
	public Dictionary<int, bool> GroundedStates = new Dictionary<int, bool>();
	public Dictionary<int, bool> CrouchingStates = new Dictionary<int, bool>();
	public Dictionary<int, bool> RunningStates = new Dictionary<int, bool>();
	public Dictionary<int, GameScene> Scenes = new Dictionary<int, GameScene>();
}