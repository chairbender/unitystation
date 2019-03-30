using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Provides the general-purpose ability to create instances of prefabs, network synced, while using object pooling.
/// </summary>
public class PoolManager : NetworkBehaviour
{
	public static PoolManager Instance;

	private Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();
	/*
	* Use this function for general purpose GameObject instantiation. It will instantiate the
	* a pooled instance immediately. If it doesn't find a pooled instance, it uses GetInstanceInactive()
	* to make a new one, and immediately instantiates and activates that. If the instance matches one already
	* in the pool (for example, one obtained from GetInstanceInactive), it just instantiates it.
	*/

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Spawn the item and ensures it is synced over the network
	/// </summary>
	/// <param name="prefab">Prefab to spawn an instance of. This is intended to be made to work for pretty much any prefab, but don't
	/// be surprised if it doesn't as there are LOTS of prefabs in the game which all have unique behavior for how they should spawn. If you are trying
	/// to instantiate something and it isn't properly setting itself up, check to make sure each component that needs to set something up has
	/// properly implemented a BeSpawned method.</param>
	/// <param name="clone">GameObject which is also an instance of prefab. This will be broadcast to each component of
	/// the newly-spawned object via BeSpawned(clone). It is up to each component to clone its state from this
	/// gameobject. If you're trying to clone something and it isn't working, check that each component that needs to set
	/// its state has properly implemented a BeSpawned method.</param>
	/// <param name="position">world position to appear at. Defaults to HiddenPos (hidden / invisible)</param>
	/// <param name="rotation">rotation to spawn with, defaults to Quaternion.identity</param>
	/// <param name="parent">Parent to spawn under, defaults to no parent. THIS SHOULD RARELY BE NULL! Most things
	/// should always be spawned under the Objects transform in their matrix.</param>
	/// <returns>the newly created GameObject</returns>
	[Server]
	public static GameObject PoolNetworkInstantiate(GameObject prefab, Vector3? position = null, Transform parent = null, Quaternion? rotation = null, GameObject clone = null)
	{
		if (!IsInstanceInit())
		{
			return null;
		}
		bool isPooled;

		GameObject tempObject = Instance.PoolInstantiate(prefab, clone, position ?? TransformState.HiddenPos, rotation ?? Quaternion.identity, parent, out isPooled);

		if (!isPooled)
		{
			NetworkServer.Spawn(tempObject);
			tempObject.GetComponent<CustomNetTransform>()?.NotifyPlayers();//Sending clientState for newly spawned items
		}


		//broadcast BeSpawned so each component can set itself up
		tempObject.BroadcastMessage("BeSpawned", clone, SendMessageOptions.DontRequireReceiver);

		return tempObject;
	}

	/// <summary>
	/// Spawn the item locally without syncing it over the network.
	/// </summary>
	/// <param name="prefab">Prefab to spawn an instance of. This is intended to be made to work for pretty much any prefab, but don't
	/// be surprised if it doesn't as there are LOTS of prefabs in the game which all have unique behavior for how they should spawn. If you are trying
	/// to instantiate something and it isn't properly setting itself up, check to make sure each component that needs to set something up has
	/// properly implemented a BeSpawned method.</param>
	/// <param name="clone">GameObject which is also an instance of prefab. This will be broadcast to each component of
	/// the newly-spawned object via BeSpawned(clone). It is up to each component to clone its state from this
	/// gameobject. If you're trying to clone something and it isn't working, check that each component that needs to set
	/// its state has properly implemented a BeSpawned method.</param>
	/// <param name="position">world position to appear at. Defaults to HiddenPos (hidden / invisible)</param>
	/// <param name="rotation">rotation to spawn with, defaults to Quaternion.identity</param>
	/// <param name="parent">Parent to spawn under, defaults to no parent. THIS SHOULD RARELY BE NULL! Most things
	/// should always be spawned under the Objects transform in their matrix.</param>
	/// <returns>the newly created GameObject</returns>
	public static GameObject PoolClientInstantiate(GameObject prefab, Vector3? position = null, Transform parent = null, Quaternion? rotation = null, GameObject clone = null)
	{
		if (!IsInstanceInit())
		{
			return null;
		}
		bool isPooled; // not used for Client-only instantiation
		return Instance.PoolInstantiate(prefab, null, position ?? TransformState.HiddenPos, rotation ?? Quaternion.identity, parent, out isPooled);
	}

