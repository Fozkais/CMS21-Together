using System;

namespace CMS21_Together_Core.Data;

[Serializable]
public class QuaternionSerializable
{
	public float x;
	public float y;
	public float z;
	public float w;

	public QuaternionSerializable()
	{
		x = 0;
		y = 0;
		z = 0;
		w = 0;
	}
}