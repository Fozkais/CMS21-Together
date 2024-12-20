using System;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public struct ModPaintData
{
	public float metal;
	public float roughness;
	public float clearCoat;
	public float normalStrenght;
	public float fresnel;
	public float p_metal;
	public float p_roughness;
	public float p_clearCoat;
	public float p_normalStrenght;
	public float p_fresnel;

	public ModPaintData()
	{
	}

	public ModPaintData(PaintData data)
	{
		metal = data.metal;
		roughness = data.roughness;
		clearCoat = data.clearCoat;
		normalStrenght = data.normalStrength;
		fresnel = data.fresnel;

		p_metal = data.Metal;
		p_roughness = data.Roughness;
		p_clearCoat = data.ClearCoat;
		p_normalStrenght = data.NormalStrength;
		p_fresnel = data.Fresnel;
	}

	public PaintData ToGame(ModPaintData data)
	{
		var paintData = new PaintData();
		paintData.metal = data.metal;
		paintData.roughness = data.roughness;
		paintData.clearCoat = data.clearCoat;
		paintData.normalStrength = data.normalStrenght;
		paintData.fresnel = data.fresnel;

		paintData.Metal = data.p_metal;
		paintData.Roughness = data.p_roughness;
		paintData.ClearCoat = data.p_clearCoat;
		paintData.NormalStrength = data.p_normalStrenght;
		paintData.Fresnel = data.p_fresnel;
		return paintData;
	}

	public PaintData ToGame()
	{
		var paintData = new PaintData();
		paintData.metal = metal;
		paintData.roughness = roughness;
		paintData.clearCoat = clearCoat;
		paintData.normalStrength = normalStrenght;
		paintData.fresnel = fresnel;

		paintData.Metal = p_metal;
		paintData.Roughness = p_roughness;
		paintData.ClearCoat = p_clearCoat;
		paintData.NormalStrength = p_normalStrenght;
		paintData.Fresnel = p_fresnel;
		return paintData;
	}
}