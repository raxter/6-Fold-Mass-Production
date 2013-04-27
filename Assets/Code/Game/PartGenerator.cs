using UnityEngine;
using System.Collections;

public class PartGenerator : HexCellPlaceable
{
	
	public GrabbablePart toGeneratePrefab;
	
	bool placeOnNextTurn = false;
	
	#region implemented abstract members of HexCellPlaceable
	protected override void PlaceableStart ()
	{
	}

	protected override void PlaceableUpdate ()
	{
	}
	#endregion
	
	public void StartSimulation()
	{
		placeOnNextTurn = true;
	}

	public GrabbablePart StepPreStart ()
	{
//		Debug.Log ("StepPreStart");
		if (hexCell != null)
		{
			GrabbablePart part = hexCell.part;
			
			if (placeOnNextTurn)
			{
				part = (GameObject.Instantiate(toGeneratePrefab.gameObject) as GameObject).GetComponent<GrabbablePart>();
				part.PlaceAtLocation(Location);
				placeOnNextTurn = false;
				return part;
			}
			else if (hexCell.part == null)
			{ 
				placeOnNextTurn = true;
			}
		}
		return null;
	}
}
