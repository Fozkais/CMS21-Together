using System;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public class ModMountObject
{
	public bool canBeUnmount;
	public float mountState = 1f;
	public float Condition = 1f;
	public Vector3Serializable oldPos;
	public Vector3Serializable posUnMount;
	public QuaternionSerializable oldRot;
	public float length;
	public Vector3Serializable oldPosChild;
	public QuaternionSerializable oldRotChild;
	public Vector3Serializable childPosUnMount;
	public bool alternativeSFX;
	public bool unmounted;
	public bool reverseMode;
	public bool mouseOver;
	public bool canAction;
	public bool IsStuck;
	public bool PlayingStuckAnim;
	public bool canUpdate = true;

	public ModMountObject(MountObject data)
	{
	}
}