﻿using UnityEngine;
using UnityEngine.Networking;

public class ClothFactory : NetworkBehaviour
{
	public static ClothFactory Instance;

	public static string ClothingHierIdentifier = "cloth";
	public static string HeadsetHierIdentifier = "headset";
	public static string BackPackHierIdentifier = "storage/backpack";
	public static string BagHierIdentifier = "storage/bag";

	private GameObject uniCloth { get; set; }
	private GameObject uniHeadSet { get; set; }
	private GameObject uniBackPack { get; set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		//Do init stuff
		uniCloth = Resources.Load("UniCloth") as GameObject;
		uniHeadSet = Resources.Load("UniHeadSet") as GameObject;
		uniBackPack = Resources.Load("UniBackPack") as GameObject;
	}

	public void PreLoadCloth(int preLoads)
	{
		for (int i = 0; i < preLoads; i++)
		{
			PoolManager.PoolNetworkPreLoad(Instance.uniCloth);
			PoolManager.PoolNetworkPreLoad(Instance.uniHeadSet);
		}
	}

	//TODO is it going to be spawned on a player in equipment etc?
	public GameObject CreateCloth(string hierString, Vector3 spawnPos, Transform parent)
	{
		if (!CustomNetworkManager.Instance._isServer)
		{
			return null;
		}

		//PoolManager handles networkspawn
		GameObject uniItem = pickClothObject(hierString);
		GameObject clothObj = PoolManager.PoolNetworkInstantiate(uniItem, spawnPos, parent: parent);
		ItemAttributes i = clothObj.GetComponent<ItemAttributes>();
		i.hierarchy = hierString;
		if (uniItem == uniHeadSet)
		{
			Headset headset = clothObj.GetComponent<Headset>();
			headset.init();
		}
		return clothObj;
	}

	private GameObject pickClothObject(string hierarchy)
	{
		if (hierarchy.Contains(ClothingHierIdentifier))
		{
			return uniCloth;
		}
		if (hierarchy.Contains(HeadsetHierIdentifier))
		{
			return uniHeadSet;
		}
		if (hierarchy.Contains(BackPackHierIdentifier) || hierarchy.Contains(BagHierIdentifier))
		{
			return uniBackPack;
		}
		Logger.LogError("Cloth factory could not pick uni item. Falling back to uniCloth", Category.DmMetadata);
		return uniCloth;
	}
}