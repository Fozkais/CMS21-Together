using System;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class Vector3Serializable(float x, float y, float z)
{
	public float X = x;
	public float Y = y;
	public float Z = z;

	public Vector3Serializable() : this(0, 0, 0) { }
}