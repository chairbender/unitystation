
using UnityEngine;
namespace StatefulInteraction.HandApply
{
	/// <summary>
	/// Base ServerPerformInteraction logic for HandApply
	/// </summary>
	public abstract class BaseServerHandApply : ScriptableObject
	{
		/// <summary>
		/// Server-side. Called after validation succeeds on server side.
		/// Server should perform the interaction and inform clients as needed.
		/// </summary>
		/// <param name="processorObject">object whose component is processing this interaction</param>
		/// <param name="interaction"></param>
		public abstract void ServerPerformInteraction(GameObject processorObject, global::HandApply interaction);
	}
}