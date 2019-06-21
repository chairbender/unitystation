
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component which allows an object to have an integrity value (basically an object's version of HP),
/// take damage, and do things in response to integrity changes. Objects are destroyed when their integrity
/// reaches 0.
///
/// This stuff is tracked server side only, client is informed only when the effects of integrity
/// changes occur.
/// </summary>
[RequireComponent(typeof(CustomNetTransform))]
[RequireComponent(typeof(RegisterTile))]
public class Integrity : MonoBehaviour, IFireExposable
{
	/// <summary>
	/// Server-side callback to invoke when object integrity reaches 0 due to fire damage.
	/// By default, the object is destroyed and leaves ash behind. Replacing this
	/// with a different action will override this behavior.
	/// </summary>
	[NonSerialized]
	public UnityAction OnBurnUpServer;

	private float integrity = 100f;
	private DamageType lastDamageType;
	private RegisterTile registerTile;

	private void Awake()
	{
		registerTile = GetComponent<RegisterTile>();
	}

	/// <summary>
	/// Directly deal damage to this object.
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="damageType"></param>
	public void ApplyDamage(float damage, DamageType damageType)
	{
		integrity -= damage;
		lastDamageType = damageType;
		CheckDestruction();
		Logger.LogTraceFormat("{0} took {1} {2} damage", Category.Health, name, damage, damageType);
	}


	private void CheckDestruction()
	{
		if (integrity <= 0)
		{
			if (lastDamageType == DamageType.Burn)
			{
				BurnUp();
			}
			//TODO: Other damage types
		}
	}

	public void OnExposed(FireExposure exposure)
	{
		ApplyDamage(Mathf.Clamp(0.02f * exposure.Temperature, 0f, 20f), DamageType.Burn);
	}

	/// <summary>
	/// Invoked when object integrity reaches 0 due to burn damage
	/// </summary>
	private void BurnUp()
	{
		if (OnBurnUpServer != null)
		{
			OnBurnUpServer();
		}
		else
		{
			DefaultBurnUp();
		}
	}

	/// <summary>
	/// Default burn logic, destroys object and leaves ash decal
	/// </summary>
	private void DefaultBurnUp()
	{
		EffectsFactory.Instance.Ash(registerTile.WorldPosition.To2Int(), false);
		GetComponent<CustomNetTransform>().DisappearFromWorldServer();
	}
}
