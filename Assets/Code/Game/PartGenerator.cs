using UnityEngine;
using System.Collections;

public class PartGenerator : HexCellPlaceable
{
	
	public GrabbablePart toGeneratePrefab;
	
	bool placeOnNextTurn = false;
	
	static int generatorCount = 0;
	
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
			if (placeOnNextTurn)
			{
				GrabbablePart part;
				part = (GameObject.Instantiate(toGeneratePrefab.gameObject) as GameObject).GetComponent<GrabbablePart>();
				part.idNumber = generatorCount;
				part.gameObject.name = toGeneratePrefab.gameObject.name+" "+generatorCount;
//				part.PlaceAtLocation(Location);
				part.transform.position = GridManager.instance.GetHexCell(Location).transform.position;
				placeOnNextTurn = false;
				generatorCount += 1;
				return part;
			}
			else if (hexCell.partOverCell == null)
			{ 
				placeOnNextTurn = true;
			}
		}
		return null;
	}
}
