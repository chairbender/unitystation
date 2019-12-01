
using UnityEngine;

/// <summary>
/// Default WillInteract logic for hand apply
/// </summary>
[CreateAssetMenu(fileName = "DefaultWillHandApply", menuName = "Interaction/HandApply/WillInteract/DefaultWillHandApply")]
public class DefaultWillHandApply : BaseWillHandApply
{
	public override bool WillInteract(GameObject processorObject, HandApply interaction, NetworkSide side)
	{
		return DefaultWillInteract.HandApply(interaction, side);
	}
}
