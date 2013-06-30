using UnityEngine;
using System.Collections;

public enum MechanismType {None, Grabber, WeldingRig, Generator};

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
//		Debug.Log ("StartDrag()" + (hexCell!= null?""+hexCell.location.x +":"+hexCell.location.y:""));
		
		PlaceAtLocation(null);
		
		_dragging = true;
	}
	
	public void StopDrag()
	{	
		if (!_dragging)
		{
			return;
		}
//		Debug.Log ("StopDrag()" + (hexCell!= null?""+hexCell.location.x +":"+hexCell.location.y:""));
		
		
		if (InputManager.instance.OverCell && InputManager.instance.OverHexCell.placedPlaceable == null)
		{
			PlaceAtLocation(InputManager.instance.OverHexCell.location);
//			InputManager.instance.SelectUniqueMechanism(this);
		}
		else
		{
			PlaceAtLocation(null);
			ObjectPoolManager.DestroyObject(gameObject);
		}
		
		_dragging = false;
		
		CoroutineUtils.instance.WaitOneFrameAndDo(() => GridManager.instance.SaveLayout());
	}
	
//	IEnumerator WaitOneFrameAndSaveLayout()
//	{
//		yield return null;
//		
//		Debug.Log("Saving Layout");
//		GridManager.instance.SaveLayout();
//	}
	
	
	protected override void PlaceableStart()
	{
		MechanismStart();
	}
	
	protected override void PlaceableUpdate()
	{
		if (_dragging)
		{
			PlaceOverLocation(InputManager.instance.ClosestHexCell.location);
		}
		
		MechanismUpdate();
		
	}
	
	protected abstract void MechanismUpdate();
	protected abstract void MechanismStart();
	
	public abstract string Encode();
	
	public abstract bool Decode(string encoded);
	
	
}
