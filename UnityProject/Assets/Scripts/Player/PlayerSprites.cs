using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handle displaying the sprites related to player, which includes clothing, the body,
/// and the ghost when the player is a ghost.
/// </summary>
public class PlayerSprites : NetworkBehaviour
{
	private readonly Dictionary<string, ClothingItem> clothes = new Dictionary<string, ClothingItem>();
	[SyncVar(hook = nameof(FaceBodyDirectionSync))]
	public Orientation currentBodyDirection;
	[SyncVar(hook = nameof(FaceGhostDirectionSync))]
	public Orientation currentGhostDirection;
	[SyncVar(hook = nameof(UpdateCharacterSprites))]
	private string characterData;

	public PlayerMove playerMove;

	public ClothingItem[] characterSprites; //For character customization
	private CharacterSettings characterSettings;
	private RegisterTile registerTile;
	private PlayerHealth playerHealth;

	/// <summary>
	/// true iff we are in the middle of a matrix rotation (between OnRotationStart and OnRotationEnd)
	/// </summary>
	private bool isMatrixRotating;
	/// <summary>
	/// Destination orientation we will rotate to when OnRotationEnd happens
	/// </summary>
	private Orientation destinationOrientation;

	private SpriteRenderer ghostRenderer; //For ghost sprites
	private readonly Dictionary<Orientation, Sprite> ghostSprites = new Dictionary<Orientation, Sprite>();

	private void Awake()
	{
		foreach (ClothingItem c in GetComponentsInChildren<ClothingItem>())
		{
			clothes[c.name] = c;
		}

		ghostSprites.Add(Orientation.Down, SpriteManager.PlayerSprites["mob"][268]);
		ghostSprites.Add(Orientation.Up, SpriteManager.PlayerSprites["mob"][269]);
		ghostSprites.Add(Orientation.Right, SpriteManager.PlayerSprites["mob"][270]);
		ghostSprites.Add(Orientation.Left, SpriteManager.PlayerSprites["mob"][271]);

		ghostRenderer = transform.Find("Ghost").GetComponent<SpriteRenderer>();

		registerTile = GetComponent<RegisterTile>();
		playerHealth = GetComponent<PlayerHealth>();

		//Sub to matrix rotation events via the registerTile because it always has the
		//correct matrix
		registerTile.OnRotateStart.AddListener(OnRotationStart);
		registerTile.OnRotateEnd.AddListener(OnRotationEnd);

	}

	private void OnDisable()
	{
		registerTile.OnRotateStart.RemoveListener(OnRotationStart);
		registerTile.OnRotateEnd.RemoveListener(OnRotationEnd);
	}

	public override void OnStartServer()
	{
		LocalFaceDirection(Orientation.Down);
		base.OnStartServer();
	}

	public override void OnStartClient()
	{
		StartCoroutine(WaitForLoad());
		base.OnStartClient();
	}

	[Command]
	private void CmdUpdateCharacter(string data)
	{
		var character = JsonUtility.FromJson<CharacterSettings>(data);
		//Remove sensitive data:
		character.username = "";
		characterSettings = character;
		characterData = JsonUtility.ToJson(character);
	}
	private void UpdateCharacterSprites(string data)
	{

		var character = JsonUtility.FromJson<CharacterSettings>(data);
		characterSettings = character;

		Color newColor = Color.white;

		//Skintone:
		ColorUtility.TryParseHtmlString(characterSettings.skinTone, out newColor);

		for (int i = 0; i < characterSprites.Length; i++)
		{
			characterSprites[i].spriteRenderer.color = newColor;
			if (i == 6)
			{
				break;
			}
		}
		//Torso
		characterSprites[0].reference = characterSettings.torsoSpriteIndex;
		characterSprites[0].UpdateSprite();
		//Head
		characterSprites[5].reference = characterSettings.headSpriteIndex;
		characterSprites[5].UpdateSprite();
		//Eyes
		ColorUtility.TryParseHtmlString(characterSettings.eyeColor, out newColor);
		characterSprites[6].spriteRenderer.color = newColor;
		//Underwear
		characterSprites[7].reference = characterSettings.underwearOffset;
		characterSprites[7].UpdateSprite();
		//Socks
		characterSprites[8].reference = characterSettings.socksOffset;
		characterSprites[8].UpdateSprite();
		//Beard
		characterSprites[9].reference = characterSettings.facialHairOffset;
		characterSprites[9].UpdateSprite();
		ColorUtility.TryParseHtmlString(characterSettings.facialHairColor, out newColor);
		characterSprites[9].spriteRenderer.color = newColor;
		//Hair
		characterSprites[10].reference = characterSettings.hairStyleOffset;
		characterSprites[10].UpdateSprite();
		ColorUtility.TryParseHtmlString(characterSettings.hairColor, out newColor);
		characterSprites[10].spriteRenderer.color = newColor;
	}

	private IEnumerator WaitForLoad()
	{
		yield return YieldHelper.EndOfFrame;
		if (PlayerManager.LocalPlayer == gameObject)
		{
			CmdUpdateCharacter(JsonUtility.ToJson(PlayerManager.CurrentCharacterSettings));
			LocalFaceDirection( currentBodyDirection );
		}
		while(string.IsNullOrEmpty(characterData)){
			yield return YieldHelper.DeciSecond;
		}
		FaceBodyDirectionSync(currentBodyDirection);
		FaceGhostDirectionSync(currentGhostDirection);
		if (PlayerManager.LocalPlayer != gameObject)
		{
			UpdateCharacterSprites(characterData);
		}
	}

