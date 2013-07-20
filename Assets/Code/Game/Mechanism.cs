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
	
	
	public bool movable = true;		
	public virtual IEnumerable<IEncodable> Encode ()
	{
		yield return (EncodableInt)Location.x;
		yield return (EncodableInt)Location.y;
		yield return (EncodableInt)(movable ? 1 : 0);
	}
	
	public virtual bool Decode (Encoding encoding)
	{
		IntVector2 newLocation = new IntVector2(encoding.Int(0), encoding.Int (1));
		Mechanism placedMechanism = GridManager.instance.GetHexCell(newLocation).placedMechanism;
		
		bool didReplacedPart = false;
		if (placedMechanism != null)
		{
			bool oldMovable = placedMechanism.movable;
			ObjectPoolManager.DestroyObject(placedMechanism);
			PlaceAtLocation(newLocation);
			bool savedMovable = (int)encoding.Int(2) == 1; // do something with this
			movable = oldMovable;
		}
		else
		{
			PlaceAtLocation(newLocation);
			movable = (int)encoding.Int(2) == 1;
		}
		
		return true;
	}
	
	
	bool _dragging = false;
		
	public void StartDrag()
	{
		if (LevelManager.instance.currentSpeed != LevelManager.SimulationSpeed.Stopped || (!LevelEditorGUI.hasActiveInstance && !movable))
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
		
		movable = !LevelEditorGUI.hasActiveInstance;
		
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
