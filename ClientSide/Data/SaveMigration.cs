using System;
using System.IO;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using Newtonsoft.Json;

namespace CMS21Together.ClientSide.Data;

// Reads pre-0.4.17 save metadata ("Mods/togetherMod/saves/save_N.cms21mp") for slots that don't
// carry the new in-band payload yet, so old saves show up correctly after updating. Purely
// read-only: the legacy JSON file and the vanilla profileN.cms21(b) file are never renamed, moved
// or modified, so both keep working exactly as before if the player goes back to an older mod
// version and continues playing that save there.
public static class SaveMigration
{
	private const string OLD_SAVE_FOLDER = @"Mods\togetherMod\saves";

	public static bool TryLoadLegacyExtension(int index, out ModProfileExtension extension)
	{
		extension = null;
		try
		{
			var path = Path.Combine(OLD_SAVE_FOLDER, $"save_{index}.cms21mp");
			if (!File.Exists(path)) return false;

			ModSaveData oldSave = JsonConvert.DeserializeObject<ModSaveData>(File.ReadAllText(path));
			if (oldSave == null) return false;

			extension = new ModProfileExtension
			{
				Name = oldSave.Name,
				SelectedGamemode = oldSave.selectedGamemode,
				PlayerInfos = oldSave.playerInfos ?? new(),
				AdditionnalStand = oldSave.additionnalStand
			};
			return true;
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"[SaveMigration] Failed to read legacy save data for slot {index}: {ex.Message}");
			extension = null;
			return false;
		}
	}
}
