using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MechanismType {None, Grabber, WeldingRig, Generator};

public abstract class Mechanism : HexCellPlaceable, IEncodable
{
	
	public abstract MechanismType MechanismType
	{
		get;
	}

	public virtual IEnumerable<IEncodable> Encode ()
	{
		yield return (EncodableInt)Location.x;
		yield return (EncodableInt)Location.y;
	}
	
	public virtual bool Decode (Encoding encoding)
	{
		PlaceAtLocation(new IntVector2(encoding.Int(0), encoding.Int (1)));
		
		return true;
	}
	
	
	bool _dragging = false;
		
	public void StartDrag()
	{
		if (LevelManager.instance.currentSpeed != LevelManager.SimulationSpeed.Stopped)
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
			if (PlaceAtLocation(InputManager.instance.OverHexCell.location))
			{
				InputManager.instance.SelectUniqueMechanism(this);
			}
			else
			{
				ObjectPoolManager.DestroyObject(gameObject);
			}
		}
		else
		{
			PlaceAtLocation(null);
			ObjectPoolManager.DestroyObject(gameObject);
		}
		
		_dragging = false;
		
		CoroutineUtils.WaitOneFrameAndDo(() => GridManager.instance.RegisterMechanismChange());
		
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
	
	
}
