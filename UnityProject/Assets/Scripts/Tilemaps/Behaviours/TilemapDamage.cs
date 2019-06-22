using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Allows for damaging tiles and updating tiles based on damage taken.
/// </summary>
public class TilemapDamage : MonoBehaviour, IFireExposable
{
	private static readonly float TILE_MIN_SCORCH_TEMPERATURE = 100f;

	private TileChangeManager tileChangeManager;

	private MetaDataLayer metaDataLayer;
	private MetaTileMap metaTileMap;

	public Layer Layer { get; private set; }

	//FIXME: cache construction prefabs in CraftingManager.Construction:
	private GameObject glassShardPrefab;
	private GameObject rodsPrefab;
	private Matrix matrix;

	void Awake()
	{
		tileChangeManager = transform.GetComponentInParent<TileChangeManager>();
		metaDataLayer = transform.GetComponentInParent<MetaDataLayer>();
		metaTileMap = transform.GetComponentInParent<MetaTileMap>();

		Layer = GetComponent<Layer>();
		matrix = GetComponentInParent<Matrix>();
	}

	void Start()
	{
		glassShardPrefab = Resources.Load("GlassShard") as GameObject;
		rodsPrefab = Resources.Load("Rods") as GameObject;
	}

	//Server Only:
	public void OnCollisionEnter2D(Collision2D coll)
	{
		if (!CustomNetworkManager.Instance._isServer)
		{
			return;
		}
		ContactPoint2D firstContact = coll.GetContact(0);
		Vector2 dirOfForce = (firstContact.point - (Vector2) coll.transform.position).normalized;
		DetermineAction(coll.gameObject, dirOfForce, firstContact.point);
	}

	private void DetermineAction(GameObject objectColliding, Vector2 forceDirection, Vector3 hitPos)
	{
		BulletBehaviour bulletBehaviour = objectColliding.transform.parent.GetComponent<BulletBehaviour>();
		if (bulletBehaviour != null)
		{
			DoBulletDamage(bulletBehaviour, forceDirection, hitPos);
		}
	}

	private void DoBulletDamage(BulletBehaviour bullet, Vector3 forceDir, Vector3 hitPos)
	{
		forceDir.z = 0;
		Vector3 bulletHitTarget = hitPos + (forceDir * 0.2f);
		Vector3Int cellPos = metaTileMap.WorldToCell(Vector3Int.RoundToInt(bulletHitTarget));
		MetaDataNode data = metaDataLayer.Get(cellPos);

		if (Layer.LayerType == LayerType.Windows)
		{
			LayerTile getTile = metaTileMap.GetTile(cellPos, LayerType.Windows);
			if (getTile != null)
			{
				//TODO damage amt based off type of bullet
				AddWindowDamage(bullet.damage, data, cellPos, bulletHitTarget);
				return;
			}
		}

		if (Layer.LayerType == LayerType.Grills)
		{
			//Make sure a window is not protecting it first:
			if (!metaTileMap.HasTile(cellPos, LayerType.Windows, true))
			{
				if (metaTileMap.HasTile(cellPos, LayerType.Grills, true))
				{
					//TODO damage amt based off type of bullet
					AddGrillDamage(bullet.damage, data, cellPos, bulletHitTarget);
				}
			}
		}
	}

	public void DoThrowDamage(Vector3Int worldTargetPos, ThrowInfo throwInfo, int dmgAmt)
	{
		DoMeleeDamage(new Vector2(worldTargetPos.x, worldTargetPos.y), throwInfo.ThrownBy, dmgAmt);
	}

	//Only works serverside:
	public void DoMeleeDamage(Vector2 dmgPosition, GameObject originator, int dmgAmt)
	{
		Vector3Int cellPos = metaTileMap.WorldToCell(dmgPosition);
		MetaDataNode data = metaDataLayer.Get(cellPos);

		if (Layer.LayerType == LayerType.Windows)
		{
			if (metaTileMap.HasTile(cellPos, LayerType.Windows, true))
			{
				SoundManager.PlayNetworkedAtPos("GlassHit", dmgPosition, Random.Range(0.9f, 1.1f));
				AddWindowDamage(dmgAmt, data, cellPos, dmgPosition);
				return;
			}
		}

		if (Layer.LayerType == LayerType.Grills)
		{
			//Make sure a window is not protecting it first:
			if (!metaTileMap.HasTile(cellPos, LayerType.Windows, true))
			{
				if (metaTileMap.HasTile(cellPos, LayerType.Grills, true))
				{
					SoundManager.PlayNetworkedAtPos("GrillHit", dmgPosition, Random.Range(0.9f, 1.1f));
					AddGrillDamage(dmgAmt, data, cellPos, dmgPosition);
				}
			}
		}
	}

