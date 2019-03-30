using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawnable which has no state or other special needs upon being spawned. Does nothing.
/// </summary>
public class StatelessSpawnable : MonoBehaviour, ISpawnable<object>
{
	public Type getStateType()
    {
	    return typeof(object);
    }

    public void SpawnWithState(SpawnInfo spawnInfo, object initialState)
    {
	    //do nothing - we're good to go
    }

    public object GetState()
    {
	    return null;
    }
}
