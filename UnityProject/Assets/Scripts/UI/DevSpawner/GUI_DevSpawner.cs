using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lucene.Unity;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Main logic for the dev spawner menu
/// </summary>
public class GUI_DevSpawner : MonoBehaviour
{

	[Tooltip("Prefab that should be used for each list item")]
	public GameObject listItemPrefab;
	[Tooltip("content panel into which the list items should be placed")]
	public GameObject contentPanel;
	public InputField searchBox;

	// search index
	private Lucene3D lucene;
	//dict for looking up prefabs by instance ID (yes, there is no mechanism in Unity to do this already)
	private Dictionary<int, GameObject> instanceIdToPrefab = new Dictionary<int, GameObject>();

    void Start()
    {
	    ConfigureLucene();

	    //Search through our resources and find each prefab that has a CNT component
	    var spawnablePrefabs = Resources.FindObjectsOfTypeAll<GameObject>()
		    .Where(IsPrefab)
		    .OrderBy(go => go.name)
		    //check if they have CNTs (thus are spawnable)
		    .Where(go => go.GetComponent<CustomNetTransform>() != null);

	    foreach (var spawnablePrefab in spawnablePrefabs)
	    {
		    instanceIdToPrefab.Add(spawnablePrefab.GetInstanceID(), spawnablePrefab);
	    }

	    StartCoroutine(lucene.IndexCoroutine(instanceIdToPrefab.Values.ToList().Select(DevSpawnerDocument.ForPrefab)));
    }

    private void OnLuceneProgress(object sender, LuceneProgressEventArgs e)
    {
	    Logger.LogFormat("Lucene indexing progress title: {0}, total: {1}", Category.ItemSpawn, e.Title, e.Total);
    }

    public void Search()
    {
	    //TODO: re-use objects
	    //delete previous results
	    foreach (Transform child in contentPanel.transform)
	    {
		    GameObject.Destroy(child.gameObject);
	    }

	    var docs = lucene.Search(searchBox.text);

	    //display new results
	    foreach (var doc in docs)
	    {
		    GameObject prefab = instanceIdToPrefab[Convert.ToInt32(doc.Get("id"))];
			CreateListItem(prefab);
	    }
    }

    private void ConfigureLucene()
    {
	    lucene = new Lucene3D();
	    lucene.Progress += OnLuceneProgress;

	    lucene.DefineIndexField<DevSpawnerDocument>("name", doc => doc.PrefabName, IndexOptions.IndexTerms);
	    lucene.DefineIndexField<DevSpawnerDocument>("id", doc => doc.PrefabInstanceID.ToString(), IndexOptions.PrimaryKey);
    }

    private static bool IsPrefab(GameObject toCheck) => !toCheck.transform.gameObject.scene.IsValid();

    //add a list item to the content panel for spawning the specified prefab
    private void CreateListItem(GameObject forPrefab)
    {
	    GameObject listItem = Instantiate(listItemPrefab);
	    listItem.GetComponent<DevSpawnerListItemController>().Initialize(forPrefab);
	    listItem.transform.SetParent(contentPanel.transform);
	    listItem.transform.localScale = Vector3.one;
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
}
