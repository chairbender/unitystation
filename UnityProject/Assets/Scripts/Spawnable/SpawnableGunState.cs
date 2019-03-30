using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gun state for spawning/cloning. Can specify how much ammo has already been spent.
/// </summary>
public class SpawnableGunState
{
	private int ammoSpent;

	private SpawnableGunState(int ammoSpent)
	{
		this.ammoSpent = ammoSpent;
	}

	public static SpawnableGunState fromWeapon(Weapon weapon)
	{
		weapon.CurrentMagazine.magazineSize
	}
}
