﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Edible which can be spawned in either an eaten or uneaten state
/// </summary>
public class SpawneableEdible : MonoBehaviour, ISpawneable<bool>
{

    public Type getStateType()
    {
	    return typeof(bool);
    }

    public void SpawnDefault(SpawnInfo spawnInfo)
    {
	    //Can we get away with doing nothing?
	    //Spawn uneaten.
    }

    public void SpawnWithState(SpawnInfo spawnInfo, bool initialState)
    {
	    throw new NotImplementedException();
    }

    public bool GetState()
    {
	    throw new NotImplementedException();
    }
}
