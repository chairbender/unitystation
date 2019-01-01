﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APC : NetworkBehaviour, IElectricalNeedUpdate, IDeviceControl
{
	public PoweredDevice poweredDevice;

	Sprite[] loadedScreenSprites;

	public Sprite[] redSprites;
	public Sprite[] blueSprites;
	public Sprite[] greenSprites;

	public Sprite deadSprite;

	public SpriteRenderer screenDisplay;

	public dynamic dynamicVariable = 1;


	public List<EmergencyLightAnimator> ListOfEmergencyLights = new List<EmergencyLightAnimator>();

	//public List<LightSwitchTrigger> ListOfLightSwitchTriggers = new List<LightSwitchTrigger>();
	//public List<LightSource> ListOfLights = new List<LightSource>();
	public Dictionary<LightSwitchTrigger,List<LightSource>> DictionarySwitchesAndLights = new Dictionary<LightSwitchTrigger,List<LightSource>> ();

	private bool SelfDestruct = false;

	private int displayIndex = 0; //for the animation

	[SyncVar(hook = "UpdateDisplay")]
	public float Voltage;

	public float Resistance = 0;
	public float PreviousResistance = 0;
	private Resistance resistance = new Resistance();

	public PowerTypeCategory ApplianceType = PowerTypeCategory.APC;
	public HashSet<PowerTypeCategory> CanConnectTo = new HashSet<PowerTypeCategory>()
	{
		PowerTypeCategory.LowMachineConnector
	};

	//green - fully charged and sufficient power from wire
	//blue - charging, sufficient power from wire
	//red - running off internal battery, not enough power from wire

	public override void OnStartClient()
	{
		base.OnStartClient();
		poweredDevice.InData.CanConnectTo = CanConnectTo;
		poweredDevice.InData.Categorytype = ApplianceType;
		poweredDevice.DirectionStart = 0;
		poweredDevice.DirectionEnd = 9;
		resistance.Ohms = Resistance;
		ElectricalSynchronisation.PoweredDevices.Add(this);
		PowerInputReactions PRLCable = new PowerInputReactions();
		PRLCable.DirectionReaction = true;
		PRLCable.ConnectingDevice = PowerTypeCategory.LowMachineConnector;
		PRLCable.DirectionReactionA.AddResistanceCall.ResistanceAvailable = true;
		PRLCable.DirectionReactionA.YouShallNotPass = true;
		PRLCable.ResistanceReaction = true;
		PRLCable.ResistanceReactionA.Resistance = resistance;
		poweredDevice.InData.ConnectionReaction[PowerTypeCategory.LowMachineConnector] = PRLCable;
		poweredDevice.InData.ControllingDevice = this;
		poweredDevice.InData.ControllingUpdate = this;
		StartCoroutine(ScreenDisplayRefresh());
		UpdateDisplay(Voltage);
	}

	private void OnDisable()
	{
		ElectricalSynchronisation.PoweredDevices.Remove(this);
	}

	public void PotentialDestroyed()
	{
		if (SelfDestruct)
		{
			//Then you can destroy
		}
	}

	public void PowerUpdateStructureChange() { }
	public void PowerUpdateStructureChangeReact() { }
	public void InitialPowerUpdateResistance() {
		foreach (KeyValuePair<IElectricityIO,HashSet<PowerTypeCategory>> Supplie in poweredDevice.Data.ResistanceToConnectedDevices) {
			poweredDevice.ResistanceInput(ElectricalSynchronisation.currentTick, 1.11111111f, Supplie.Key.GameObject(), null);
			ElectricalSynchronisation.NUCurrentChange.Add (Supplie.Key.InData.ControllingUpdate);
		}
	}
	public void PowerUpdateResistanceChange() { 
		foreach (KeyValuePair<IElectricityIO,HashSet<PowerTypeCategory>> Supplie in poweredDevice.Data.ResistanceToConnectedDevices) {
			poweredDevice.ResistanceInput(ElectricalSynchronisation.currentTick, 1.11111111f, Supplie.Key.GameObject(), null);
			ElectricalSynchronisation.NUCurrentChange.Add (Supplie.Key.InData.ControllingUpdate);
		}
		
	}
	public void PowerUpdateCurrentChange()
	{

	}

	public void PowerNetworkUpdate()
	{
		Voltage = poweredDevice.Data.ActualVoltage;
		UpdateLights();
		if (Resistance != PreviousResistance)
		{
			if (Resistance == 0 || double.IsInfinity(Resistance)) {
				Resistance = 9999999999;
			}
			PreviousResistance = Resistance;
			resistance.Ohms = Resistance;
			ElectricalSynchronisation.ResistanceChange.Add (this);
		}
	}
	public void UpdateLights()
	{
		float CalculatingResistance = new float();
		foreach (KeyValuePair<LightSwitchTrigger,List<LightSource>> SwitchTrigger in  DictionarySwitchesAndLights) {
			SwitchTrigger.Key.PowerNetworkUpdate (Voltage);
			if (SwitchTrigger.Key.isOn) {
				for (int i = 0; i < SwitchTrigger.Value.Count; i++)
				{
					SwitchTrigger.Value[i].PowerLightIntensityUpdate(Voltage);
					CalculatingResistance += (1/SwitchTrigger.Value [i].Resistance);
				}
			}

		}
		Resistance = (1 / CalculatingResistance);
	}

	void UpdateDisplay(float voltage)
	{
		Voltage = voltage;
		ToggleEmergencyLights(voltage);
		if (Voltage == 0)
		{
			loadedScreenSprites = null; // dead
		}
		if (Voltage >= 40f && Voltage < 219f)
		{
			loadedScreenSprites = blueSprites;
		}
		if (Voltage > 219f)
		{
			loadedScreenSprites = greenSprites;
		}
		if (Voltage < 40f && Voltage > 0f)
		{
			loadedScreenSprites = redSprites;
		}
	}

	void ToggleEmergencyLights(float voltage)
	{
		if (ListOfEmergencyLights.Count == 0)
		{
			return;
		}

		for (int i = 0; i < ListOfEmergencyLights.Count; i++)
		{
			ListOfEmergencyLights[i].Toggle(voltage == 0);
		}
	}

	IEnumerator ScreenDisplayRefresh()
	{
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (loadedScreenSprites == null)
				screenDisplay.sprite = deadSprite;
			else
			{
				if (++displayIndex >= loadedScreenSprites.Length)
				{
					displayIndex = 0;
				}
				screenDisplay.sprite = loadedScreenSprites[displayIndex];
			}
			yield return new WaitForSeconds(3f);
		}
	}

	//FIXME: Objects at runtime do not get destroyed. Instead they are returned back to pool
	//FIXME: that also renderers IDevice useless. Please reassess
	public void OnDestroy()
	{
//		ElectricalSynchronisation.StructureChangeReact = true;
//		ElectricalSynchronisation.ResistanceChange = true;
//		ElectricalSynchronisation.CurrentChange = true;
		ElectricalSynchronisation.PoweredDevices.Remove(this);
		SelfDestruct = true;
		//Making Invisible
	}
	public void TurnOffCleanup (){
	}
}