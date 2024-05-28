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

		[HarmonyPostfix, HarmonyPatch(typeof(NetworkRoomManager), "SceneLoadedForPlayer")]
		private static void FixBrokenCharacterOnMidroundJoin(NetworkConnectionToClient conn,
		                                                     GameObject roomPlayer) {
			NetworkManager NetMan = NetworkManager.singleton;
			if (NetMan != null && (NetMan.mode != NetworkManagerMode.Host &&
			                       NetMan.mode != NetworkManagerMode.ServerOnly))
				return;

			MultiplayerRoomPlayer RoomPlayer = roomPlayer.GetComponent<MultiplayerRoomPlayer>();
			if (RoomPlayer == null)
				return;
			if (conn.identity == null || conn.identity.gameObject == null)
				return;
			if (!MidroundJoinPlugin.DeleteCharacterOnce.ContainsKey(RoomPlayer.netId))
				return;

			GameObject PlayerCharacter = conn.identity.gameObject;
			PlayerMultiplayerInputManager InputManager =
			    PlayerCharacter.GetComponent<PlayerMultiplayerInputManager>();

			MidroundJoinPlugin.DeleteCharacterOnce.Remove(RoomPlayer.netId);
			InputManager.HandlePlayerDeath();
			NetworkServer.Destroy(PlayerCharacter.transform.Find("PlayerModelPhysics").gameObject);
			NetworkServer.Destroy(
			    PlayerCharacter.transform.Find("PlayerModelAnimation").gameObject);
		}
	}
}
