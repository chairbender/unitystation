
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Used to define the possible interaction states in a particular
/// stateful interaction component.
///
/// NOTE: Can't actually be serialized due to using generics, but this is just here to show what should be
/// added to each subclass.
/// </summary>
[Serializable]
public abstract class InteractionStateEntry<T> where T : Interaction
{
	[Tooltip("Required. Indicates a possible state this object can be in and defines the interaction logic " +
	         "that should be used when object is in this state.")]
	[SerializeField]
	private StatefulState state;

	/// <summary>
	/// Interaction state for this entry.
	/// </summary>
	public StatefulState State => state;

	[Tooltip("Event hooks for adding custom logic on top of the interaction state's own logic.")]
	[SerializeField]
	private Events events;

	/// <summary>
	/// Defines the customizations for interaction logic on top of the InteractionState itself.
	/// </summary>
	[Serializable]
	private class Events
	{
		[Tooltip("Additional WillInteract logic which will be invoked on after the interaction state's own logic. " +
		         "If any of these invoke" +
		         " WillInteractResponder.RespondNo, interaction will be cancelled.")]
		[SerializeField]
		private WillInteractEvent OnWillInteract = new WillInteractEvent();

		[Tooltip("Invoked when server will perform interaction, before the interaction state's own logic.")]
		[SerializeField]
		private InteractEvent OnServerPerformInteract = new InteractEvent();

		[Tooltip("Invoked when client prediction will happen, before the interaction state's own logic.")]
		[SerializeField]
		private InteractEvent OnClientPredictInteract = new InteractEvent();

		[Tooltip("Invoked when server should rollback client's prediction, before the interaction state's own logic.")]
		[SerializeField]
		private InteractEvent OnServerRollbackClient = new InteractEvent();
	}

	private class InteractEvent : UnityEvent<T>
	{
	}


	private class WillInteractEvent : UnityEvent<T, NetworkSide, WillInteractResponder>
	{
	}
}


