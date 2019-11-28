
using UnityEngine;
namespace StatefulInteraction.HandApply
{
	/// <summary>
	/// Despawns an object or consumes a defined amount of it.
	/// </summary>
	public class Consume : BaseServerHandApply
	{

		[Tooltip("Which participant should be consumed?")]
		[SerializeField]
		private InteractionParticipant participant;

		[Tooltip("Amount of the participant to consume. Set to -1 to despawn it regardless of its stacked quantity.")]
		[SerializeField]
		private int amount;

		public override void ServerPerformInteraction(GameObject processorObject, HandApply interaction)
		{
			GameObject chosenObject = null;
			switch (participant)
			{
				case InteractionParticipant.Performer:
					Logger.LogErrorFormat("Config error in {0}, cannot consume participant.", Category.Interaction, name);
					return;
				case InteractionParticipant.Target:
					chosenObject = interaction.TargetObject;
					break;
				case InteractionParticipant.UsedObject:
					chosenObject = interaction.UsedObject;
					break;
				default:
					Logger.LogErrorFormat("Coding error in {0}, unrecognized participant type.", Category.Interaction, name);
					return;
			}

			if (chosenObject == null) return;

			if (amount == -1)
			{
				Despawn.ServerSingle(chosenObject);
			}

			var stackable = chosenObject.GetComponent<Stackable>();
			if (stackable == null)
			{
				Despawn.ServerSingle(chosenObject);
			}
			else
			{
				stackable.ServerConsume(amount);
			}
		}
	}
}