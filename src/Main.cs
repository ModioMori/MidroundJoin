using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ModMidroundJoin {
	[BepInPlugin("gay.crf.modiomori.midroundjoin", "MidRoundJoin", "1.0.0")]
	public class MidroundJoinPlugin : BaseUnityPlugin {
		public static ManualLogSource? Log;
		private void Awake() {
			Log = Logger;
			Chainloader.ManagerObject.hideFlags = HideFlags.HideAndDontSave;
			Harmony.CreateAndPatchAll(typeof(HarmonyPatches));

			Logger.LogInfo($"Plugin MidroundJoin is loaded!");
		}
	}
}
