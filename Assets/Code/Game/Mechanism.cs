using UnityEngine;
using System.Collections;

public enum MechanismType {None, Grabber, WeldingRig};

public abstract class Mechanism : HexCellPlaceable 
{
	
	public abstract MechanismType MechanismType
	{
		get;
	}
	
	
	bool _dragging = false;
		
	public void StartDrag()
	{
		if (GameManager.instance.currentSpeed != GameManager.SimulationSpeed.Stopped)
		{
			return;
		}
		Debug.Log ("StartDrag()" + (hexCell!= null?""+hexCell.location.x +":"+hexCell.location.y:""));
		if (hexCell != null)
		{
			hexCell.placedPlaceable = null;
		}
		_dragging = true;
	}
	public void StopDrag()
	{	
		if (!_dragging)
		{
			return;
		}
		Debug.Log ("StopDrag()" + (hexCell!= null?""+hexCell.location.x +":"+hexCell.location.y:""));
		
		if (InputManager.instance.OverCell && InputManager.instance.OverHexCell.placedPlaceable == null)
		{
			PlaceAtLocation(InputManager.instance.OverHexCell.location);
			InputManager.instance.SelectUniqueMechanism(this);
		}
		else
		{
			PlaceAtLocation(null);
			GameObject.Destroy(gameObject);
		}
		
		_dragging = false;
	}
	
	
	
	
	protected override void PlaceableStart()
	{
	}
	
	protected override void PlaceableUpdate()
	{
		if (_dragging)
		{
			PlaceOverLocation(InputManager.instance.ClosestHexCell.location);
		}
	}
	
	protected abstract void MechanismUpdate();
	
}
