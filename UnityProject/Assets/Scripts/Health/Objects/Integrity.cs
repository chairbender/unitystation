
using UnityEngine;

/// <summary>
/// Component which allows an object to have an integrity value (basically an object's version of HP),
/// take damage, and do things in response to integrity changes. Objects are destroyed when their integrity
/// reaches 0.
///
/// This stuff is tracked server side.
/// </summary>
public class Integrity : MonoBehaviour, IFireExposable
{
	private float integrity = 100f;

	public void OnExposed(FireExposure exposure)
	{
		integrity -= Mathf.Clamp(0.02f * exposure.Temperature, 0f, 20f);
		CheckDestruction();
	}

	private void CheckDestruction()
	{
		if (integrity <= 0)
		{
			//destroy the object
			//TODO: Leave some ash?
		}
	}
}
