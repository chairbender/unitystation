
using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles animation of burning prefabs, which are injected into burning objects.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Burning : MonoBehaviour
{
	[Tooltip("Seconds between sprite changes")]
	public float AnimationSpeed = 0.1f;
	[Tooltip("Possible burning sprites for this prefab")]
	public Sprite[] sprites;

	private bool burn;

	private SpriteRenderer spriteRenderer;

	private float animSpriteTime;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Burn()
	{
		spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
		spriteRenderer.enabled = true;
		burn = true;
	}

	public void StopBurning()
	{
		spriteRenderer.sprite = null;
		spriteRenderer.enabled = false;
		burn = false;
	}

	private void Update()
	{
		if (!burn) return;
		animSpriteTime += Time.deltaTime;
		if (animSpriteTime > AnimationSpeed)
		{
			animSpriteTime = 0f;
			spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
		}
	}
}
