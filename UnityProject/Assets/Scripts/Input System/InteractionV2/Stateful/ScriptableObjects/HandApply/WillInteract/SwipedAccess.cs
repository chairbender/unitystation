
using UnityEngine;

/// <summary>
/// Checks if used object is an ID card with the indicated access.
/// </summary>
[CreateAssetMenu(fileName = "SwipedAccess", menuName = "Interaction/HandApply/WillInteract/SwipedAccess")]
public class SwipedAccess : BaseWillHandApply
{
	[Tooltip("Access required by the ID card in the performer's hand.")]
	[SerializeField]
	private Access requiredAccess;

	public override bool WillInteract(GameObject processorObject, HandApply interaction, NetworkSide side)
	{
		var used = interaction.HandObject;
		if (used == null) return false;

		var id = used.GetComponent<IDCard>();
		if (id == null) return false;

		return id.HasAccess(requiredAccess);
	}
}
