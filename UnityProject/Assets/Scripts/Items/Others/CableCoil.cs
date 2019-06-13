﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cable coil which can be applied to the ground to lay cable.
/// </summary>
[RequireComponent(typeof(Pickupable))]
public class CableCoil : NBPositionalHandApplyInteractable
{
	public WiringColor CableType;
	public GameObject CablePrefab;

	public Connection GetDirectionFromFaceDirection(GameObject originator) {
		var playerScript = originator.GetComponent<PlayerScript>();
		switch (playerScript.CurrentDirection.ToString())
		{
			case "Left":
				{
					return (Connection.West);
				}
			case "Right":
				{

					return (Connection.East);
				}
			case "Up":
				{

					return (Connection.North);
				}
			case "Down":
				{

					return (Connection.South);
				}
		}
		return (Connection.NA);

	}


	protected override InteractionValidationChain<PositionalHandApply> InteractionValidationChain()
	{
		return InteractionValidationChain<PositionalHandApply>.Create()
			//can only be placed on tiles
			.WithValidation(DoesTargetObjectHaveComponent<InteractableTiles>.DOES)
			.WithValidation(IsHand.OCCUPIED)
			.WithValidation(CanApply.ONLY_IF_CONSCIOUS);
	}

	protected override void ServerPerformInteraction(PositionalHandApply interaction)
	{
		var cableCoil = interaction.UsedObject.GetComponent<CableCoil>();
		if (cableCoil != null)
		{
			Vector3Int worldPosInt = interaction.WorldPositionTarget.To2Int().To3Int();
			MatrixInfo matrix = MatrixManager.AtPoint(worldPosInt, true);
			var localPosInt = MatrixManager.WorldToLocalInt(worldPosInt, matrix);
			if (matrix.Matrix != null)
			{
				//can't place wires here
				if (!matrix.Matrix.IsClearUnderfloorConstruction(localPosInt, true))
				{
					return;
				}
			}
			else {
				//no matrix found to place wires in
				return;
			}

			var roundTargetWorldPosition = interaction.WorldPositionTarget.RoundToInt();
			Vector3 PlaceDirection = interaction.Performer.Player().Script.WorldPos - (Vector3)roundTargetWorldPosition;
			Connection WireEndB = Connection.NA;
			if (PlaceDirection == Vector3.up) { WireEndB = Connection.North; }
			else if (PlaceDirection == Vector3.down) { WireEndB = Connection.South; }
			else if (PlaceDirection == Vector3.right) { WireEndB = Connection.East; }
			else if (PlaceDirection == Vector3.left) { WireEndB = Connection.West; }

			else if (PlaceDirection == Vector3.down + Vector3.left) { WireEndB = Connection.SouthWest; }
			else if (PlaceDirection == Vector3.down + Vector3.right) { WireEndB = Connection.SouthEast; }
			else if (PlaceDirection == Vector3.up + Vector3.left) { WireEndB = Connection.NorthWest; }
			else if (PlaceDirection == Vector3.up + Vector3.right) { WireEndB = Connection.NorthEast; }
			else if (PlaceDirection == Vector3.zero) { WireEndB = GetDirectionFromFaceDirection(interaction.Performer); }

			if (WireEndB != Connection.NA)
			{
				if (CableType == WiringColor.high)
				{
					switch (WireEndB)
					{
						case Connection.NorthEast:
							return;
						case Connection.NorthWest:
							return;
						case Connection.SouthWest:
							return;
						case Connection.SouthEast:
							return;
					}

				}
				var econs = interaction.Performer.GetComponentInParent<Matrix>().GetElectricalConnections(localPosInt);
				foreach (var con in econs) {
					if (con.WireEndA == Connection.Overlap || con.WireEndB == Connection.Overlap) {
						if (con.WireEndA == WireEndB || con.WireEndB == WireEndB)
						{
							ChatRelay.Instance.AddToChatLogClient("There is already a cable at that position", ChatChannel.Examine);
							return;
						}
						foreach (var Econ in econs)
						{
							if (Econ.WireEndA == WireEndB || Econ.WireEndB == WireEndB)
							{
								if (con.WireEndA == Econ.WireEndA || con.WireEndB == Econ.WireEndA){
									ChatRelay.Instance.AddToChatLogClient("There is already a cable at that position", ChatChannel.Examine);
									return;
								}
								else if (con.WireEndA == Econ.WireEndB || con.WireEndB == Econ.WireEndB){
									ChatRelay.Instance.AddToChatLogClient("There is already a cable at that position", ChatChannel.Examine);
									return;
								}
							}
						}
					}
				}
				BuildCable(roundTargetWorldPosition, interaction.Performer.transform.parent, WireEndB);
			}
		}
	}

	private void BuildCable(Vector3 position, Transform parent, Connection WireEndB)
	{
		Connection WireEndA = Connection.Overlap;
		GameObject Cable = PoolManager.PoolNetworkInstantiate(CablePrefab, position, parent);
		ElectricalCableMessage.Send(Cable, WireEndA, WireEndB, CableType);
		Cable.GetComponent<CableInheritance>().SetDirection(WireEndB, WireEndA, CableType);
	}
}
