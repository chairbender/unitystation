using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for a behavior that allows an object to be spawned (with default or user-defined initial state)
/// and cloned in game. State is represented as a generic type so that
/// </summary>
///<typeparamref name="T">Type which encapsulates the state of the object.</typeparamref>
public interface ISpawnable<T> : IHasSpawnableState
{

	/// <summary>
	/// The object should be spawned in, initialized to a default state (it's up to the item what that is).
	///
	/// Prior to spawning, the object's transform and parent will already be set to the intended
	/// destination. It's up to this method to do any additional work that's needed.
	/// </summary>
	/// <param name="spawnInfo">details about how the object was spawned</param>
	void SpawnDefault(SpawnInfo spawnInfo);

	/// <summary>
	/// The object should be spawned in, initialized to the specified state.
	///
	/// Prior to spawning, the object's transform and parent will already be set to the intended
	/// destination. It's up to this method to do any additional work that's needed.
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
