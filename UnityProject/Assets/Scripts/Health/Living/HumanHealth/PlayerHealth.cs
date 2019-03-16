﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Provides central access to the Players Health
/// </summary>
public class PlayerHealth : LivingHealthBehaviour
{
	private PlayerMove playerMove;

	private PlayerNetworkActions playerNetworkActions;
	/// <summary>
	/// Cached register player
	/// </summary>
	private RegisterPlayer registerPlayer;

	//fixme: not actually set or modified. keep an eye on this!
	public bool serverPlayerConscious { get; set; } = true; //Only used on the server

	public override void OnStartClient()
	{
		playerNetworkActions = GetComponent<PlayerNetworkActions>();
		playerMove = GetComponent<PlayerMove>();
		registerPlayer = GetComponent<RegisterPlayer>();

		PlayerScript playerScript = GetComponent<PlayerScript>();
		//fixme: these are all workarounds to hide your spess dummy player. get rid of him
		if (playerScript.JobType == JobType.NULL)
		{
			foreach (Transform t in transform)
			{
				t.gameObject.SetActive(false);
			}
			ConsciousState = ConsciousState.DEAD;

			// Fixme: No more setting allowInputs on client:
			// When job selection screen is removed from round start
			// (and moved to preference system in lobby) then we can remove this
			playerMove.allowInput = false;
		}

		base.OnStartClient();
	}

	protected override void OnDeathActions()
	{
		if (CustomNetworkManager.Instance._isServer)
		{
			PlayerNetworkActions pna = gameObject.GetComponent<PlayerNetworkActions>();
			PlayerMove pm = gameObject.GetComponent<PlayerMove>();

			ConnectedPlayer player = PlayerList.Instance.Get(gameObject);

			string killerName = "Stressful work";
			if (LastDamagedBy != null)
			{
				killerName = PlayerList.Instance.Get(LastDamagedBy).Name;
			}

			string playerName = player.Name ?? "dummy";
			if (killerName == playerName)
			{
				PostToChatMessage.Send(playerName + " commited suicide", ChatChannel.System); //Killfeed
			}
			else if (killerName.EndsWith(playerName))
			{
				// chain reactions
				PostToChatMessage.Send(
					playerName + " screwed himself up with some help (" + killerName + ")",
					ChatChannel.System); //Killfeed
			}
			else
			{
				PlayerList.Instance.UpdateKillScore(LastDamagedBy, gameObject);
			}
			pna.DropItem("rightHand");
			pna.DropItem("leftHand");

			if (isServer)
			{
				EffectsFactory.Instance.BloodSplat(transform.position, BloodSplatSize.large);
			}

			PlayerDeathMessage.Send(gameObject);
			//TODO: Refactor this stuff as ghost will be created as a separate object
			/*
			//syncvars for everyone
			pm.IsGhost = true;
			pm.allowInput = true;
			//consider moving into PlayerDeathMessage.Process()
			pna.RpcSpawnGhost();
			RpcPassBullets(gameObject);

			//FIXME Remove for next demo
			pna.RespawnPlayer(10);
			*/
		}
	}

	[ClientRpc]
	private void RpcPassBullets(GameObject target)
	{
		foreach (BoxCollider2D comp in target.GetComponents<BoxCollider2D>())
		{
			if (!comp.isTrigger)
			{
				comp.enabled = false;
			}
		}
	}

	///     make player unconscious upon crit
	protected override void OnConsciousStateChange( ConsciousState oldState, ConsciousState newState )
	{
		if ( isServer )
		{
			playerNetworkActions.OnConsciousStateChanged(oldState, newState);
		}

		if (newState != ConsciousState.CONSCIOUS)
		{
			registerPlayer.LayDown();
		}
		else
		{
			registerPlayer.GetUp();
		}

	}
}