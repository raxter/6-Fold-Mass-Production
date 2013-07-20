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
	
	
	public bool isSolutionMechanism = true;		
	public virtual IEnumerable<IEncodable> Encode ()
	{
		yield return (EncodableInt)Location.x;
		yield return (EncodableInt)Location.y;
		yield return (EncodableInt)(isSolutionMechanism ? 1 : 0);
	}
	
	public virtual bool Decode (Encoding encoding)
	{
		IntVector2 newLocation = new IntVector2(encoding.Int(0), encoding.Int (1));
		Mechanism placedMechanism = GridManager.instance.GetHexCell(newLocation).placedMechanism;
		
		bool didReplacedPart = false;
		if (placedMechanism != null) // we are replacing a part
		{
			if (placedMechanism.MechanismType == MechanismType && // we are replacing the same type of part
				placedMechanism.Location.IsEqualTo(newLocation) && // the location of the old part is the same as this new one (important for multicell mechanisms e.g. weldingRig)
				!placedMechanism.isSolutionMechanism) // is a level mechanism (not part of the solution, part of the problem ;p)
			{
				ObjectPoolManager.DestroyObject(placedMechanism);
				PlaceAtLocation(newLocation);
				isSolutionMechanism = false; // we use the already on board's movable (i.e. immovable)
				
			}
			else
			{
				// something went wrong, we are loading a mechanism on top of one that is different, or a solution mechanism
				Debug.LogError("Something went wrong, we are loading a mechanism on top of one that is different, or a solution mechanism");
				return false;
			}
		}
		else // this is a new part
		{
			PlaceAtLocation(newLocation);
			isSolutionMechanism = (int)encoding.Int(2) == 1;
		}
		
		return true;
	}
	
	
	bool _dragging = false;
		
	public void StartDrag()
	{
		if (LevelManager.instance.currentSpeed != LevelManager.SimulationSpeed.Stopped || (!LevelEditorGUI.hasActiveInstance && !isSolutionMechanism))
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
		
		isSolutionMechanism = !LevelEditorGUI.hasActiveInstance;
		
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
