using UnityEngine;
using System.Collections;

public class PartGenerator : HexCellPlaceable
{
	
	public GrabbablePart toGeneratePrefab;
	
	#region implemented abstract members of HexCellPlaceable
	protected override void PlaceableStart ()
	{
	}

	protected override void PlaceableUpdate ()
	{
	}
	#endregion

	public void StepPreStart ()
	{
		Debug.Log ("StepFinished");
		if (hexCell != null)
		{
			GrabbablePart part = hexCell.part;
			
			if (part == null)
			{
				part = (GameObject.Instantiate(toGeneratePrefab.gameObject) as GameObject).GetComponent<GrabbablePart>();
				part.PlaceAtLocation(Location);
			}
		}
	}
}
