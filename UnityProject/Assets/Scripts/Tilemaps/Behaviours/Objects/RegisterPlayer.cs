using UnityEngine;


[ExecuteInEditMode]
public class RegisterPlayer : RegisterTile
{
	/// <summary>
	/// Cached playerHealth on this player object
	/// </summary>
	private PlayerHealth playerHealth;

	/// <summary>
	/// Whether the player should currently be depicted laying on the ground
	/// </summary>
	private bool isDown;

	public bool IsBlocking { get; set; } = true;

	private void Awake()
	{
		playerHealth = GetComponent<PlayerHealth>();
		//initially we are upright and don't rotate with the matrix
		rotateWithMatrix = false;
	}

	public override bool IsPassable()
	{
		return !IsBlocking;
	}

	public override bool IsPassable(Vector3Int from)
	{
		return IsPassable();
	}

	protected override void OnRotationEnd(RotationOffset fromCurrent, bool isInitialRotation)
	{
		base.OnRotationEnd(fromCurrent, isInitialRotation);

		//add additional rotation to remain sideways if we are down
		if (isDown)
		{
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.transform.Rotate(0, 0, -90);
			}
		}
	}

	/// <summary>
	/// Cause the player sprite to appear as laying down, which also causes them to rotate with the matrix and become
	/// passable
	/// </summary>
	public void LayDown()
	{
		if (!isDown)
		{
			isDown = true;
			IsBlocking = false;
			//rotate the sprites and change their layer
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.transform.rotation = Quaternion.identity;
				spriteRenderer.transform.Rotate(0, 0, -90);
				spriteRenderer.sortingLayerName = "Blood";
			}
		}

	}

	/// <summary>
	/// Causes the player sprite to stand upright, causing them to not rotate with th matrix anymore and become
	/// blocking again.
	/// </summary>
	public void GetUp()
	{
		if (isDown)
		{

			isDown = false;
			IsBlocking = true;
			//change sprites to be upright
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.transform.rotation = Quaternion.identity;
				spriteRenderer.sortingLayerName = "Players";
			}
		}
	}
}