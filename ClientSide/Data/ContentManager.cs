using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net.Http;
using CMS21Together.Shared.Data;

namespace CMS21Together.ClientSide.Data;

//[RegisterTypeInIl2Cpp]
public class ContentManager
{
	public static ContentManager Instance;

	public string gameVersion { get; private set; }
	public ReadOnlyDictionary<string, bool> ownedContents { get; private set; }

	public void Initialize()
	{
		if (ownedContents != null) return;

		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			MelonLogger.Msg("Instance already exists, destroying object!");
		}

		GetGameVersion();
		CheckContent();
	}

	private void GetGameVersion()
	{
		if (ownedContents != null) return;

		gameVersion = GameObject.Find("GameVersion").GetComponent<Text>().text;
	}

	protected void CheckContent()
	{
		if (ownedContents != null) return;

		ownedContents = new ReadOnlyDictionary<string, bool>(ApiCalls.API_M3());
	}
	
	public VersionStatus  IsNewVersionAvailable(string versionName)
	{
		try
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("User-Agent", "MyModChecker");
				string url = $"https://api.github.com/repos/Fozkais/CMS21-Together/releases/latest";

				string json = client.GetStringAsync(url).Result;
				
				var m = Regex.Match(json, @"""name""\s*:\s*""([^""]+)""");
				if (!m.Success)
				{
					MelonLogger.Msg("VersionChecker : could not find 'name' field on response");
					return VersionStatus.Latest;
				}
				string releaseName = m.Groups[1].Value;
				string remoteVersion = releaseName.Split(' ')[0];

				Version local = new Version(versionName);
				Version remote = new Version(remoteVersion);

				int cmp = local.CompareTo(remote);
				if (cmp < 0)
					return VersionStatus.Outdated;
				if (cmp > 0)
					return VersionStatus.Dev;
				return VersionStatus.Latest;
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Msg("VersionChecker exception : " + ex);
			return VersionStatus.Latest;
		}
	}
}