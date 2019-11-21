using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Mirror;

/// <summary>
/// The main metal sheet component
/// </summary>
[RequireComponent(typeof(Stackable))]
public class Metal : NetworkBehaviour, IInteractable<HandActivate>
{
	public GameObject girderPrefab;
	private Stackable stackable;

	private void Awake()
	{
		stackable = GetComponent<Stackable>();
	}

	public void ServerPerformInteraction(HandActivate interaction)
	{
		startBuilding(interaction);
	}

	[Server]
	private void startBuilding(HandActivate interaction)
	{
		var progressFinishAction = new ProgressCompleteAction(() => BuildGirder(interaction));
		UIManager.ServerStartProgress(ProgressAction.Construction, interaction.Performer.TileWorldPosition().To3Int(), 5f, progressFinishAction, interaction.Performer);
	}

	[Server]
	private void BuildGirder(HandActivate interaction)
	{
		Spawn.ServerPrefab(girderPrefab, interaction.Performer.TileWorldPosition().To3Int());
		stackable.ServerConsume(1);
	}

}