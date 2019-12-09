﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ConnectionConfig = UnityEngine.Networking.ConnectionConfig;
using Mirror;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
	public static bool IsServer => Instance._isServer;

	public static CustomNetworkManager Instance;

	[HideInInspector] public bool _isServer;
	[HideInInspector] public bool spawnableListReady;
	public GameObject humanPlayerPrefab;
	public GameObject ghostPrefab;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		SetSpawnableList();

		//Automatically host if starting up game *not* from lobby
		if (SceneManager.GetActiveScene().name != offlineScene)
		{
			StartHost();
		}
	}

	private void SetSpawnableList()
	{
		spawnPrefabs.Clear();

		NetworkIdentity[] networkObjects = Resources.LoadAll<NetworkIdentity>("Prefabs");
		foreach (NetworkIdentity netObj in networkObjects)
		{
			spawnPrefabs.Add(netObj.gameObject);
		}

		string[] dirs = Directory.GetDirectories(Application.dataPath, "Resources/Prefabs", SearchOption.AllDirectories); //could be changed later not to load everything to save start-up times
		foreach (string dir in dirs)
		{
			//Should yield For a frame to Increase performance
			loadFolder(dir);
			foreach (string subdir in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
			{
				loadFolder(subdir);
			}
		}

		spawnableListReady = true;
	}

	private void loadFolder(string folderpath)
	{
		folderpath = folderpath.Substring(folderpath.IndexOf("Resources", StringComparison.Ordinal) + "Resources".Length);
		foreach (NetworkIdentity netObj in Resources.LoadAll<NetworkIdentity>(folderpath))
		{
			if (!spawnPrefabs.Contains(netObj.gameObject))
			{
				spawnPrefabs.Add(netObj.gameObject);
			}
		}
	}

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += OnLevelFinishedLoading;
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= OnLevelFinishedLoading;
	}

	public override void OnStartServer()
	{
		_isServer = true;
		base.OnStartServer();
		this.RegisterServerHandlers();
		// Fixes loading directly into the station scene
		if (GameManager.Instance.LoadedDirectlyToStation)
		{
			GameManager.Instance.PreRoundStart();
		}
	}

	public static void Kick(ConnectedPlayer player, string raisins = "4 no raisins")
	{
		if (!player.Connection.isConnected)
		{
			Logger.Log($"Not kicking, already disconnected: {player}", Category.Connections);
			return;
		}
		Logger.Log($"Kicking {player} : {raisins}", Category.Connections);
		InfoWindowMessage.Send(player.GameObject, $"Kicked: {raisins}", "Kicked");
		Chat.AddGameWideSystemMsgToChat($"Player '{player.Name}' got kicked: {raisins}");
		player.Connection.Disconnect();
		//MIRRORUPGRADE: This doesn't seem to be needed anymore, mirror takes care of this automatically?
		//player.Connection.Dispose();
	}

	public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage)
	{
		//This spawns the player prefab
		if (GameData.IsHeadlessServer || GameData.Instance.testServer)
		{
			//this is a headless server || testing headless (it removes the server player for localClient)
			if (conn.address != "localClient")
			{
				StartCoroutine(WaitToSpawnPlayer(conn));
			}
		}
		else
		{
			//This is a host server (keep the server player as it is for the host player)
			StartCoroutine(WaitToSpawnPlayer(conn));
		}

		if (_isServer)
		{
			//Tell them what the current round time is
			UpdateRoundTimeMessage.Send(GameManager.Instance.stationTime.ToString("O"));
		}
	}
	private IEnumerator WaitToSpawnPlayer(NetworkConnection conn)
	{
		yield return WaitFor.Seconds(1f);
		OnServerAddPlayerInternal(conn);
	}

	private void OnServerAddPlayerInternal(NetworkConnection conn)
	{
		if (playerPrefab == null)
		{
			if (!LogFilter.Debug)
			{
				return;
			}
			Logger.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.", Category.Connections);
		}
		else if (playerPrefab.GetComponent<NetworkIdentity>() == null)
		{
			if (!LogFilter.Debug)
			{
				return;
			}
			Logger.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.", Category.Connections);
		}
		// else if (playerControllerId < conn.playerControllers.Count && conn.playerControllers[playerControllerId].IsValid &&
		// 	conn.playerControllers[playerControllerId].gameObject != null)
		// {
		// 	if (!LogFilter.Debug)  FIXME!! - need a way to determine if player has already spawned before this round
		// 	{
		// 		return;
		// 	}
		// 	Logger.LogError("There is already a player at that playerControllerId for this connections.", Category.Connections);
		// }
		else
		{
			PlayerSpawn.ServerSpawnViewer(conn);
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		//Does this need to happen all the time? OnClientConnect can be called multiple times
		this.RegisterClientHandlers(conn);

		base.OnClientConnect(conn);
	}

	///Sync some position data explicitly, if it is required
	/// Warning: sending a lot of data, make sure client receives it only once
	public void SyncPlayerData(GameObject playerGameObject)
	{
		//All matrices
		MatrixMove[] matrices = FindObjectsOfType<MatrixMove>();
		for (var i = 0; i < matrices.Length; i++)
		{
			matrices[i].NotifyPlayer(playerGameObject, true);
		}

		//All transforms
		CustomNetTransform[] scripts = FindObjectsOfType<CustomNetTransform>();
		for (var i = 0; i < scripts.Length; i++)
		{
			scripts[i].NotifyPlayer(playerGameObject);
		}

		//All player bodies
		PlayerSync[] playerBodies = FindObjectsOfType<PlayerSync>();
		for (var i = 0; i < playerBodies.Length; i++)
		{
			var playerBody = playerBodies[i];
			playerBody.NotifyPlayer(playerGameObject, true);
			var playerSprites = playerBody.GetComponent<PlayerSprites>();
			if (playerSprites)
			{
				playerSprites.NotifyPlayer(playerGameObject);
			}
			var equipment = playerBody.GetComponent<Equipment>();
			if (equipment)
			{
				equipment.NotifyPlayer(playerGameObject);
			}
		}
		//TileChange Data
		TileChangeManager[] tcManagers = FindObjectsOfType<TileChangeManager>();
		for (var i = 0; i < tcManagers.Length; i++)
		{
			tcManagers[i].NotifyPlayer(playerGameObject);
		}

		//Doors
		DoorController[] doors = FindObjectsOfType<DoorController>();
		for (var i = 0; i < doors.Length; i++)
		{
			doors[i].NotifyPlayer(playerGameObject);
		}
		Logger.Log($"Sent sync data ({matrices.Length} matrices, {scripts.Length} transforms, {playerBodies.Length} players) to {playerGameObject.name}", Category.Connections);
	}

	/// server actions when client disconnects
	public override void OnServerDisconnect(NetworkConnection conn)
	{
		var player = PlayerList.Instance.Get(conn);
		Logger.Log($"Player Disconnected: {player.Name}", Category.Connections);
		PlayerList.Instance.Remove(conn);
	}

	private void OnLevelFinishedLoading(Scene oldScene, Scene newScene)
	{
		if (newScene.name != "Lobby")
		{
			//INGAME:
			// TODO check if this is needed
			// EventManager.Broadcast(EVENT.RoundStarted);
			StartCoroutine(DoHeadlessCheck());

		}
	}

	private IEnumerator DoHeadlessCheck()
	{
		yield return WaitFor.Seconds(0.1f);
		if (!GameData.IsHeadlessServer && !GameData.Instance.testServer)
		{
			if (!IsClientConnected())
			{
				//				if (GameData.IsInGame) {
				//					UIManager.Display.logInWindow.SetActive(true);
				//				}
				UIManager.Display.jobSelectWindow.SetActive(false);
			}
		}
		else
		{
			//Set up for headless mode stuff here
			//Useful for turning on and off components
			_isServer = true;
		}
	}

	//Editor item transform dance experiments
#if UNITY_EDITOR
	public void MoveAll()
	{
		StartCoroutine(TransformWaltz());
	}

	private IEnumerator TransformWaltz()
	{
		CustomNetTransform[] scripts = FindObjectsOfType<CustomNetTransform>();
		var sequence = new []
		{
			Vector3.right, Vector3.up, Vector3.left, Vector3.down,
				Vector3.down, Vector3.left, Vector3.up, Vector3.right
		};
		for (var i = 0; i < sequence.Length; i++)
		{
			for (var j = 0; j < scripts.Length; j++)
			{
				NudgeTransform(scripts[j], sequence[i]);
			}
			yield return WaitFor.Seconds(1.5f);
		}
	}

	private static void NudgeTransform(CustomNetTransform netTransform, Vector3 where)
	{
		netTransform.SetPosition(netTransform.ServerState.Position + where);
	}
#endif
}