
using UnityEngine;
namespace StatefulInteraction.HandApply
{
	/// <summary>
	/// Switch the object to a different HandApplyState.
	/// </summary>
	[CreateAssetMenu(fileName = "ChangeState", menuName = "Interaction/HandApply/ServerPerform/ChangeState")]
	public class ChangeState : BaseServerHandApply
	{

		[Tooltip("State to change this object to. Must exist in the object's" +
		         " HandAppliable component's States.")]
		[SerializeField]
		private HandApplyState newState;

		public override void ServerPerformInteraction(GameObject processorObject, HandApply interaction)
		{
			var handAppliable = processorObject.GetComponent<HandAppliable>();
			handAppliable.ServerChangeState(newState);
		}
	}
}