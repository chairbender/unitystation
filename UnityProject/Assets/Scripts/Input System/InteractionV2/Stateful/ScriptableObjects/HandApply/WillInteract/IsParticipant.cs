
using UnityEngine;

/// <summary>
/// Checks if this object is the indicated kind of participant of the interaction (target or used object)
/// </summary>
public class IsParticipant : BaseWillHandApply
{

	[Tooltip("Should this object be the target of the interaction (true), or the " +
	         "used object (false)?")]
	[SerializeField]
	private InteractionParticipant participant;

	public override bool WillInteract(GameObject processorObject, HandApply interaction, NetworkSide side)
	{
		if (participant == InteractionParticipant.Target) return processorObject == interaction.TargetObject;
		else if (participant == InteractionParticipant.UsedObject) return processorObject == interaction.UsedObject;
		//performer never processes an interaction
		return false;
	}
}
