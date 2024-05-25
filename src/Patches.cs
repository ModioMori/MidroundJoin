using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine;
using MoveClasses;
using Utils;

namespace ModMidroundJoin {
	[HarmonyPatch]
	internal static class HarmonyPatches {
		[HarmonyPrefix, HarmonyPatch(typeof(GameSettingsManagerMultiplayer), "SetJoinable")]
		private static bool JoinableHook(bool joinable) {
			// If we're setting joinable to false, then skip the function.
			return joinable;
		}

		[HarmonyTranspiler, HarmonyPatch(typeof(NetworkRoomManager), "OnServerConnect")]
		private static IEnumerable<CodeInstruction> ConnectHook(
		    IEnumerable<CodeInstruction> Instructions) {
			// Removes check for if current scene is room scene on connection.
			List<CodeInstruction> NewInstructions = new List<CodeInstruction>(Instructions);
			NewInstructions.RemoveRange(0, 11);

			return NewInstructions.AsEnumerable();
		}

		[HarmonyTranspiler, HarmonyPatch(typeof(NetworkRoomManager), "OnServerAddPlayer")]
		private static IEnumerable<CodeInstruction> AddPlayerHook(
		    IEnumerable<CodeInstruction> Instructions) {
			// Removes check for if current scene is room scene on player creation.
			List<CodeInstruction> NewInstructions = new List<CodeInstruction>(Instructions);
			NewInstructions.RemoveRange(6, 4);

			return NewInstructions.AsEnumerable();
		}

		[HarmonyPostfix,
		 HarmonyPatch(typeof(NetworkServer), "AddPlayerForConnection",
		              new System.Type[] { typeof(NetworkConnectionToClient), typeof(GameObject) })]
		private static void PendingPlayerHook(NetworkConnectionToClient conn, GameObject player) {
			if (SceneManager.GetActiveScene().name == "LobbyMultiplayer")
				return;

			NetworkRoomManager RoomManager = (NetworkRoomManager)NetworkRoomManager.singleton;
			MultiplayerRoomPlayer RoomPlayer = player.GetComponent<MultiplayerRoomPlayer>();
			if (RoomPlayer == null)
				return;

			MidroundJoinPlugin.DeleteCharacterOnce[RoomPlayer.netId] = true;
			RoomManager.SceneLoadedForPlayer(conn, player);
		}

		[HarmonyPrefix,
		 HarmonyPatch(typeof(MultiplayerRoomManager), "OnRoomServerSceneLoadedForPlayer")]
		private static void FixMovesetEquipmentHook(NetworkConnectionToClient conn,
		                                            GameObject roomPlayer, GameObject gamePlayer) {
			MultiplayerRoomPlayer RoomPlayerComponent =
			    roomPlayer.GetComponent<MultiplayerRoomPlayer>();
			if (RoomPlayerComponent.selectedMoveSet != null &&
			    (RoomPlayerComponent.selectedEquipment != null &&
			     RoomPlayerComponent.selectedEquipment.Count > 0))
				return;

			foreach (MoveSet Set in MoveSetHelpers.MoveSets) {
				if (Set.name.Contains("Bardiche")) {
					RoomPlayerComponent.selectedMoveSet = Set;
					RoomPlayerComponent.selectedEquipment = Set.defaultEquipment;
					break;
				}
			}
		}

		[HarmonyPostfix, HarmonyPatch(typeof(PlayerMultiplayerInputManager), "Start")]
		private static void FixBrokenCharacterOnMidroundJoin(
		    PlayerMultiplayerInputManager __instance, GameObject ___playerCharacter) {
			if (__instance.multiplayerRoomPlayer == null)
				return;

			NetworkManager NetMan = NetworkManager.singleton;
			if (NetMan != null && (NetMan.mode != NetworkManagerMode.Host &&
			                       NetMan.mode != NetworkManagerMode.ServerOnly))
				return;

			if (!MidroundJoinPlugin.DeleteCharacterOnce.ContainsKey(
			        __instance.multiplayerRoomPlayer.netId))
				return;

			MidroundJoinPlugin.DeleteCharacterOnce.Remove(__instance.multiplayerRoomPlayer.netId);
			__instance.HandlePlayerDeath();
			Object.Destroy(___playerCharacter.transform.Find("PlayerModelPhysics").gameObject);
			Object.Destroy(___playerCharacter.transform.Find("PlayerModelAnimation").gameObject);
		}
	}
}
