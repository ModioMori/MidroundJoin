using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace ModMidroundJoin {
	[BepInPlugin("gay.crf.modiomori.midroundjoin", "MidRoundJoin", "1.0.1")]
	public class MidroundJoinPlugin : BaseUnityPlugin {
		internal static ManualLogSource? Log;
		internal static Dictionary<uint, bool> DeleteCharacterOnce = new Dictionary<uint, bool>();

		private void Awake() {
			Log = Logger;
			Chainloader.ManagerObject.hideFlags = HideFlags.HideAndDontSave;
			Harmony.CreateAndPatchAll(typeof(HarmonyPatches));

			Logger.LogInfo($"Plugin MidroundJoin is loaded!");
		}
	}
}