	private GameObject PoolInstantiate(GameObject prefab, GameObject clone, Vector3 position, Quaternion rotation, Transform parent, out bool pooledInstance)
	{
		GameObject tempObject = null;
		bool hide = position == TransformState.HiddenPos;
		//Cut off Z-axis
		Vector3 cleanPos = ( Vector2 ) position;
		Vector3 pos = hide ? TransformState.HiddenPos : cleanPos;
		if (CanLoadFromPool(prefab))
		{
			//pool exists and has unused instances
			int index = pools[prefab].Count - 1;
			tempObject = pools[prefab][index];
			pools[prefab].RemoveAt(index);
			tempObject.SetActive(true);

			ObjectBehaviour objBehaviour = tempObject.GetComponent<ObjectBehaviour>();
			if (objBehaviour)
			{
				objBehaviour.visibleState = !hide;
			}
			tempObject.transform.position = pos;
			tempObject.transform.rotation = rotation;
			tempObject.transform.localScale = prefab.transform.localScale;
			tempObject.transform.parent = parent;
			var cnt = tempObject.GetComponent<CustomNetTransform>();
			if ( cnt )
			{
				cnt.ReInitServerState();
				cnt.NotifyPlayers(); //Sending out clientState for already spawned items
			}

			pooledInstance = true;
		}
		else
		{
			tempObject = Instantiate(prefab, pos, rotation, parent);

			tempObject.GetComponent<CustomNetTransform>()?.ReInitServerState();

			tempObject.AddComponent<PoolPrefabTracker>().myPrefab = prefab;

			pooledInstance = false;
		}

		return tempObject;

	}

	private bool CanLoadFromPool(GameObject prefab)
	{
		return pools.ContainsKey(prefab) && pools[prefab].Count > 0;
	}

	private static bool IsInstanceInit()
	{
		if (Instance == null)
		{
			//TODO: What's the proper way to prevent this?
			Logger.LogError("PoolManager was attempted to be used before it has initialized. Please delay using" +
			                " PoolManager (such as by using a coroutine to wait) until it is initialized. Nothing will" +
			                " be initialized and null will be returned.");
			return false;
		}

		return true;
	}

	[Server]
	public static void PoolNetworkPreLoad(GameObject prefab)
	{
		if (!IsInstanceInit())
		{
			return;
		}
		GameObject tempObject = null;

		if (prefab == null)
		{
			return;
		}

		//pool for this prefab does not yet exist
		if (!Instance.pools.ContainsKey(prefab))
		{
			Instance.pools.Add(prefab, new List<GameObject>());
		}

		tempObject = Instantiate(prefab, Vector2.zero, Quaternion.identity);
		tempObject.AddComponent<PoolPrefabTracker>().myPrefab = prefab;
		NetworkServer.Spawn(tempObject);
		PoolNetworkDestroy(tempObject);
	}

	/// <summary>
	///     For any objects placed in the scene prior to build
	///     that need to be added to the serverside object pool
	///     pass the object here and make sure object has a
	///     ObjectBehaviour component attached
	/// </summary>
	[Server]
	public static void PoolCacheObject(GameObject obj)
	{
		if (!IsInstanceInit())
		{
			return;
		}
		obj.AddComponent<PoolPrefabTracker>().myPrefab = obj;
		if (!Instance.pools.ContainsKey(obj))
		{
			Instance.pools.Add(obj, new List<GameObject>());
		}
	}

	/// <summary>
	///     For items that are network synced only!
	/// </summary>
	[Server]
	public static void PoolNetworkDestroy(GameObject target)
	{
		if (!IsInstanceInit())
		{
			return;
		}
		Instance.AddToPool(target);
		target.GetComponent<ObjectBehaviour>().visibleState = false;
	}

	/// <summary>
	///     For non network stuff only! (e.g. bullets)
	/// </summary>
	public static void PoolClientDestroy(GameObject target)
	{
		if (!IsInstanceInit())
		{
			return;
		}
		Instance.AddToPool(target);
		target.SetActive(false);
	}

	private void AddToPool(GameObject target)
	{
		var poolPrefabTracker = target.GetComponent<PoolPrefabTracker>();
		if ( !poolPrefabTracker )
		{
			Logger.LogWarning($"PoolPrefabTracker not found on {target}",Category.ItemSpawn);
			return;
		}
		GameObject prefab = poolPrefabTracker.myPrefab;
		prefab.transform.position = Vector2.zero;

		if (!pools.ContainsKey(prefab))
		{
			//pool for this prefab does not yet exist
			pools.Add(prefab, new List<GameObject>());
		}

		pools[prefab].Add(target);
	}

	/*
	* Use this function when you want to get an GameObject instance, but not enable it yet.
	* A good example would be when you want to pass information to the GameObject before it calls
	* OnEnable(). If it can't find a pooled instance, it creates and returns a new one. It does not
	* remove the instance from the pool. Note that it will always be enabled until the next frame, so OnEnable() will run.
	*/
	public static GameObject GetInstanceInactive(GameObject prefab)
	{
		if (!IsInstanceInit())
		{
			return null;
		}
		GameObject tempObject = null;
		if (Instance.pools.ContainsKey(prefab))
		{
			if (Instance.pools[prefab].Count > 0)
			{
				int index = Instance.pools[prefab].Count - 1;
				tempObject = Instance.pools[prefab][index];
				return tempObject;
			}
		}
		else
		{
			Instance.pools.Add(prefab, new List<GameObject>());
		}

		tempObject = Instantiate(prefab);
		tempObject.SetActive(false);
		tempObject.AddComponent<PoolPrefabTracker>().myPrefab = prefab;
		return tempObject;
	}

	public static void ClearPool()
	{
		if (!IsInstanceInit())
		{
			return;
		}
		Instance.pools.Clear();
	}
}

//not used for clients unless it is a client side pool object only
public class PoolPrefabTracker : MonoBehaviour
{
	public GameObject myPrefab;
}