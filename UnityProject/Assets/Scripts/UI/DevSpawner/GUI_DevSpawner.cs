using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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


    void Start()
    {
	    ConfigureLucene();

	    //get prefabs
	    var toIndex = PoolManager.SpawnablePrefabs.Select(DevSpawnerDocument.ForPrefab).ToList();

	    //get unicloths
	    TextAsset asset = Resources.Load(Path.Combine("metadata", "hier"))as TextAsset;
	    if (asset != null)
	    {
		    var unicloths =
			    asset.text
					.Split('\n')
					//filter by things allowed to spawn
					.Where(str => str.Contains(UniItemUtils.BagHierIdentifier) ||
					              str.Contains(UniItemUtils.HeadsetHierIdentifier) ||
					              str.Contains(UniItemUtils.ClothingHierIdentifier) ||
					              str.Contains(UniItemUtils.BackPackHierIdentifier))
					.Select(str => str.Trim())
					.Select(DevSpawnerDocument.ForUniCloth)
					.ToList();
		    toIndex.AddRange(unicloths);
	    }
	    else
	    {
		    Logger.LogError("Unable to load unicloths from metadata/hier directory. They will be unavailable" +
		                    " in the dev spawner.");
	    }

	    //start indexing
	    StartCoroutine(lucene.IndexCoroutine(toIndex));
    }

    private void OnLuceneProgress(object sender, LuceneProgressEventArgs e)
    {
	    Logger.LogFormat("Lucene indexing progress title: {0}, total: {1}", Category.ItemSpawn, e.Title, e.Total);
    }

    public void Search()
    {
	    //delete previous results
	    foreach (Transform child in contentPanel.transform)
	    {
		    Destroy(child.gameObject);
	    }

	    var docs = lucene.Search(searchBox.text);

	    //display new results
	    foreach (var doc in docs)
	    {
		    if (doc.Get("type").Equals(DevSpawnerDocument.UNICLOTH_TYPE))
		    {
			    CreateListItem(doc.Get("name"));
		    }
		    else
		    {
			    GameObject prefab = PoolManager.GetPrefabByName(doc.Get("name"));
			    CreateListItem(prefab);
		    }
	    }
    }

    private void ConfigureLucene()
    {
	    lucene = new Lucene3D();
	    lucene.Progress += OnLuceneProgress;

	    lucene.DefineIndexField<DevSpawnerDocument>("id", doc => doc.Name, IndexOptions.PrimaryKey);
	    lucene.DefineIndexField<DevSpawnerDocument>("name", doc => doc.Name, IndexOptions.IndexTermsAndStore);
	    lucene.DefineIndexField<DevSpawnerDocument>("type", doc => doc.Type, IndexOptions.IndexTermsAndStore);
    }



    //add a list item to the content panel for spawning the specified prefab
    private void CreateListItem(GameObject forPrefab)
    {
	    GameObject listItem = Instantiate(listItemPrefab);
	    listItem.GetComponent<DevSpawnerListItemController>().Initialize(forPrefab);
	    listItem.transform.SetParent(contentPanel.transform);
	    listItem.transform.localScale = Vector3.one;
    }

    private void CreateListItem(string forHier)
    {
	    GameObject listItem = Instantiate(listItemPrefab);
	    listItem.GetComponent<DevSpawnerListItemController>().Initialize(forHier);
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
