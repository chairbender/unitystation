using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Stateful interaction component which allows an object to respond to hand apply interactions
/// and allows it to move into different possible states and have different interaction logic depending
/// on the state.
/// </summary>
public class HandAppliable : NetworkBehaviour, IServerSpawn
{

	[Tooltip("Required. Initial HandApplyState when this object spawns. Must reference a state which exists in States.")]
	[SerializeField]
	private BaseHandApplyState initialState;

	[Tooltip("Possible interaction states this object can have, and any custom logic for those" +
	         " states. No HandApplyState should appear more than once in this list.")]
	[SerializeField]
	private List<HandApplyStateEntry> states;

	//tracks the index of the state we are currently in.
	[SyncVar(hook = nameof(SyncCurrentStateIndex))]
	private int currentStateIndex = -1;

	private HandApplyStateEntry currentStateEntry;


	public override void OnStartClient()
	{
		SyncCurrentStateIndex(currentStateIndex);
	}

	public void OnSpawnServer(SpawnInfo info)
	{
		//start in initial state
		if (initialState == null)
		{
			Logger.LogErrorFormat("Initial State not defined for {0}. Please fix this component.", Category.Interaction,
				this);
			return;
		}
		var initialStateIndex = states.FindIndex(se => se.State == initialState);
		if (initialStateIndex == -1)
		{
			Logger.LogErrorFormat("Initial State doesn't exist in States defined for {0}. Please fix this component.", Category.Interaction,
				this);
			return;
		}

		SyncCurrentStateIndex(initialStateIndex);
	}


	private void SyncCurrentStateIndex(int newStateIndex)
	{
		currentStateIndex = newStateIndex;
		currentStateEntry = states[currentStateIndex];
	}

	/// <summary>
	/// Change the current state to the indicated one, changing this object's interaction logic. State must have an entry in
	/// States.
	/// </summary>
	/// <param name="newState"></param>
	[Server]
	public void ServerChangeState(BaseHandApplyState newState)
	{
		var newStateIndex = states.FindIndex(se => se.State == newState);
		if (newStateIndex == -1)
		{
			Logger.LogErrorFormat("New state doesn't exist in States defined for {0}. State will not be changed.", Category.Interaction,
				this);
			return;
		}
		SyncCurrentStateIndex(newStateIndex);
	}
}
