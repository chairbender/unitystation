
using UnityEngine;

/// <summary>
/// Describes the way an object should currently react to a particular interaction.
/// </summary>
public abstract class InteractionState<T> : ScriptableObject
	where T : Interaction
{
	/// <summary>
	/// Decides if interaction logic should proceed. On client side, the interaction
	/// request will only be sent to the server if this returns true. On server side,
	/// the interaction will only be performed if this returns true.
	/// </summary>
	/// <param name="processorObject">object whose component is processing this interaction</param>
	/// <param name="interaction">interaction to validate</param>
	/// <param name="side">which side of the network this is being invoked on</param>
	/// <returns>True/False based on whether the interaction logic should proceed as described above.</returns>
	public abstract bool WillInteract(GameObject processorObject, T interaction, NetworkSide side);

	/// <summary>
	/// Server-side. Called after validation succeeds on server side.
	/// Server should perform the interaction and inform clients as needed.
	/// </summary>
	/// <param name="processorObject">object whose component is processing this interaction</param>
	/// <param name="interaction"></param>
	public abstract void ServerPerformInteraction(GameObject processorObject, T interaction);

	/// <summary>
	/// Client-side prediction. Called after Willinteract returns true on client side.
	/// Client can perform client side prediction. NOT invoked for server player, since there is no need
	/// for prediction.
	/// </summary>
	/// <param name="processorObject">object whose component is processing this interaction</param>
	public abstract void ClientPredictInteraction(GameObject processorObject, T interaction);

	/// <summary>
	/// Called when server-side validation fails. Server can use this hook to use whatever
	/// means necessary to tell client to roll back its prediction and get back in sync with server.
	/// </summary>
	/// <param name="processorObject">object whose component is processing this interaction</param>
	/// <param name="interaction"></param>
	public abstract void ServerRollbackClient(GameObject processorObject, T interaction);
}
