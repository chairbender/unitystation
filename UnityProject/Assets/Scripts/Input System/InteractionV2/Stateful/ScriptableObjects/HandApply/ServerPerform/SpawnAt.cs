
using UnityEngine;
namespace StatefulInteraction.HandApply
{
	/// <summary>
	/// Spawns something at a particular location.
	/// </summary>
	public class SpawnAt : BaseServerHandApply
	{

		[Tooltip("Where to spawn it.")]
		[SerializeField]
		private InteractionParticipant location;

		[Tooltip("Prefab to spawn.")]
		[SerializeField]
		private GameObject prefab;

		[Tooltip("Amount to spawn.")]
		[SerializeField]
		private int amount = 1;

		public override void ServerPerformInteraction(GameObject processorObject, HandApply interaction)
		{
			switch (location)
			{
				case InteractionParticipant.Performer:
				case InteractionParticipant.UsedObject:
					Spawn.ServerPrefab(prefab, SpawnDestination.At(interaction.Performer), amount);
					break;
				case InteractionParticipant.Target:
					Spawn.ServerPrefab(prefab, SpawnDestination.At(interaction.TargetObject), amount);
					break;
			}
		}
	}
}