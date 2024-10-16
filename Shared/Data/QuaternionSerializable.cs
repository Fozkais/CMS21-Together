using System;
using UnityEngine;

namespace CMS21Together.Shared.Data;

[Serializable]
public class QuaternionSerializable
{
	public float x;
	public float y;
	public float z;
	public float w;

	public QuaternionSerializable(Quaternion rotation)
	{
		x = rotation.x;
		y = rotation.y;
		z = rotation.z;
		w = rotation.w;
	}

	public Quaternion toQuaternion()
	{
		return new Quaternion(x, y, z, w);
	}
}