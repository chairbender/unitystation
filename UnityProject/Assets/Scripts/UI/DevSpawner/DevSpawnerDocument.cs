using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// POCO representing something spawnable, indexed into Lucene to support searching for spawnable items.
/// </summary>
public class DevSpawnerDocument
{
	// string representing the unicloth type in the indexed documents
	public static readonly string UNICLOTH_TYPE = "UniCloth";
	// string representing the prefab type in the indexed documents
	public static readonly string PREFAB_TYPE = "Prefab";

	/// <summary>
	/// If prefab, Name of the prefab (without .prefab). If unicloth, hier string.
	/// </summary>
	public readonly string Name;
	/// <summary>
	/// Type of this spawnable.
	/// </summary>
	public string Type => isUniCloth ? UNICLOTH_TYPE : PREFAB_TYPE;

	private readonly bool isUniCloth;

	private DevSpawnerDocument(string name, bool isUniCloth = false)
	{

		Name = name;
		this.isUniCloth = isUniCloth;
	}

	/// <summary>
	/// Create a dev spawner document representing this prefab.
	/// </summary>
	/// <param name="prefab"></param>
	public static DevSpawnerDocument ForPrefab(GameObject prefab)
	{
		return new DevSpawnerDocument(prefab.name);
	}

	/// <summary>
	/// Create a dev spawner document representing this prefab.
	/// </summary>
	/// <param name="prefab"></param>
	public static DevSpawnerDocument ForUniCloth(string hier)
	{
		return new DevSpawnerDocument(hier, true);
	}
}
