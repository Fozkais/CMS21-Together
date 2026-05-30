using System;

namespace CMS21_Together_Core.Data.GameType
{
	[Serializable]
	public class QuaternionSerializable
	{
		public float X;
		public float Y;
		public float Z;
		public float W;

		public QuaternionSerializable() { }

		public QuaternionSerializable(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
	}
}