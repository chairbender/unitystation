using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Message allowing a client dev / admin to spawn something, validated server side.
/// </summary>
public class DevSpawnMessage : ClientMessage
{
	public static short MessageType = (short) MessageTypes.DevSpawnMessage;
	// name of the prefab to spawn
	public string PrefabName;
	// position to spawn at.
	public Vector2 WorldPosition;

	public override IEnumerator Process()
	{
		//TODO: Validate if player is allowed to spawn things. For now we will let anyone spawn.
		PoolManager.PoolNetworkInstantiate(PrefabName, WorldPosition);
		yield return null;
	}

	public override string ToString()
	{
		return $"[DevSpawnMessage PrefabName={PrefabName} WorldPosition={WorldPosition}]";
	}

	/// <summary>
	/// Ask the server to spawn a specific prefab
	/// </summary>
	/// <param name="prefabName">name of the prefab to instantiate (network synced)</param>
	/// <param name="worldPosition">world position to spawn it at</param>
	/// <returns></returns>
	public static void Send(string prefabName, Vector2 worldPosition)
	{

		DevSpawnMessage msg = new DevSpawnMessage
		{
			PrefabName = prefabName,
			WorldPosition = worldPosition
		};
		msg.Send();
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		PrefabName = reader.ReadString();
		WorldPosition = reader.ReadVector2();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(PrefabName);
		writer.Write(WorldPosition);
	}
}
