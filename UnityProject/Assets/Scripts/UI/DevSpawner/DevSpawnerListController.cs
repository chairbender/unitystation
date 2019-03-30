using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Light2D;
using UnityEditor;
using UnityEngine;

public class DevSpawnerListController : MonoBehaviour
{
	[Tooltip("Prefab that should be used for each list item")]
	public GameObject listItemPrefab;
	[Tooltip("content panel into which the list items should be placed")]
	public GameObject contentPanel;

    // Start is called before the first frame update
    void Start()
    {
	    //Search through our resources and find each prefab that has a CNT component
	    var spawnablePrefabs = AssetDatabase.GetAllAssetPaths()
		    //get prefabs
		    .Where(s => s.EndsWith(".prefab"))
		    //load them as game objects
		    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
		    //check if they have CNTs (thus are spawnable)
		    .Where(go => go != null && go.GetComponent<CustomNetTransform>() != null);

	    foreach (var spawnablePrefab in spawnablePrefabs)
	    {
		    CreateListItem(spawnablePrefab);
	    }
    }

    //add a list item to the content panel for spawning the specified prefab
    private void CreateListItem(GameObject forPrefab)
    {
	    GameObject listItem = Instantiate(listItemPrefab);
	    listItem.GetComponent<DevSpawnerListItemController>().Initialize(forPrefab);
	    listItem.transform.parent = contentPanel.transform;
	    listItem.transform.localScale = Vector3.one;
    }
}
