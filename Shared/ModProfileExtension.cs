using System;
using System.Collections.Generic;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using Newtonsoft.Json;

namespace CMS21Together.Shared;

[Serializable]
public class ModProfileExtension
{
	public string MagicWord = "CMS21-TOGETHER";
	public int ModVersion = 1;

	public string Name = "";
	public Gamemode SelectedGamemode = Gamemode.Normal;
	public List<PlayerInfo> PlayerInfos = new();
	public ModEngineStand AdditionnalStand;
	
	public byte[] ToBytes() => System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
    
	public static ModProfileExtension FromBytes(byte[] data) 
		=> JsonConvert.DeserializeObject<ModProfileExtension>(System.Text.Encoding.UTF8.GetString(data));
}