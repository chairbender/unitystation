
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IngameDebugConsole;
using UnityEngine;
using Mirror;
using UnityEngine.Serialization;
using Random = System.Random;

/// <summary>
/// Various attributes associated with a particular item.
/// New and improved, removes need for UniCloth type stuff, works
/// well with using prefab variants.
/// </summary>
[RequireComponent(typeof(SpriteDataHandler))]
[RequireComponent(typeof(Pickupable))]
[RequireComponent(typeof(ObjectBehaviour))]
[RequireComponent(typeof(RegisterItem))]
[RequireComponent(typeof(CustomNetTransform))]
public class ItemAttributesV2 : MonoBehaviour, IRightClickable
{
	[Tooltip("Display name of this item.")]
	[SerializeField]
	private string itemName;

	[Tooltip("Description of this item.")]
	[SerializeField]
	private string itemDescription;

	[SerializeField]
	[Tooltip("Initial traits of this item on spawn.")]
	private List<ItemTrait> initialTraits;

	[Tooltip("Initial size of this item on spawn.")]
	[SerializeField]
	private ItemSize initialSize;

	//TODO: This shouldn't be needed
	//public SpriteType spriteType;

	[Tooltip("Damage when we click someone with harm intent")]
	[Range(0, 100)]
	[SerializeField]
	private float hitDamage = 0;

	[Tooltip("Type of damage done when this is thrown or used for melee.")]
	[SerializeField]
	private DamageType damageType = DamageType.Brute;

	[Tooltip("How painful it is when someone throws it at you")]
	[Range(0, 100)]
	[SerializeField]
	private float throwDamage = 0;

	[Tooltip("How many tiles to move per 0.1s when being thrown")]
	[SerializeField]
	private float throwSpeed = 2;

	[Tooltip("Max throw distance")]
	[SerializeField]
	private float throwRange = 7;

	[Tooltip("Sound to be played when we click someone with harm intent")]
	[SerializeField]
	private string hitSound = "GenericHit";

	[Tooltip("Is this a mask that can connect to a tank?")]
	[SerializeField]
	private bool canConnectToTank;

	[Tooltip("Possible verbs used to describe the attack when this is used for melee.")]
	[SerializeField]
	private List<string> attackVerbs;

	/// <summary>
	/// Actual current traits, accounting for dynamic add / remove.
	/// </summary>
	private HashSet<ItemTrait> traits = new HashSet<ItemTrait>();

	private SpriteDataHandler spriteDataHandler;
	private SpriteHandler inventoryIcon;

	///<Summary>
	/// Can this item protect humans against spess?
	///</Summary>
	public bool IsEVACapable { get; private set; }

	private void Awake()
	{
		spriteDataHandler = GetComponentInChildren<SpriteDataHandler>();
		inventoryIcon = GetComponentInChildren<SpriteHandler>();
	}

	/// <summary>
	/// Gets all the traits this object currently has
	/// </summary>
	/// <returns></returns>
	public IEnumerable<ItemTrait> GetTraits()
	{
		return traits;
	}

	/// <summary>
	/// Returns true iff this itemattributes has the specified trait
	/// </summary>
	/// <param name="toCheck"></param>
	/// <returns></returns>
	public bool HasTrait(ItemTrait toCheck)
	{
		return traits.Contains(toCheck);
	}

	/// <summary>
	/// Dynamically adds the specified trait to this item attributes
	/// </summary>
	/// <param name="toAdd"></param>
	public void AddTrait(ItemTrait toAdd)
	{
		traits.Add(toAdd);
	}

	/// <summary>
	/// Dynamically removes the specified trait from this item attributes
	/// </summary>
	/// <param name="toAdd"></param>
	public void RemoveTrait(ItemTrait toRemove)
	{
		traits.Remove(toRemove);
	}

	public void SetUpFromClothingData(EquippedData equippedData)
	{
		spriteDataHandler.Infos = new SpriteData();
		spriteDataHandler.Infos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(equippedData.InHandsLeft));
		spriteDataHandler.Infos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(equippedData.InHandsRight));
		inventoryIcon.Infos = new SpriteData();
		inventoryIcon.Infos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(equippedData.ItemIcon));
		inventoryIcon.PushTexture();
	}

#if UNITY_EDITOR
	public void AttributesFromCD(ItemAttributesData ItemAttributes)
	{
		itemName = ItemAttributes.itemName;
		itemDescription = ItemAttributes.itemDescription;
		var trait = TypeToTrait(ItemAttributes.itemType);
		if (trait != null)
		{
			traits.Add(trait);
		}
		initialSize = ItemAttributes.size;
		canConnectToTank = ItemAttributes.CanConnectToTank;
		hitDamage = ItemAttributes.hitDamage;
		damageType = ItemAttributes.damageType;
		throwDamage = ItemAttributes.throwDamage;
		throwSpeed = ItemAttributes.throwSpeed;
		throwRange = ItemAttributes.throwRange;
		hitSound = ItemAttributes.hitSound;
		attackVerbs = ItemAttributes.attackVerb;
		IsEVACapable = ItemAttributes.IsEVACapable;
	}
	private ItemTrait TypeToTrait(ItemType itemType)
	{
		return ItemTypeToTraitMapping.Instance.GetTrait(itemType);
	}
#endif

	private static string GetMasterTypeHandsString(SpriteType masterType)
	{
		switch (masterType)
		{
			case SpriteType.Clothing: return "clothing";

			default: return "items";
		}
	}


	public void OnHoverStart()
	{
		// Show the parenthesis for an item's description only if the item has a description
		UIManager.SetToolTip =
			itemName +
			(String.IsNullOrEmpty(itemDescription) ? "" : $" ({itemDescription})");
	}

	public void OnHoverEnd()
	{
		UIManager.SetToolTip = String.Empty;
	}

	// Sends examine event to all monobehaviors on gameobject
	public void SendExamine()
	{
		SendMessage("OnExamine");
	}

	// When right clicking on an item, examine the item
	public void OnHover()
	{
		if (CommonInput.GetMouseButtonDown(1))
		{
			SendExamine();
		}
	}

	private void OnExamine()
	{
		if (!string.IsNullOrEmpty(itemDescription))
		{
			Chat.AddExamineMsgToClient(itemDescription);
		}
	}

	public RightClickableResult GenerateRightClickOptions()
	{
		return RightClickableResult.Create()
			.AddElement("Examine", OnExamine);
	}

}