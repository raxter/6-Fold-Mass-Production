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
		foreach (HexCell hc in new HexCell [4] {hexCell, leftBelow, centerBelow, rightBelow})
		{
			Debug.Log ("("+hc.location.x+":"+hc.location.y+")"+hc.partOnCell+" -> "+hc.partHeldOverCell +" | "+hc.partOverCell);
		}
	}
	
	
	
	
	
	
	
	
}














