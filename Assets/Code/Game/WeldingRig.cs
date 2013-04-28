using UnityEngine;
using System.Collections;

public class WeldingRig : Mechanism 
{
	#region implemented abstract members of Mechanism
	public override MechanismType MechanismType {
		get 
		{
			return MechanismType.WeldingRig;
		}
	}

	protected override void MechanismUpdate ()
	{
		
	}
	#endregion
	
	HexCell leftBelow = null, centerBelow = null, rightBelow = null;
	
	
	public override void PlaceAtLocation(IntVector2 location)
	{
		
		Location = location;
		if (location != null)
		{
			hexCell     = GridManager.instance.GetHexCell(location);
			leftBelow   = GridManager.instance.GetHexCell(location+HexMetrics.GetRelativeLocation(HexMetrics.Direction.LeftDown));
			centerBelow = GridManager.instance.GetHexCell(location+HexMetrics.GetRelativeLocation(HexMetrics.Direction.Down));
			rightBelow  = GridManager.instance.GetHexCell(location+HexMetrics.GetRelativeLocation(HexMetrics.Direction.RightDown));
			
			if (hexCell != null && leftBelow != null && centerBelow != null && rightBelow != null)
			{
				transform.position = hexCell.transform.position;
				foreach (HexCell hc in new HexCell [4] {hexCell, leftBelow, centerBelow, rightBelow})
				{
					hc.placedPlaceable = this;
				}
			}
		}
		else
		{
			if (hexCell != null && leftBelow != null && centerBelow != null && rightBelow != null)
			{
//				if (hexCell.placedPlaceable == this)
//				{
//					hexCell.placedPlaceable = null;
//				}
				foreach (HexCell hc in new HexCell [4] {hexCell, leftBelow, centerBelow, rightBelow})
				{
					if (hc.placedPlaceable == this)
					{
						hc.placedPlaceable = null;
					}
				}
				hexCell = null;
				leftBelow = null;
				centerBelow = null;
				rightBelow = null;
			}
		}
	}
	
	
	
	
	public void PerformPreStart()
	{
//		foreach (HexCell hc in new HexCell [4] {hexCell, leftBelow, centerBelow, rightBelow})
//		{
//			Debug.Log ("("+hc.location.x+":"+hc.location.y+")"+hc.partOnCell+" -> "+hc.partHeldOverCell +" | "+hc.partOverCell);
//			
//			// there are 5 welding combos (t = top, b = botton, l = left, r = right):
//			// tl, tr, bl, br, tb
//			
//		}
		
		GrabbablePart topPart = hexCell.partOverCell;
		GrabbablePart bottomPart = centerBelow.partOverCell;
		GrabbablePart leftPart = leftBelow.partOverCell;
		GrabbablePart rightPart = rightBelow.partOverCell;
		
		if (topPart != null)
		{
			if (leftPart   != null) topPart.ConnectPart(leftPart);
			if (bottomPart != null) topPart.ConnectPart(bottomPart);
			if (rightPart  != null) topPart.ConnectPart(rightPart);
		}
		if (bottomPart != null)
		{
			if (leftPart  != null) bottomPart.ConnectPart(leftPart);
			if (rightPart != null) bottomPart.ConnectPart(rightPart);
		}
		
//		if (hexCell.partOverCell)
	}
	
	
	
	
	
	
	
	
}














