using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for a behavior that allows an object to be spawned (with default or user-defined initial state)
/// and cloned in game. State is represented as a generic type so that
/// </summary>
///<typeparamref name="T">Type which encapsulates the state of the object.</typeparamref>
public interface ISpawneable<T> : IHasSpawneableState
{

	/// <summary>
	/// The object has been spawned in and should initialize itself to a default state.
	/// </summary>
	/// <param name="spawnInfo">details about how the object was spawned</param>
	void SpawnDefault(SpawnInfo spawnInfo);

	/// <summary>
	/// The object has been spawned in and should initialize itself with the specified state.
	/// </summary>
	/// <param name="spawnInfo">details about how the object was spawned</param>
	/// <param name="initialState">initial state the object should use to set itself up</param>
	void SpawnWithState(SpawnInfo spawnInfo, T initialState);

	/// <summary>
	/// The object should provide a representation of its state. This will be called infrequently (only when
	/// trying to clone an object), so it is
	/// acceptable to build the state object from scratch here rather than trying to maintain it and update it over the
	/// object's lifetime.
	/// </summary>
	/// <returns>the state of the object</returns>
	T GetState();
}
