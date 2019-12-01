
using UnityEngine;

/// <summary>
/// Base class for the trait system. Defines a particular
/// trait than an item can have (assigned in ItemAttributes)
/// </summary>
[CreateAssetMenu(fileName = "ItemTrait", menuName = "Traits/ItemTrait")]
public class ItemTrait : ScriptableObject
{
	[Tooltip("A short description of the trait and what it does. For documentation only, not used for any game logic.")]
	[TextArea]
	[SerializeField]
	private string traitDescription = "Describe me!"; // A short description of the trait and what it does
}
