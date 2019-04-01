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
	private string hier;

	/// <summary>
	/// Initializes it to display an item for spawning a prefab
	/// </summary>
	/// <param name="forPrefab"></param>
	public void Initialize(GameObject forPrefab)
	{
		//use the first sprite we find on the prefab as the image
		Sprite toUse = forPrefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
		if (toUse != null)
		{
			image.sprite = toUse;
		}

		titleText.text = forPrefab.name;
		detailText.text = "Prefab";
		prefab = forPrefab;
	}

	/// <summary>
	/// Initializes it to display an item for spawning a unicloth
	/// </summary>
	/// <param name="forhier">hier of the unicloth</param>
	public void Initialize(string forhier)
	{
		//try to get the icon
		Sprite toUse = UniItemUtils.GetInventoryIconSprite(forhier);
		if (toUse != null)
		{
			image.sprite = toUse;
		}

		string[] nodes = forhier.Split('/');
		titleText.text = nodes[nodes.Length-1];
		detailText.text = forhier;
		hier = forhier;
	}

	public void Spawn()
	{
		if (CustomNetworkManager.IsServer)
		{
			Vector3 position = PlayerManager.LocalPlayer.transform.position;
			Transform parent = PlayerManager.LocalPlayer.transform.parent;
			if (hier != null)
			{
				ClothFactory.CreateCloth(hier, position, parent);
			}
			else
			{
				PoolManager.PoolNetworkInstantiate(prefab, position, parent);
			}

		}
		else
		{
			if (hier != null)
			{
				DevSpawnMessage.Send(hier, true, PlayerManager.LocalPlayer.WorldPos());
			}
			else
			{
				DevSpawnMessage.Send(prefab.name, false, PlayerManager.LocalPlayer.WorldPos());
			}

		}

	}
}
