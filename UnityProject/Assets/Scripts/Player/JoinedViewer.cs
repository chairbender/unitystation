﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Mirror;

/// <summary>
/// This is the Viewer object for a joined player.
/// Once they join they will have local ownership of this object until a job is determined
/// and then they are spawned as player entity
/// </summary>
public class JoinedViewer : NetworkBehaviour
{
	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		if (!PlayerPrefs.HasKey(PlayerPrefKeys.ClientID))
		{
			PlayerPrefs.SetString(PlayerPrefKeys.ClientID, "");
			PlayerPrefs.Save();
		}

		CmdServerSetupPlayer(PlayerPrefs.GetString(PlayerPrefKeys.ClientID), PlayerManager.CurrentCharacterSettings.username);
	}

	[Command]
	private void CmdServerSetupPlayer(string clientID, string username)
	{
		var connPlayer = new ConnectedPlayer
		{
			Connection = connectionToClient,
			GameObject = gameObject,
			Username = username,
			Job = JobType.NULL,
			ClientId = clientID
		};
		//Add player to player list
		PlayerList.Instance.Add(connPlayer);

		// Sync all player data and the connected player count
		CustomNetworkManager.Instance.SyncPlayerData(gameObject);
		UpdateConnectedPlayersMessage.Send();

		// If they have a player to rejoin send the client the player to rejoin, otherwise send a null gameobject.
		var loggedOffPlayer = PlayerList.Instance.TakeLoggedOffPlayer(clientID);
		if (loggedOffPlayer == null)
		{
			//This is the players first time connecting to this round, assign them a Client ID;
			clientID = System.Guid.NewGuid().ToString();
			connPlayer.ClientId = clientID;
		}

		// Only sync the pre-round countdown if it's already started
		if (GameManager.Instance.CurrentRoundState == RoundState.PreRound)
		{
			if (GameManager.Instance.waitForStart)
			{
				TargetSyncCountdown(connectionToClient, GameManager.Instance.waitForStart, GameManager.Instance.startTime);
			}
			else
			{
				GameManager.Instance.CheckPlayerCount();
			}
		}

		TargetLocalPlayerSetupPlayer(connectionToClient, loggedOffPlayer, clientID, GameManager.Instance.CurrentRoundState);
	}

	[TargetRpc]
	private void TargetLocalPlayerSetupPlayer(NetworkConnection target, GameObject loggedOffPlayer,
		string serverClientID, RoundState roundState)
	{
		PlayerPrefs.SetString(PlayerPrefKeys.ClientID, serverClientID);
		PlayerPrefs.Save();

		PlayerManager.SetViewerForControl(this);
		UIManager.ResetAllUI();

		// Determine what to do depending on the state of the round
		switch (roundState)
		{
			case RoundState.PreRound:
				// Round hasn't yet started, give players the pre-game screen
				UIManager.Display.SetScreenForPreRound();
				break;
			// case RoundState.Started:
			// TODO spawn a ghost if round has already ended?
			default:
				// If player is joining for the first time let them pick a job, otherwise rejoin character.
				if (loggedOffPlayer == null)
				{
					UIManager.Display.SetScreenForJobSelect();
				}
				else
				{
					//MIRRORUPGRADE: Doesn't appear to be needed anymore
					//loggedOffPlayer.GetComponent<NetworkIdentity>().Loc();
					CmdRejoin(loggedOffPlayer);
				}
				break;
		}
	}

	/// <summary>
	/// At the moment players can choose their jobs on round start:
	/// </summary>
	[Command]
	public void CmdRequestJob(JobType jobType, CharacterSettings characterSettings)
	{
		PlayerSpawn.ServerSpawnPlayer(this, GameManager.Instance.GetRandomFreeOccupation(jobType),
			characterSettings);
	}

	/// <summary>
	/// Asks the server to let the client rejoin into a logged off character.
	/// </summary>
	/// <param name="loggedOffPlayer">The character to be rejoined into.</param>
	[Command]
	public void CmdRejoin(GameObject loggedOffPlayer)
	{
		PlayerSpawn.ServerRejoinPlayer(this, loggedOffPlayer);
	}

	/// <summary>
	/// Tells the client to start the countdown if it's already started
	/// </summary>
	[TargetRpc]
	private void TargetSyncCountdown(NetworkConnection target, bool started, float countdownTime)
	{
		Logger.Log("Syncing countdown!", Category.Round);
		UIManager.Instance.displayControl.preRoundWindow.GetComponent<GUI_PreRoundWindow>().SyncCountdown(started, countdownTime);
	}
}