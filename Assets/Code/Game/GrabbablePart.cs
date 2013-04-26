using UnityEngine;
using System.Collections;

public class GrabbablePart : HexCellPlaceable 
{
	#region implemented abstract members of HexCellPlaceable
	protected override void PlaceableStart ()
	{
	}

	protected override void PlaceableUpdate ()
	{
	}
	#endregion
	
	public override void PlaceAtLocation(IntVector2 location)
	{
		Location = location;
		if (location != null)
		{
			hexCell = GridManager.instance.GetHexCell(location);
			if (hexCell != null)
			{
				transform.position = hexCell.transform.position;
				hexCell.part = this;
			}
		}
		else
		{
			if (hexCell != null)
			{
				if (hexCell.part == this)
				{
					hexCell.part = null;
				}
				hexCell = null;
			}
		}
	}
}
