﻿using UnityEngine;

public class BodyPartBehaviour : MonoBehaviour
{
	//Different types of damages for medical.
	private float bruteDamage;
	private float burnDamage;
	public float BruteDamage { get { return bruteDamage; } set { bruteDamage = Mathf.Clamp(value, 0, 101); } }
	public float BurnDamage { get { return burnDamage; } set { burnDamage = Mathf.Clamp(value, 0, 101); } }
	public Sprite BlueDamageMonitorIcon;
	public Sprite GreenDamageMonitorIcon;
	public Sprite YellowDamageMonitorIcon;
	public Sprite OrangeDamageMonitorIcon;
	public Sprite DarkOrangeDamageMonitorIcon;
	public Sprite RedDamageMonitorIcon;
	public Sprite GrayDamageMonitorIcon;
	private int MaxDamage = 100;
	public BodyPartType Type;

	public DamageSeverity Severity; //{ get; private set; }
	public float OverallDamage => BruteDamage + BurnDamage;

	void Start()
	{
		UpdateIcons();
	}

	//Apply damages from here.
	public virtual void ReceiveDamage(DamageType damageType, float damage)
	{
		UpdateDamage(damage, damageType);
		Logger.LogTraceFormat("{0} received {1} {2} damage. Total {3}/{4}, limb condition is {5}", Category.Health, Type, damage, damageType, damage, MaxDamage, Severity);
	}

	private void UpdateDamage(float damage, DamageType type)
	{
		switch (type)
		{
			case DamageType.Brute:
				BruteDamage += damage;
				break;

			case DamageType.Burn:
				BurnDamage += damage;
				break;
		}
		UpdateSeverity();
	}

	//Restore/heal damage from here
	public virtual void HealDamage(int damage, DamageType type)
	{
		switch (type)
		{
			case DamageType.Brute:
				BruteDamage -= damage;
				break;

			case DamageType.Burn:
				BurnDamage -= damage;
				break;
		}
		UpdateSeverity();
	}

	private void UpdateIcons()
	{
		if (!IsLocalPlayer())
		{
			return;
		}
		UIManager.PlayerHealthUI.SetBodyTypeOverlay(this);
	}

	protected bool IsLocalPlayer()
	{
		//kinda crappy way to determine local player,
		//but otherwise UpdateIcons would have to be moved to HumanHealthBehaviour
		return PlayerManager.LocalPlayerScript == gameObject.GetComponentInParent<PlayerScript>();
	}

	private void UpdateSeverity()
	{
		// update UI limbs depending on their severity of damage
		float severity = (float)OverallDamage / MaxDamage;
		if (severity <= 0)
		{
			Severity = DamageSeverity.None;
		}
		else if (severity <= 0.2) 
		{
			Severity = DamageSeverity.Light;
		}
		else if (severity <= 0.4)
		{
			Severity = DamageSeverity.Moderate;
		}
		else if (severity <= 0.6)
		{
			Severity = DamageSeverity.Bad;
		}
		else if (severity <= 0.8)
		{
			Severity = DamageSeverity.Critical;
		}
		else if (severity <= 1f)
		{
			Severity = DamageSeverity.Max;
		}

		UpdateIcons();
	}

	public virtual void RestoreDamage()
	{
		bruteDamage = 0;
		burnDamage = 0;
		UpdateSeverity();
	}

	// --------------------
	// UPDATES FROM SERVER
	// --------------------
	public void UpdateClientBodyPartStat(float _bruteDamage, float _burnDamage)
	{
		bruteDamage = _bruteDamage;
		burnDamage = _burnDamage;
		UpdateSeverity();
	}
}