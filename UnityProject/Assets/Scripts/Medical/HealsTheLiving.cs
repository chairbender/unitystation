using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Component which allows this object to be applied to a living thing, healing it.
/// </summary>
[RequireComponent(typeof(Stackable))]
public class HealsTheLiving : MonoBehaviour, ICheckedInteractable<HandApply>
{
	public DamageType healType;
	private Stackable stackable;

	private void Awake()
	{
		stackable = GetComponent<Stackable>();
	}

	public bool WillInteract(HandApply interaction, NetworkSide side)
	{
		if (!DefaultWillInteract.Default(interaction, side)) return false;
		//can only be applied to LHB
		if (!Validations.HasComponent<LivingHealthBehaviour>(interaction.TargetObject)) return false;
		return true;
	}

	public void ServerPerformInteraction(HandApply interaction)
	{
		var LHB = interaction.TargetObject.GetComponent<LivingHealthBehaviour>();
		if (LHB.IsDead)
		{
			return;
		}
		var targetBodyPart = LHB.FindBodyPart(interaction.TargetBodyPart);
		if (targetBodyPart.GetDamageValue(healType) > 0)
		{
			if (interaction.TargetObject != interaction.Performer)
			{
				ApplyHeal(targetBodyPart);
			}
			else
			{
				SelfHeal(interaction.Performer, targetBodyPart);
			}
		}
	}

	[Server]
	private void ApplyHeal(BodyPartBehaviour targetBodyPart)
	{
		targetBodyPart.HealDamage(40, healType);
		stackable.ServerConsume(1);
	}

	[Server]
	private void SelfHeal(GameObject originator, BodyPartBehaviour targetBodyPart)
	{
		var progressFinishAction = new ProgressCompleteAction(() => ApplyHeal(targetBodyPart));
		UIManager.ServerStartProgress(ProgressAction.SelfHeal, originator.transform.position.RoundToInt(), 5f, progressFinishAction, originator);
	}
}
