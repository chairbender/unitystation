﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// using UnityEngine.Events;

/// <summary>
/// This component allows the object to be disabled with the escape key automatically
/// It pushes the object to the escape key target stack when it's enabled, and pops it when it's disabled.
///
/// This logic takes place in KeyboardInputManager.
/// </summary>
public class EscapeKeyTarget : MonoBehaviour {
	// This component allows the object to be disabled with the escape key automatically

	[Tooltip("What to invoke when this component receives the escape command, other than disabling if DisableOnEscape is true.")]
	public UnityEvent OnEscapeKey = new UnityEvent();

	[Tooltip("If true, disables the game object when escape is recieved after calling OnEscapeKey")]
	public bool DisableOnEscape = true;

	/// <summary>
	/// This is the stack which keeps track of all the game objects so they can be closed later
	/// </summary>
	[HideInInspector]
	public static Stack<GameObject> TargetStack = new Stack<GameObject>();

	// void Awake()
	// {
	// 	if (OnEscapeKey == null)
	// 	{
	// 		// If no function is provided in the editor,
	// 		// perform the default click and disable the object
	// 		OnEscapeKey.AddListener(DisableObject);
	// 	}
	// }
	void OnEnable()
	{
		// Add this game object to the top of the stack so Esc will close it next
		TargetStack.Push(gameObject);
		// Logger.Log("Pushing escape key target stack: " + TargetStack.Peek().name, Category.UI);
	}
	void OnDisable()
	{
		// Revert back to the previous escape key target
		// Logger.Log("Popping escape key target stack: " + TargetStack.Peek().name, Category.UI);
		TargetStack.Pop();
	}

	// void DisableObject()
	// {
	// 	SoundManager.Play("Click01");
	// 	Logger.Log("Disabling " + gameObject.name, Category.UI);
	// 	gameObject.SetActive(false);
	// }

	// public void Trigger()
	// {
	// 	OnEscapeKey.Invoke();
	// }
}
