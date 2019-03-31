using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DevSpawnerListItemController : MonoBehaviour
{
	public Image image;
	public Text titleText;
	public Text detailText;
	private GameObject prefab;

	public void Initialize(GameObject forPrefab)
	{
		//use the first sprite we find on the prefab as the image
		Sprite toUse = forPrefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
		if (toUse != null)
		{
			image.sprite = toUse;
		}

		titleText.text = forPrefab.name;
		//TODO: Can add extra info to detail text as desired
		detailText.text = "Prefab";
		prefab = forPrefab;
	}

	public void Spawn()
	{
		Vector3 position = PlayerManager.LocalPlayer.transform.position;
		Transform parent = PlayerManager.LocalPlayer.transform.parent;
		PoolManager.PoolNetworkInstantiate(prefab, position, parent);
	}
}
