using UnityEngine;
using System.Collections;

public enum HexCellPlaceableType {None, Grabber};

public abstract class HexCellPlaceable : MonoBehaviour
{
	public IntVector2 Location
	{
		get; set;
	}
	
	public abstract HexCellPlaceableType MechanismType
	{
		get;
	}
	
	
	[SerializeField]
	bool _placeAtStart = false;
	
	[SerializeField]
	IntVector2 debugLocation;
	
	bool _dragging = false;
		
	public void StartDrag()
	{
		Debug.Log ("StartDrag()");
		if (_hexCell != null)
		{
			_hexCell.placedMechanism = null;
		}
		_dragging = true;
	}
	public void StopDrag()
	{	
		Debug.Log ("StopDrag()");
		if (InputManager.instance.OverCell)
		{
			PlaceAtLocation(InputManager.instance.ClosestHexCell.location);
		}
		else
		{
			PlaceAtLocation(null);
			GameObject.Destroy(gameObject);
		}
		
		_dragging = false;
	}
	
	void PlaceAtLocation(IntVector2 location)
	{
		Location = location;
		if (location != null)
		{
			_hexCell = GridManager.instance.GetHexCell(location);
			if (_hexCell)
			{
				transform.position = _hexCell.transform.position;
				_hexCell.placedMechanism = this;
			}
		}
		else
		{
			_hexCell.placedMechanism = null;
			_hexCell = null;
		}
	}
	
	void PlaceOverLocation(IntVector2 location)
	{
		if (location != null)
		{
			_hexCell = GridManager.instance.GetHexCell(location);
			if (_hexCell)
			{
				transform.position = _hexCell.transform.position;
			}
		}
	}
	
	protected HexCell _hexCell;
	
	void Start()
	{
		if (_placeAtStart)
		{
			PlaceAtLocation(debugLocation);
		}
	}
	
	void Update()
	{
		if (_dragging)
		{
			PlaceOverLocation(InputManager.instance.ClosestHexCell.location);
		}
		
		PlaceableUpdate();
	}
	
	protected abstract void PlaceableUpdate();
	

}
