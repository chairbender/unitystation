using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUI_DevCloner : MonoBehaviour
{

	private enum State
	{
		INACTIVE,
		SELECTING,
		DRAWING
	}

	[Tooltip("Text which shows the current status of the cloner.")]
	public Text statusText;
	[Tooltip("Prefab to use as our cursor when painting.")]
	public GameObject cursorPrefab;

	// objects selectable for cloning
	private LayerMask layerMask;

	//current state
	private State state;

	//object selected for cloning
	private GameObject toClone;
	//cursor object showing the item to be spawned
	private GameObject cursorObject;
	// so we can escape while drawing - enabled while drawing, disabled when done
	private EscapeKeyTarget escapeKeyTarget;

	void Awake()
	{
		ToState(State.SELECTING);
		layerMask = LayerMask.GetMask("Furniture", "Machines", "Items",
			"Objects");
		escapeKeyTarget = GetComponent<EscapeKeyTarget>();
	}

	private void ToState(State newState)
	{
		if (newState == state)
		{
			return;
		}
		if (state == State.DRAWING)
		{
			//stop drawing
			Destroy(cursorObject);
		}

		if (newState == State.SELECTING)
		{
			statusText.text = "Click to select object to clone (ESC to Cancel)";
			UIManager.IsMouseInteractionDisabled = true;

		}
		else if (newState == State.DRAWING)
		{
			statusText.text = "Click to spawn selected object (ESC to Cancel)";
			UIManager.IsMouseInteractionDisabled = true;
			//just chosen to be spawned on the map. Put our object under the mouse cursor
			cursorObject = Instantiate(cursorPrefab, transform.root);
			cursorObject.GetComponent<SpriteRenderer>().sprite = toClone.GetComponentInChildren<SpriteRenderer>().sprite;
		}
		else if (newState == State.INACTIVE)
		{
			statusText.text = "Click to select object to clone (ESC to Cancel)";
			UIManager.IsMouseInteractionDisabled = false;
			gameObject.SetActive(false);
		}

		state = newState;
	}

	public void OnEscape()
	{
		if (state == State.DRAWING)
		{
			ToState(State.SELECTING);
		}
		else if (state == State.SELECTING)
		{
			ToState(State.INACTIVE);
		}

	}

	private void OnDisable()
	{
		ToState(State.INACTIVE);
	}

	public void Open()
	{
		ToState(State.SELECTING);
	}

	private void Update()
	{
		if (state == State.SELECTING)
		{
			// ignore when we are over UI
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			//check which objects we are over, pick the top one to spawn
			if (CommonInput.GetMouseButtonDown(0))
			{
				SpriteRenderer hitRender = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(CommonInput.mousePosition), Vector2.zero, 10f, layerMask)
					//get the hit game object
					.Select(hit => hit.transform.gameObject)
					//check if there is a CNT
					.Where(go => go.GetComponent<CustomNetTransform>() != null)
					//sprite renderer so we can order by layer and figure out which was on top
					.Select(go => go.GetComponentInChildren<SpriteRenderer>())
					.Where(r => r != null)
					//order by sort layer
					.OrderByDescending(r => SortingLayer.GetLayerValueFromID(r.sortingLayerID))
					//then by sort order
					.ThenByDescending(renderer => renderer.sortingOrder)
					.First();
				if (hitRender != null)
				{
					toClone = hitRender.GetComponentInParent<CustomNetTransform>().gameObject;
					ToState(State.DRAWING);
				}

			}

		}
		else if (state == State.DRAWING)
		{
			cursorObject.transform.position = Camera.main.ScreenToWorldPoint(CommonInput.mousePosition);
			if (CommonInput.GetMouseButtonDown(0))
			{
				Vector3Int position = cursorObject.transform.position.RoundToInt();
				position.z = 0;
				if (MatrixManager.IsPassableAt(position))
				{
					if (CustomNetworkManager.IsServer)
					{
						PoolManager.NetworkClone(toClone, position);
					}
					else
					{
						DevCloneMessage.Send(toClone, (Vector3) position);
					}
				}
			}
		}
	}
}
