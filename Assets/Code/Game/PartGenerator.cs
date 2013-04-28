using UnityEngine;
using System.Collections;

public class PartGenerator : HexCellPlaceable
{
	
	public GrabbablePart toGeneratePrefab;
	
	bool placeOnNextTurn = false;
	
	int generatorCount = 0;
	
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
		generatorCount = 0;
	}

	public GrabbablePart StepPreStart ()
	{
//		Debug.Log ("StepPreStart");
		if (hexCell != null)
		{
			GrabbablePart part = hexCell.partOnCell;
			
			if (placeOnNextTurn)
			{
				part = (GameObject.Instantiate(toGeneratePrefab.gameObject) as GameObject).GetComponent<GrabbablePart>();
				part.gameObject.name = toGeneratePrefab.gameObject.name+" "+generatorCount;
				part.PlaceAtLocation(Location);
				placeOnNextTurn = false;
				generatorCount += 1;
				return part;
			}
			else if (hexCell.partOnCell == null)
			{ 
				placeOnNextTurn = true;
			}
		}
		return null;
	}
}
