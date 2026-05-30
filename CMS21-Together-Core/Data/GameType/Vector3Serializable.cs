using System;

namespace CMS21_Together_Core.Data.GameType
{
	[Serializable]
	public class Vector3Serializable
	{
		public float X;
		public float Y;
		public float Z;

		public Vector3Serializable() { }

		public Vector3Serializable(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}