
using Atmospherics;
using UnityEngine;

/// <summary>
/// Provides information on the fire an object is being exposed to
/// </summary>
public class FireExposure
{
	private readonly float temperature;
	private readonly Vector2Int localPosition;

	/// <summary>
	/// Temperature of the fire.
	/// </summary>
	public float Temperature => temperature;

	/// <summary>
	/// local tile position (within parent matrix) the fire exposure is occurring at.
	/// If you can figure out how to eliminate the need for this be my guest, since
	/// most objects already know their own position. This is only for tiles, because tilemaps
	/// are one big object and need to know which tile is effected.
	/// </summary>
	public Vector2Int LocalPosition => localPosition;

	private FireExposure(float temperature, Vector2Int localPosition)
	{
		this.temperature = temperature;
		this.localPosition = localPosition;
	}

	public static FireExposure FromMetaDataNode(MetaDataNode node)
	{
		if (!node.HasHotspot)
		{
			Logger.LogErrorFormat("MetaDataNode at local position {0} has no hotspot, so no fire exposure" +
			                      " will occur. This is likely a coding error.", Category.Atmos, node.Position);
			return null;
		}
		return new FireExposure(node.Hotspot.Temperature, node.Position.To2Int());
	}
}
