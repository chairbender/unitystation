
using UnityEngine;

/// <summary>
/// Util class for spawning commonly-spawned items without needing to save their
/// prefab references
/// </summary>
public static class ItemFactory
{
	private static GameObject metalPrefab;
	private static GameObject glassShardPrefab;

	private static bool hasInit = false;

	private static void EnsureInit()
	{
		if (hasInit) return;
		metalPrefab = Resources.Load<GameObject>("Metal");
		glassShardPrefab = Resources.Load("GlassShard") as GameObject;
		hasInit = true;
	}

	private static void Spawn(int amount, GameObject prefab, Vector2Int tileWorldPosition, Transform parent)
	{
		for (int i = 0; i < amount; i++)
		{
			PoolManager.PoolNetworkInstantiate(prefab, tileWorldPosition.To3Int(), parent);
		}
	}

	public static void SpawnMetal(int amount, Vector2Int tileWorldPosition, Transform parent)
	{
		EnsureInit();
		Spawn(amount, metalPrefab, tileWorldPosition, parent);
	}

	public static void SpawnGlassShard(int amount, Vector2Int tileWorldPosition, Transform parent)
	{
		EnsureInit();
		Spawn(amount, glassShardPrefab, tileWorldPosition, parent);
	}

}
