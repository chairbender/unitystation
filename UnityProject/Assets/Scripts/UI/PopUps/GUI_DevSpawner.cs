using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Main logic for the dev spawner menu
/// </summary>
public class GUI_DevSpawner : MonoBehaviour
{
	public static GUI_DevSpawner Instance;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		//TODO: Not sure if this is needed, its used in GUI_IngameMenu
		/*
		else
		{
			Destroy(gameObject);
		}
		*/
	}

	public void Open()
	{
		SoundManager.Play("Click01");
		Logger.Log("Opening dev spawner menu", Category.UI);
		transform.GetChild(0).gameObject.SetActive(true);
		transform.SetAsLastSibling();
	}

	public void Close()
	{
		SoundManager.Play("Click01");
		Logger.Log("Closing dev spawner menu", Category.UI);
		transform.GetChild(0).gameObject.SetActive(false);
	}

	public void SpawnCoffee()
	{
		var coffeeprefab = Resources.Load("Coffee") as GameObject;
		Vector3 position = PlayerManager.LocalPlayer.transform.position;
		Transform parent = PlayerManager.LocalPlayer.transform.parent;
		var coffeeInstance = Object.Instantiate(coffeeprefab, position, Quaternion.identity, parent);
		var spawneableState = coffeeInstance.GetComponent<IHasSpawneableState>();
		//instantiate the default
		Type stateType = spawneableState.getStateType();
		Type spawneableType = typeof(ISpawneable<>).MakeGenericType(stateType);
		var spawneable = coffeeInstance.GetComponent(spawneableType);
		var defaultSpawn = spawneableType.GetMethod("SpawnDefault");
		defaultSpawn.Invoke(spawneable, new object[]{SpawnInfo.AtPosition((Vector2Int) position.RoundToInt())});
	}
}
