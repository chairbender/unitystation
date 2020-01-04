
using System.Collections.Generic;

/// <summary>
/// Info on a weld being performed, causing the objects involved to be attached to each other and move
/// as one.
/// </summary>
public class WeldInfo
{
	/// <summary>
	/// ALl the pushables that should be welded together.
	/// </summary>
	public readonly IEnumerable<IPushable> Pushables;
}
