
using System;
using Unity.Entities;
using Unity.Jobs;

/// <summary>
/// Updates atmos nodes in response to commands
/// </summary>
public class AtmosNodeCommandSystem : JobComponentSystem
{


	protected override void OnCreate()
	{
		base.OnCreate();
	}

	struct UpdateAtmosNodesJob : IJobForEachWithEntity<AtmosNodeUpdateComponent>
	{
		public void Execute(Entity entity, int index, ref AtmosNodeUpdateComponent c0)
		{
			throw new NotImplementedException();
		}
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		var job = new UpdateAtmosNodesJob();
		return job.Schedule(this, inputDeps);
	}
}
