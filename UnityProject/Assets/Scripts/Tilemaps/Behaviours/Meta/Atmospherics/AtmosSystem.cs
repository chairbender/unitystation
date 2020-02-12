﻿using System.Threading;
using Atmospherics;
using Tilemaps.Behaviours.Meta;
using UnityEngine;

public class AtmosSystem : SubsystemBehaviour
{
	public override void Initialize()
	{
		BoundsInt bounds = metaTileMap.GetBounds();

		foreach (Vector3Int position in bounds.allPositionsWithin)
		{
			MetaDataNode node = metaDataLayer.Get(position, false);

			node.GasMix = new GasMix( (node.IsRoom||node.IsOccupied) ? GasMixes.Air : GasMixes.Space );
		}
	}

	public override void UpdateAt(Vector3Int localPosition)
	{
		AtmosThread.Enqueue(metaDataLayer.Get(localPosition));
	}

	public UniqueQueue<MetaDataNode> UpdatedNodes => AtmosThread.UpdatedNodes;
}