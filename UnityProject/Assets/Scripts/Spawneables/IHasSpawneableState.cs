using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a behavior which has a type that provides a representation of its object's state for the purposes
/// of spawning / cloning.
/// </summary>
public interface IHasSpawneableState
{
	/// <summary>
	/// Provides
	/// </summary>
	/// <returns>The concrete type that represents this object's state.</returns>
	Type getStateType();
}
