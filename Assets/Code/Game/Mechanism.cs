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
		
		PlaceAtLocation(null);
		
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
		
		StartCoroutine(WaitOneFrameAndSaveLayout());
	}
	
	IEnumerator WaitOneFrameAndSaveLayout()
	{
		yield return null;
		
		Debug.Log("Saving Layout");
		GridManager.instance.SaveLayout();
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
	
	public abstract string Encode();
	
	public abstract bool Decode(string encoded);
	
	public static int CodeToNumber(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c-'0';
		}
		if (c >= 'a' && c <= 'z')
		{
			return c-'a';
		}
		
		return -1;
	}
	
	public static char NumberToCode(int i)
	{
		if (i <= 9)
		{
			return (char)('0'+i);
		}
		
		if (i > 9)
		{
			return (char)('a'+(i-10));
		}
		
		return '!';
	}
	
}
