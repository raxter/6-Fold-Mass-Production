using UnityEngine;
using System.Collections;

public class PartGenerator : HexCellPlaceable
{
	
	public Construction toGenerateConstruction;
	
	bool placeOnNextTurn = false;
	
	static int generatorCount = 0;
	static int constructionCount = 0;
	
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

	public Construction StepPreStart ()
	{
//		Debug.Log ("StepPreStart");
		if (hexCell != null)
		{
			if (placeOnNextTurn)
			{
				Construction construction;
				construction = (GameObject.Instantiate(toGenerateConstruction.gameObject) as GameObject).GetComponent<Construction>();
				construction.idNumber = generatorCount;
				construction.gameObject.name = toGenerateConstruction.gameObject.name+" "+generatorCount;
//				part.PlaceAtLocation(Location);
				
				construction.transform.position = GridManager.instance.GetHexCell(Location).transform.position;
//				construction.AddToConstruction(construction);
//				construction.transform.localPosition = Vector3.zero;
				
				placeOnNextTurn = false;
				generatorCount += 1;
				return construction;
			}
			else if (hexCell.partOverCell == null)
			{ 
				placeOnNextTurn = true;
			}
		}
		return null;
	}
}



