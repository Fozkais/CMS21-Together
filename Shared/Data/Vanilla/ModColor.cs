using System;
using UnityEngine;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModColor
{
	public float r;
	public float g;
	public float b;
	public float a;

	public ModColor() { }
	
	public ModColor(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public ModColor(Color color)
	{
		r = Mathf.Round(color.r * 1000f) / 1000f;
		g = Mathf.Round(color.g * 1000f) / 1000f;
		b = Mathf.Round(color.b * 1000f) / 1000f;
		a = Mathf.Round(color.a * 1000f) / 1000f;
	}

	public Color ToGame()
	{
		return new Color(r, g, b, a);
	}

	public bool IsDifferent(Color color, float tolerance = 0.03f)
	{
		return Mathf.Abs(r - color.r) > tolerance ||
		       Mathf.Abs(g - color.g) > tolerance ||
		       Mathf.Abs(b - color.b) > tolerance ||
		       Mathf.Abs(a - color.a) > tolerance;
	}
}