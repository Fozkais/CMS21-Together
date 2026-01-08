using System;

namespace CMS21_Together_Core.Data;

[Serializable]
public class Vector3Serializable
{
	public float x;
	public float y;
	public float z;

	public Vector3Serializable()
	{
		x = 0;
		y = 0;
		z = 0;
	}
}