
using System.IO.Abstractions.TestingHelpers;
using UnityEngine;
namespace StatefulInteraction.HandApply
{
	/// <summary>
	/// Broadcasts an action message to local chat.
	/// </summary>
	[CreateAssetMenu(fileName = "SendActionMessage", menuName = "Interaction/HandApply/ServerPerform/SendActionMessage")]
	public class SendActionMessage : BaseServerHandApply
	{
		[Tooltip("Message to show to performer.")]
		[SerializeField]
		private string performerMessage;

		[Tooltip("Message to show to other players. Use {performer} as placeholder for performer's name.")]
		[SerializeField]
		private string othersMessage;

		public override void ServerPerformInteraction(GameObject processorObject, global::HandApply interaction)
		{
			var finalOtherMessage = othersMessage.Replace("{performer}", interaction.Performer.ExpensiveName());
			Chat.AddActionMsgToChat(interaction.Performer, performerMessage, finalOtherMessage);
		}
	}
}