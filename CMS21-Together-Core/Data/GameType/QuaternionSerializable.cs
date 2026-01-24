using System;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class QuaternionSerializable(float x, float y, float z, float w)
{
	public float X = x;
	public float Y = y;
	public float Z = z;
	public float W = w;

	public QuaternionSerializable() : this(0, 0, 0, 0) { }
}