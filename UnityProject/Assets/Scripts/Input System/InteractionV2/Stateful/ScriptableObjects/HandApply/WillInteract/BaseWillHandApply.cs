
using UnityEngine;

/// <summary>
/// Base WillInteract asset for HandApply.
/// </summary>
public abstract class BaseWillHandApply : ScriptableObject
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
	public abstract bool WillInteract(GameObject processorObject, HandApply interaction, NetworkSide side);
}
