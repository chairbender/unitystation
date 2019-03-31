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

	/// <summary>
	/// Instance ID of the prefab in our prefab map so we can retrieve it to spawn / render results.
	/// </summary>
	/// <returns></returns>
	public readonly int PrefabInstanceID;

	private DevSpawnerDocument(string prefabName, int prefabInstanceId)
	{
		PrefabName = prefabName;
		PrefabInstanceID = prefabInstanceId;
	}

	/// <summary>
	/// Create a dev spawner document representing this prefab.
	/// </summary>
	/// <param name="prefab"></param>
	public static DevSpawnerDocument ForPrefab(GameObject prefab)
	{
		return new DevSpawnerDocument(prefab.name, prefab.GetInstanceID());
	}
}
