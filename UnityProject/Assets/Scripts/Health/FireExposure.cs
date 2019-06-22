
using Atmospherics;
using UnityEngine;

/// <summary>
/// Provides information on the fire an object is being exposed to.
/// If an object is atmos passable, an exposure can occur directly on it. However, if it
/// is not atmos passable, an exposure can occur from the side of the object - the fire brushes
/// against the object.
/// </summary>
public class FireExposure
{
	private readonly float temperature;
	private readonly Vector2Int hotspotLocalPosition;
	private readonly Vector2Int atLocalPosition;

	/// <summary>
	/// True iff this is a side exposure (on a non-atmos-passable object)
	/// </summary>
	public bool IsSideExposure => atLocalPosition != hotspotLocalPosition;

	/// <summary>
	/// Temperature of the fire.
	/// </summary>
	public float Temperature => temperature;

	/// <summary>
	/// local tile position (within parent matrix) the hotspot is at.
	///
	/// If you can figure out how to eliminate the need for this be my guest, since
	/// most objects already know their own position. This is only for tiles, because tilemaps
	/// are one big object and need to know which tile is effected.
	/// </summary>
	public Vector2Int HotspotLocalPosition => hotspotLocalPosition;

	/// <summary>
	/// Position that is actually being exposed to this hotspot. This will not be
	/// the same as the HotspotLocalPosition if this is a side exposure.
	/// </summary>
	public Vector2Int ExposedLocalPosition => atLocalPosition;

	private FireExposure(float temperature, Vector2Int hotspotLocalPosition, Vector2Int atLocalPosition)
	{
		this.temperature = temperature;
		this.hotspotLocalPosition = hotspotLocalPosition;
		this.atLocalPosition = atLocalPosition;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="hotspotNode">node of the hotspot</param>
	/// <param name="atLocalPosition">position to expose this hotspot to</param>
	/// <returns></returns>
	public static FireExposure FromMetaDataNode(MetaDataNode hotspotNode, Vector2Int atLocalPosition)
	{
		if (!hotspotNode.HasHotspot)
		{
			Logger.LogErrorFormat("MetaDataNode at local position {0} has no hotspot, so no fire exposure" +
			                      " will occur. This is likely a coding error.", Category.Atmos, hotspotNode.Position);
			return null;
		}
		return new FireExposure(hotspotNode.Hotspot.Temperature, hotspotNode.Position.To2Int(), atLocalPosition);
	}
}
