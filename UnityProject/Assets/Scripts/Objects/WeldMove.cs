
using UnityEngine;

/// <summary>
/// Info about movement caused by a welded object moving other objects that it is welded to.
/// </summary>
public struct WeldMove
{
	/// <summary>
	/// Object which is causing the weld movement.
	/// </summary>
	public readonly IPushable Cause;

	/// <summary>
	/// World position being moved from
	/// </summary>
	public readonly Vector3Int FromWorldPosition;

	/// <summary>
	/// World position that will be moved to.
	/// </summary>
	public readonly Vector3Int ToWorldPosition;
}