	private void OnRotationStart(RotationOffset fromCurrent, bool isInitialRotation)
	{
		//ignore the initial rotation message as we determine initial rotation from the
		//currentBodyDirection syncvar in playerSprites
		if (!isInitialRotation)
		{
			//determine our destination rotation
			if (playerMove.isGhost)
			{
				destinationOrientation = currentGhostDirection.Rotate(fromCurrent);
			}
			else
			{
				destinationOrientation = currentBodyDirection.Rotate(fromCurrent);
			}

			isMatrixRotating = true;
		}
	}

	private void OnRotationEnd(RotationOffset fromCurrent, bool isInitialRotation)
	{

		//ignore the initial rotation message as we determine initial rotation from the
		//currentBodyDirection syncvar in playerSprites
		if (!isInitialRotation)
		{
			LocalFaceDirection(destinationOrientation);
			isMatrixRotating = false;
		}

	}

	/// <summary>
	/// Change current facing direction to match direction
	/// </summary>
	/// <param name="direction">new direction</param>
	[Command]
	private void CmdChangeDirection(Orientation direction)
	{
		LocalFaceDirection(direction);
	}

	/// <summary>
	/// Locally changes the direction of this player to face the specified direction but doesn't tell the server.
	/// If this is a client, only changes the direction locally and doesn't inform other players / server.
	/// If this is on the server, the direction change will be sent to all clients due to the syncvar.
	///
	/// Changes ghost direction if they are a ghost, otherwise changes body direction.
	/// </summary>
	/// <param name="direction"></param>
	public void LocalFaceDirection(Orientation direction)
	{
		if (playerMove.isGhost)
		{
			SetGhostDir(direction);
		}
		else
		{
			SetBodyDir(direction);
		}
	}

	/// <summary>
	/// Cause player to face in the specified absolute orientation and syncs this change to the server / other
	/// hosts.
	/// </summary>
	/// <param name="newOrientation">new absolute orientation</param>
	public void ChangeAndSyncPlayerDirection(Orientation newOrientation)
	{
		CmdChangeDirection(newOrientation);
		//Prediction
		LocalFaceDirection(newOrientation);
	}

	/// <summary>
	/// Does nothing if this is the local player (unless player is in crit).
	///
	/// Invoked when currentBodyDirection syncvar changes. Update the direction of this player to face the specified
	/// direction. However, if this is the local player's body that is not in crit or a player being pulled by the local player,
	/// nothing is done and we stick with whatever direction we had already set for them locally (this is to avoid
	/// glitchy changes in facing direction caused by latency in the syncvar).
	/// </summary>
	/// <param name="dir"></param>
	private void FaceBodyDirectionSync(Orientation dir)
	{
		//ignore this while we are rotating in a matrix
		if (isMatrixRotating)
		{
			return;
		}
//		//don't sync facing direction for players you're pulling locally, unless you're standing still
		PushPull localPlayer = PlayerManager.LocalPlayerScript ? PlayerManager.LocalPlayerScript.pushPull : null;
		if ( localPlayer && localPlayer.Pushable != null && localPlayer.Pushable.IsMovingClient )
		{
			if ( playerMove && playerMove.PlayerScript && playerMove.PlayerScript.pushPull
			     && playerMove.PlayerScript.pushPull.IsPulledByClient( localPlayer ) ) {
				return;
			}
		}

		//check if we are crit, or else our direction might be out of sync with the server
		if (PlayerManager.LocalPlayer != gameObject || playerHealth.IsCrit)
		{
			currentBodyDirection = dir;
			SetBodyDir(dir);
		}
	}

	/// <summary>
	/// Does nothing if this is the local player.
	///
	/// Invoked when currentBodyDirection syncvar changes. Update the direction of this player ghost to face the specified
	/// direction. However, if this is the local player's ghost nothing is done and we stick with whatever direction we
	/// had already set for them locally (this is to avoid
	/// glitchy changes in facing direction caused by latency in the syncvar).
	/// </summary>
	/// <param name="dir"></param>
	private void FaceGhostDirectionSync(Orientation dir)
	{
		//ignore this while we are rotating in a matrix
		if (isMatrixRotating)
		{
			return;
		}

		if (PlayerManager.LocalPlayer != gameObject)
		{
			currentGhostDirection = dir;
			SetGhostDir(dir);
		}
	}

	/// <summary>
	/// Updates the direction of the body / clothing sprites. This will have no effect if player
	/// is unconscious or dead.
	/// </summary>
	/// <param name="direction"></param>
	private void SetBodyDir(Orientation direction)
	{
		//body direction is never changed while we are unconscious or dead - we lay where we were
		if (playerHealth.ConsciousState != ConsciousState.CONSCIOUS)
		{
			return;
		}
		foreach (ClothingItem c in clothes.Values)
		{
			c.Direction = direction;
		}

		currentBodyDirection = direction;
	}

	/// <summary>
	/// Updates the direction of the ghost if player is a ghost
	/// </summary>
	/// <param name="direction"></param>
	private void SetGhostDir(Orientation direction)
	{
		if (playerMove.isGhost)
		{
			ghostRenderer.sprite = ghostSprites[direction];
			currentGhostDirection = direction;
		}

	}
}