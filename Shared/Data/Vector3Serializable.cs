using System;
using UnityEngine;

namespace CMS21Together.Shared.Data;

[Serializable]
public class Vector3Serializable
{
	public float x;
	public float y;
	public float z;

	public Vector3Serializable(Vector3 position)
	{
		x = position.x;
		y = position.y;
		z = position.z;
	}

	public Vector3 toVector3()
	{
		return new Vector3(x, y, z);
	}
}