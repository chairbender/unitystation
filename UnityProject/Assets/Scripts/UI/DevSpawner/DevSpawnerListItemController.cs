using System.Collections;
using System.Collections.Generic;
using Lucene.Net.Documents;
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
	/// Initializes it to display the document
	/// </summary>
	/// <param name="resultDoc">document to display</param>
	public void Initialize(Document resultDoc)
	{
		if (resultDoc.Get("type").Equals(DevSpawnerDocument.PREFAB_TYPE))
		{
			prefab = PoolManager.GetPrefabByName(resultDoc.Get("name"));
			Sprite toUse = prefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
			if (toUse != null)
			{
				image.sprite = toUse;
			}
		}
		else
		{
			hier = resultDoc.Get("hier");
			Sprite toUse = UniItemUtils.GetInventoryIconSprite(hier);
			if (toUse != null)
			{
				image.sprite = toUse;
			}
		}
		titleText.text = resultDoc.Get("name");
		detailText.text = hier ?? "Prefab";
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
