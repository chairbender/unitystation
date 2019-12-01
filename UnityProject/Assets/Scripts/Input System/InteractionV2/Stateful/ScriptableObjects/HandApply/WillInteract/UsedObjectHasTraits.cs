
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Checks if the used object meets the particular trait criteria.
/// </summary>
[CreateAssetMenu(fileName = "UsedObjectHasTraits", menuName = "Interaction/HandApply/WillInteract/UsedObjectHasTraits")]
public class UsedObjectHasTraits : BaseWillHandApply
{
	[SerializeField]
	[Tooltip("Used objects which have at least one of these traits will be allowed, provided they also have all" +
	         " the required traits. If empty, used objects will be allowed as long as they" +
	         " don't have blacklisted traits and have all required traits.")]
	private List<ItemTrait> Whitelist;

	[SerializeField]
	[Tooltip("Used object MUST have ALL of these traits in order to succeed.")]
	private List<ItemTrait> Required;

	[SerializeField]
	[Tooltip("Used objects with any of these traits will be disallowed, regardless of if they have the" +
	         " required traits or whitelisted traits (blacklist takes priority over whitelist). " +
	         "If blank, has no effect on validation logic.")]
	private List<ItemTrait> Blacklist;

	public override bool WillInteract(GameObject processorObject, HandApply interaction, NetworkSide side)
	{

		if (interaction.UsedObject == null) return false;

		var itemAttrs = interaction.UsedObject.GetComponent<ItemAttributesV2>();
		if (Required != null && Required.Count > 0)
		{
			Logger.LogTraceFormat("Requirements are {0}", Category.Interaction,
				String.Join(", ", Required.Select(it => it.name)));
			if (itemAttrs == null)
			{
				Logger.LogTrace("Item has no ItemAttributes, thus cannot meet the requirements ", Category.Interaction);
				return false;
			}
			//requirements are defined, check them
			foreach (var requiredTrait in Required)
			{
				if (!itemAttrs.HasTrait(requiredTrait))
				{
					Logger.LogTraceFormat("Item doesn't have required trait {0}", Category.Interaction, requiredTrait.name);
					return false;
				}
			}
		}

		if (Blacklist != null && Blacklist.Count > 0)
		{
			Logger.LogTraceFormat("Blacklist is {0}", Category.Interaction,
				String.Join(", ", Blacklist.Select(it => it.name)));
			if (itemAttrs == null)
			{
				Logger.LogTrace("Item has no ItemAttributes, thus cannot be blacklisted", Category.Interaction);
			}
			else
			{
				foreach (var blacklistTrait in Blacklist)
				{
					if (itemAttrs.HasTrait(blacklistTrait))
					{
						Logger.LogTraceFormat("Item has blacklisted trait {0}", Category.Interaction, blacklistTrait.name);
						return false;
					}
				}
			}
		}

		if (Whitelist == null || Whitelist.Count == 0)
		{
			Logger.LogTrace("No whitelist defined, item is allowed.", Category.Interaction);
			return true;
		}
		else
		{
			Logger.LogTraceFormat("Whitelist is {0}", Category.Interaction,
				String.Join(", ", Whitelist.Select(it => it.name)));
			if (itemAttrs == null)
			{
				Logger.LogTrace("Item has no ItemAttributes, thus has no whitelisted traits", Category.Interaction);
				return false;
			}
			foreach (var whitelistTrait in Whitelist)
			{
				if (itemAttrs.HasTrait(whitelistTrait))
				{
					Logger.LogTraceFormat("Item has whitelisted trait {0}", Category.Interaction, whitelistTrait.name);
					return true;
				}
			}

			Logger.LogTrace("Item has no whitelisted traits", Category.Interaction);
			return false;
		}
	}
}