	private void AddWindowDamage(float damage, MetaDataNode data, Vector3Int cellPos, Vector3 bulletHitTarget)
	{
		data.Damage += damage;
		if (data.Damage >= 20 && data.Damage < 50 && data.WindowDmgType != "crack01")
		{
			tileChangeManager.UpdateTile(cellPos, TileType.WindowDamaged, "crack01");
			data.WindowDmgType = "crack01";
		}

		if (data.Damage >= 50 && data.Damage < 75 && data.WindowDmgType != "crack02")
		{
			tileChangeManager.UpdateTile(cellPos, TileType.WindowDamaged, "crack02");
			data.WindowDmgType = "crack02";
		}

		if (data.Damage >= 75 && data.Damage < 100 && data.WindowDmgType != "crack03")
		{
			tileChangeManager.UpdateTile(cellPos, TileType.WindowDamaged, "crack03");
			data.WindowDmgType = "crack03";
		}

		if (data.Damage >= 100 && data.WindowDmgType != "broken")
		{
			tileChangeManager.UpdateTile(cellPos, TileType.WindowDamaged, "none");
			tileChangeManager.RemoveTile(cellPos, LayerType.Windows);

			//Spawn 3 glass shards with different sprites:
			SpawnGlassShards(bulletHitTarget);

			//Play the breaking window sfx:
			SoundManager.PlayNetworkedAtPos("GlassBreak0" + Random.Range(1, 4).ToString(), bulletHitTarget, 1f);

			data.WindowDmgType = "broken";
			data.ResetDamage();
		}
	}

	private void AddGrillDamage(float damage, MetaDataNode data, Vector3Int cellPos, Vector3 bulletHitTarget)
	{
		data.Damage += damage;

		//Make grills a little bit weaker (set to 60 hp):
		if (data.Damage >= 60)
		{
			tileChangeManager.RemoveTile(cellPos, LayerType.Grills);
			tileChangeManager.UpdateTile(cellPos, TileType.WindowDamaged, "GrillDestroyed");

			SoundManager.PlayNetworkedAtPos("GrillHit", bulletHitTarget, 1f);

			//Spawn rods:
			SpawnRods(bulletHitTarget);

			data.ResetDamage();
		}
	}

	//Only works server side:
	public void WireCutGrill(Vector3 snipPosition)
	{
		Vector3Int cellPos = metaTileMap.WorldToCell(snipPosition);
		MetaDataNode data = metaDataLayer.Get(cellPos);

		if (Layer.LayerType == LayerType.Grills)
		{
			//Make sure a window is not protecting it first:
			if (!metaTileMap.HasTile(cellPos, LayerType.Windows, true))
			{
				if (metaTileMap.HasTile(cellPos, LayerType.Grills, true))
				{
					tileChangeManager.RemoveTile(cellPos, LayerType.Grills);

					SoundManager.PlayNetworkedAtPos("WireCutter", snipPosition, 1f);
					SpawnRods(snipPosition);
				}
			}
		}

		data.ResetDamage();
	}

	private void SpawnRods(Vector3 pos)
	{
		GameObject rods = PoolManager.PoolNetworkInstantiate(rodsPrefab, Vector3Int.RoundToInt(pos));

		CustomNetTransform netTransform = rods.GetComponent<CustomNetTransform>();
		netTransform?.SetPosition(netTransform.ServerState.WorldPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)));
	}

	private void SpawnGlassShards(Vector3 pos)
	{
		//Spawn 3 glass shards with different sprites:
		PoolManager.PoolNetworkInstantiate(glassShardPrefab, Vector3Int.RoundToInt(pos)).GetComponent<GlassShard>().SetSpriteAndScatter(0);

		PoolManager.PoolNetworkInstantiate(glassShardPrefab, Vector3Int.RoundToInt(pos)).GetComponent<GlassShard>().SetSpriteAndScatter(1);

		PoolManager.PoolNetworkInstantiate(glassShardPrefab, Vector3Int.RoundToInt(pos)).GetComponent<GlassShard>().SetSpriteAndScatter(2);

		//Play the breaking window sfx:
		SoundManager.PlayNetworkedAtPos("GlassBreak0" + Random.Range(1, 4), pos, 1f);
	}

	public void OnExposed(FireExposure exposure)
	{
		//Determine if tile should be scorched
		if (Layer.LayerType != LayerType.Floors) return;
		//TODO: Not currently handling side exposures on tiles
		if (exposure.IsSideExposure) return;
		if (!(exposure.Temperature > TILE_MIN_SCORCH_TEMPERATURE)) return;
		var cellPos = exposure.ExposedLocalPosition.To3Int();
		if (!metaTileMap.HasTile(cellPos, true)) return;
		//is it already scorched
		var metaData = metaDataLayer.Get(exposure.ExposedLocalPosition.To3Int());
		if (metaData.IsScorched) return;

		//scorch the tile, choose appearance randomly
		//TODO: This should be done using an overlay system which hasn't been implemented yet, this replaces
		//the tile's original appearance
		if (Random.value >= 0.5)
		{
			tileChangeManager.UpdateTile(cellPos, TileType.Floor, "floorscorched1");
		}
		else
		{
			tileChangeManager.UpdateTile(cellPos, TileType.Floor, "floorscorched2");
		}

		metaData.IsScorched = true;
	}
}