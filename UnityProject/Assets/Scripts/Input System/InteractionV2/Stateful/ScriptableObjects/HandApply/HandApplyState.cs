
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General purpose hand apply state, allows defining all of the logic that should happen by referencing
/// other assets.
/// </summary>
[CreateAssetMenu(fileName = "HandApplyState", menuName = "Interaction/HandApply/HandApplyState")]
public class HandApplyState : BaseHandApplyState
{

	[Tooltip("All of the WillInteract checks to perform in this state.")]
	[SerializeField]
	private List<BaseWillHandApply> willHandApply;


	public override bool WillInteract(GameObject processorObject, HandApply interaction, NetworkSide side)
	{
		throw new System.NotImplementedException();
	}

	public override void ServerPerformInteraction(GameObject processorObject, HandApply interaction)
	{
		throw new System.NotImplementedException();
	}


	//currently no client prediction
	public override void ClientPredictInteraction(GameObject processorObject, HandApply interaction) { }
	public override void ServerRollbackClient(GameObject processorObject, HandApply interaction) { }
}
