using System;
using Atmospherics;
using Tilemaps.Behaviours.Meta;
using Unity.Entities;
using UnityEngine;


public class ECSAtmosManager : MonoBehaviour
{
	private UniqueQueue<MetaDataNode> updatedNodes;
	private AtmosNodeCommandSystem atmosNodeCommandSystem;
	private EntityArchetype atmosNodeArchetype;
	private EntityArchetype atmosUpdateArchetype;
	private EntityManager entityManager;

	private void Awake()
	{
		updatedNodes = GetComponent<AtmosSystem>().UpdatedNodes;
		entityManager = World.Active.EntityManager;
		atmosNodeCommandSystem = World.Active.GetOrCreateSystem<AtmosNodeCommandSystem>();

		atmosNodeArchetype = entityManager.CreateArchetype(ComponentType.ReadWrite<AtmosNodeComponent>());
		atmosUpdateArchetype = entityManager.CreateArchetype(ComponentType.ReadOnly<AtmosNodeUpdateComponent>());
	}

	private void Update()
	{
		//check if there's any gas updates from atmos thread and
		//update entities in our system if so
		int count = updatedNodes.Count;
		for (int i = 0; i < count; i++)
		{
			if (updatedNodes.TryDequeue(out MetaDataNode node))
			{
				//create the command
				var updateCommand = entityManager.CreateEntity(atmosUpdateArchetype);
				entityManager.SetComponentData(updateCommand,
					new AtmosNodeComponent(node.Position.To2Int(), node.GasMix.Temperature,
						node.GasMix.Gases[Gas.Oxygen], node.GasMix.Gases[Gas.Plasma]));

			}
		}
	}
}
