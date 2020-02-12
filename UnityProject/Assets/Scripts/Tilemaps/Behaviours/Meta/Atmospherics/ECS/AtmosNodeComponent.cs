
//an individual tile in the atmos system

using Atmospherics;
using Unity.Entities;
using UnityEngine;

public struct AtmosNodeComponent : IComponentData
{
	public Vector2Int localPosition;
	public float temperature;
	public float oxygen;
	public float plasma;

	public AtmosNodeComponent(Vector2Int localPosition, float temperature, float oxygen, float plasma)
	{
		this.localPosition = localPosition;
		this.temperature = temperature;
		this.oxygen = oxygen;
		this.plasma = plasma;
	}
}
