using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
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
		              new Type[] { typeof(NetworkConnectionToClient), typeof(GameObject) })]
		private static void PendingPlayerHook(NetworkConnectionToClient conn, GameObject player) {
			if (SceneManager.GetActiveScene().name != "LobbyMultiplayer") {
				NetworkRoomManager.PendingPlayer PendingPlayer;
				PendingPlayer.conn = conn;
				PendingPlayer.roomPlayer = player;

				MultiplayerRoomManager RoomManager =
				    (MultiplayerRoomManager)NetworkRoomManager.singleton;
				RoomManager.pendingPlayers.Add(PendingPlayer);
			}
		}

		[HarmonyPrefix,
		 HarmonyPatch(typeof(MultiplayerRoomManager), "OnRoomServerSceneLoadedForPlayer")]
		private static void FixMovesetEquipmentHook(NetworkConnectionToClient conn,
		                                            GameObject roomPlayer, GameObject gamePlayer) {
			MultiplayerRoomPlayer RoomPlayerComponent =
			    roomPlayer.GetComponent<MultiplayerRoomPlayer>();
			if (RoomPlayerComponent.selectedMoveSet != null &&
			    (RoomPlayerComponent.selectedEquipment != null &&
			     RoomPlayerComponent.selectedEquipment.Count > 0)) {
				return;
			}

			foreach (MoveSet Set in MoveSetHelpers.MoveSets) {
				MidroundJoinPlugin.Log!.LogInfo(Set.name);
				if (Set.name.Contains("Bardiche")) {
					RoomPlayerComponent.selectedMoveSet = Set;
					RoomPlayerComponent.selectedEquipment = Set.defaultEquipment;
					break;
				}
			}
		}
	}
}
