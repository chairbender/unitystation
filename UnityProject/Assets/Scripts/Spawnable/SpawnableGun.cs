using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior allowing spawning of a gun.
/// </summary>
public class SpawnableGun : MonoBehaviour, ISpawnable<SpawnableGunState>
{
	private Weapon weapon;

	void Awake()
	{
		weapon = GetComponent<Weapon>();
	}

	public Type getStateType()
	{
		return typeof(SpawnableGunState);
	}

	public void SpawnWithState(SpawnInfo spawnInfo, SpawnableGunState initialState)
	{
		throw new NotImplementedException();
	}

	public SpawnableGunState GetState()
	{
		return SpawnableGunState.fromWeapon(weapon);
	}
}
