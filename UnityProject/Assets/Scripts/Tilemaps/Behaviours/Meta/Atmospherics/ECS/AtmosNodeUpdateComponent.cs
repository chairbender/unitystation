
using Unity.Entities;
using UnityEngine;

public struct AtmosNodeUpdateComponent : IComponentData
{
	public Vector2Int localPosition;
	public float temperature;
	public float oxygen;
	public float plasma;
}
