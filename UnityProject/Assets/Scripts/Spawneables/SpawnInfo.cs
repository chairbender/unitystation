using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Holds the details of how an object was spawned (except for the object's initial state)
/// </summary>
public class SpawnInfo
{
	/// <summary>
	/// World position the object will be spawned at
	/// </summary>
	public readonly Vector2Int WorldPosition;

	private SpawnInfo(Vector2Int worldPosition)
	{
		WorldPosition = worldPosition;
	}

	public static SpawnInfo AtPosition(Vector2Int roundToInt)
	{
		return new SpawnInfo(roundToInt);
	}
}
