using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// POCO representing something spawnable, indexed into Lucene to support searching for spawnable items.
/// </summary>
public class DevSpawnerDocument
{
	/// <summary>
	/// Name of the prefab (without .prefab)
	/// </summary>
	public readonly string PrefabName;

	private DevSpawnerDocument(string prefabName)
	{
		PrefabName = prefabName;
	}

	/// <summary>
	/// Create a dev spawner document representing this prefab.
	/// </summary>
	/// <param name="prefab"></param>
	public static DevSpawnerDocument ForPrefab(GameObject prefab)
	{
		return new DevSpawnerDocument(prefab.name);
	}
}
