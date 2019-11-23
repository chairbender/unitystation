using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Indicates that a given item functions as an article of clothing. Contains the data related to the clothing / how it should look.
/// New and improved, removes need for UniCloth type stuff, works
/// well with using prefab variants.
/// </summary>
public class ClothingV2 : MonoBehaviour
{

	//TODO: This can probably be migrated to this component rather than using a separate SO, since
	//there's probably no situation where we'd want to re-use the same cloth data on more than one item.
	[Tooltip("Clothing data describing the various sprites for this clothing.")]
	[SerializeField]
	private BaseClothData clothData;

	[Tooltip("Index of the variant to use. -1 for default.")]
	[SerializeField]
	private int variantIndex = -1;

	[Tooltip("Type of variant to use of the clothing data.")]
	[SerializeField]
	private ClothingVariantType variantType;

	private Dictionary<ClothingVariantType, int> variantStore = new Dictionary<ClothingVariantType, int>();
	private List<int> variantList;
	private SpriteData spriteInfo;

	private Pickupable pickupable;

	/// <summary>
	/// Clothing item this is currently equipped to, if there is one. Will be updated when the data is synced.
	/// </summary>
	private ClothingItem clothingItem;

	/// <summary>
	/// The sprite info for this clothing. Describes all the sprites it has for various situations.
	/// </summary>
	public SpriteData SpriteInfo => spriteInfo;

	//TODO: Refactor this, quite confusing
	/// <summary>
	/// The index of the sprites that should currently be used for rendering this, an index into SpriteData.List
	/// </summary>
	public int SpriteInfoState => variantStore.ContainsKey(variantType) ? variantStore[variantType] : 0;

	private void Awake()
	{
		pickupable = GetComponent<Pickupable>();
		TryInit();
	}

	//set up the sprites of this instance using the cloth data
	private void TryInit()
	{
		if (clothData is ClothingData clothingData)
		{
			var item = GetComponent<ItemAttributesV2>();
			spriteInfo = SetUpSheetForClothingData(clothingData);
			item.SetUpFromClothingData(clothingData.Base);

			switch (variantType)
			{
				case ClothingVariantType.Default:
					if (variantIndex > -1)
					{
						if (!(clothingData.Variants.Count >= variantIndex))
						{
							item.SetUpFromClothingData(clothingData.Variants[variantIndex]);
						}
					}

					break;
				case ClothingVariantType.Skirt:
					item.SetUpFromClothingData(clothingData.DressVariant);
					break;
				case ClothingVariantType.Tucked:
					item.SetUpFromClothingData(clothingData.Base_Adjusted);
					break;
			}
		}
		else if (clothData is ContainerData containerData)
		{
			var Item = GetComponent<ItemAttributesV2>();
			this.spriteInfo = StaticSpriteHandler.SetupSingleSprite(containerData.Sprites.Equipped);
			Item.SetUpFromClothingData(containerData.Sprites);
		}
		else if (clothData is HeadsetData headsetData)
		{
			var Item = GetComponent<ItemAttributesV2>();
			var Headset = GetComponent<Headset>();
			this.spriteInfo = StaticSpriteHandler.SetupSingleSprite(headsetData.Sprites.Equipped);
			Item.SetUpFromClothingData(headsetData.Sprites);
			Headset.EncryptionKey = headsetData.Key.EncryptionKey;
		}
	}

	private SpriteData SetUpSheetForClothingData(ClothingData clothingData)
	{
		var SpriteInfos = new SpriteData();
		SpriteInfos.List = new List<List<List<SpriteDataHandler.SpriteInfo>>>();
		int c = 0;

		SpriteInfos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(clothingData.Base.Equipped));
		variantStore[ClothingVariantType.Default] = c;
		c++;

		if (clothingData.Base_Adjusted.Equipped.Texture != null)
		{
			SpriteInfos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(clothingData.Base_Adjusted.Equipped));
			variantStore[ClothingVariantType.Tucked] = c;
			c++;
		}

		if (clothingData.DressVariant.Equipped.Texture != null)
		{
			SpriteInfos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(clothingData.DressVariant.Equipped));
			variantStore[ClothingVariantType.Skirt] = c;
			c++;
		}
		if (clothingData.Variants.Count > 0)
		{
			foreach (var Variant in clothingData.Variants)
			{
				SpriteInfos.List.Add(StaticSpriteHandler.CompleteSpriteSetup(Variant.Equipped));
				variantStore[ClothingVariantType.Skirt] = c;
				c++;
			}
		}
		return (SpriteInfos);
	}

	public void SetClothingItem(ClothingItem clothingItem)
	{
		this.clothingItem = clothingItem;
		RefreshClothingItem();
	}

	private void RefreshClothingItem()
	{
		if (clothingItem != null)
		{
			clothingItem.RefreshFromClothing(this);
		}
	}
}