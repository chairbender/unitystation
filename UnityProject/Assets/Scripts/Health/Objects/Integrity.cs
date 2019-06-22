
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
[RequireComponent(typeof(Meleeable))]
public class Integrity : MonoBehaviour, IFireExposable
{

	/// <summary>
	/// Server-side event invoked when object integrity reaches 0 by any means and object
	/// destruction logic is about to be invoked. Does not override the default destruction logic,
	/// simply provides a hook for when it is going to be invoked.
	/// </summary>
	[NonSerialized]
	public DestructionEvent OnWillDestroyServer = new DestructionEvent();

	/// <summary>
	/// Server-side burn up logic - invoked when integrity reaches 0 due to burn damage.
	/// Setting this will override the default burn up logic.
	/// See OnWillDestroyServer if you only want an event when the object is destroyed
	/// and don't want to override the burn up logic.
	/// </summary>
	/// <returns></returns>
	[NonSerialized]
	public UnityAction<DestructionInfo> OnBurnUpServer;

	/// <summary>
	/// Armor for this object. This can be set in inspector but some
	/// components might override this value in code.
	/// </summary>
	[Tooltip("Armor for this object. This can be set in inspector but some" +
	         " components might override this value in code.")]
	public Armor Armor = new Armor();

	/// <summary>
	/// Below this temperature, the object will take no damage from fire or heat.
	/// </summary>
	[Tooltip("Below this temperature, the object will take no damage from fire or heat.")]
	public float HeatResistance = 100;

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
	public void ApplyDamage(float damage, AttackType attackType, DamageType damageType)
	{
		damage = Armor.GetDamage(damage, attackType);
		if (damage > 0)
		{
			integrity -= damage;
			lastDamageType = damageType;
			CheckDestruction();
			Logger.LogTraceFormat("{0} took {1} {2} damage from {3} attack (resistance {4}) (integrity now {5})", Category.Health, name, damage, damageType, attackType, Armor.GetRating(attackType), integrity);
		}
	}


	private void CheckDestruction()
	{
		if (integrity <= 0)
		{
			var destructInfo = new DestructionInfo(lastDamageType);
			OnWillDestroyServer.Invoke(destructInfo);

			if (destructInfo.DamageType == DamageType.Burn)
			{
				if (OnBurnUpServer != null)
				{
					OnBurnUpServer(destructInfo);
				}
				else
				{
					DefaultBurnUp(destructInfo);
				}
			}
			else
			{
				DefaultDestroy(destructInfo);
			}
		}
	}

	private void DefaultBurnUp(DestructionInfo info)
	{
		//just a guess - objects which can be picked up should have a smaller amount of ash
		EffectsFactory.Instance.Ash(registerTile.WorldPosition.To2Int(), GetComponent<Pickupable>() == null);
		ChatRelay.Instance.AddToChatLogServer(new ChatEvent($"{name} burnt to ash.", ChatChannel.Local));
		PoolManager.PoolNetworkDestroy(gameObject);
	}

	private void DefaultDestroy(DestructionInfo info)
	{
		if (info.DamageType == DamageType.Brute)
		{
			ChatRelay.Instance.AddToChatLogServer(new ChatEvent($"{name} was smashed to pieces.", ChatChannel.Local));
			PoolManager.PoolNetworkDestroy(gameObject);
		}
		//TODO: Other damage types (acid)
		else
		{
			ChatRelay.Instance.AddToChatLogServer(new ChatEvent($"{name} was destroyed.", ChatChannel.Local));
			PoolManager.PoolNetworkDestroy(gameObject);
		}
	}

	public void OnExposed(FireExposure exposure)
	{
		if (exposure.Temperature > HeatResistance)
		{
			ApplyDamage(Mathf.Clamp(0.02f * exposure.Temperature, 0f, 20f), AttackType.Fire, DamageType.Burn);
		}
	}

}

/// <summary>
/// Contains info on how an object was destroyed
/// </summary>
public class DestructionInfo
{
	/// <summary>
	/// Damage that destroyed the object
	/// </summary>
	public readonly DamageType DamageType;

	public DestructionInfo(DamageType damageType)
	{
		DamageType = damageType;
	}
}

/// <summary>
/// Event fired when an object is destroyed
/// </summary>
public class DestructionEvent : UnityEvent<DestructionInfo>{}
