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

	protected override void MechanismStart ()
	{
		
	}
	protected override void MechanismUpdate ()
	{
		
	}
	#endregion
	
	HexCell leftBelow = null, centerBelow = null, rightBelow = null;
	
	public override string Encode()
	{
		return "";
	}
	
	public override bool Decode(string encoded)
	{
		return true;
	}
	
	public override void PlaceAtLocation(IntVector2 location)
	{
		
		Location = location;
		if (location != null)
		{
			hexCell     = GridManager.instance.GetHexCell(location);
			leftBelow   = GridManager.instance.GetHexCell(location+HexMetrics.GetGridOffset(HexMetrics.Direction.LeftDown));
			centerBelow = GridManager.instance.GetHexCell(location+HexMetrics.GetGridOffset(HexMetrics.Direction.Down));
			rightBelow  = GridManager.instance.GetHexCell(location+HexMetrics.GetGridOffset(HexMetrics.Direction.RightDown));
			
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
	
	
	
	
	public void PerformPostStart()
	{
//		foreach (HexCell hc in new HexCell [4] {hexCell, leftBelow, centerBelow, rightBelow})
//		{
//			Debug.Log ("("+hc.location.x+":"+hc.location.y+")"+hc.partOnCell+" -> "+hc.partHeldOverCell +" | "+hc.partOverCell);
//			
//			// there are 5 welding combos (t = top, b = botton, l = left, r = right):
//			// tl, tr, bl, br, tb
//			
//		}
		
		System.Action<Construction> deleteFunction = (obj) => Destroy(obj.gameObject);
		//InstantiatePrefabDelegate instantiateFunction = (prefab) => Instantiate(prefab) as GameObject;
		
		GrabbablePart topPart = hexCell.partOverCell;
		GrabbablePart bottomPart = centerBelow.partOverCell;
		GrabbablePart leftPart = leftBelow.partOverCell;
		GrabbablePart rightPart = rightBelow.partOverCell;
		
		if (topPart != null)
		{
			if (leftPart   != null) topPart.ConnectPartOnGrid(leftPart, GrabbablePart.PhysicalConnectionType.Weld, deleteFunction);
			if (bottomPart != null) topPart.ConnectPartOnGrid(bottomPart, GrabbablePart.PhysicalConnectionType.Weld, deleteFunction);
			if (rightPart  != null) topPart.ConnectPartOnGrid(rightPart, GrabbablePart.PhysicalConnectionType.Weld, deleteFunction);
		}
		if (bottomPart != null)
		{
			if (leftPart  != null) bottomPart.ConnectPartOnGrid(leftPart, GrabbablePart.PhysicalConnectionType.Weld, deleteFunction);
			if (rightPart != null) bottomPart.ConnectPartOnGrid(rightPart, GrabbablePart.PhysicalConnectionType.Weld, deleteFunction);
		}
		
//		if (hexCell.partOverCell)
	}
	
	
	
	
	
	
	
	
}














