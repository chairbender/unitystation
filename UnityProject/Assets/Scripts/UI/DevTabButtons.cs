using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for the various buttons on the Dev tab
/// </summary>
public class DevTabButtons : MonoBehaviour
{
	public void BtnSpawnItem()
	{
		//TODO: Pop up item spawner window
		GUI_DevSpawner.Instance.Open();
	}
}
