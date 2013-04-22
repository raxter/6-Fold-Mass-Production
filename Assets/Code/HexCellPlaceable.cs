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
	GameObject _selectionItem;
	
	public bool selected
	{
		get
		{
			return _selectionItem.active;
		}
		set
		{
			Debug.Log(gameObject+" selected = "+value);
			_selectionItem.SetActiveRecursively(value);
		}
	}
	
	[SerializeField]
	bool _placeAtStart = false;
	
	[SerializeField]
	IntVector2 debugLocation;
	
	bool _dragging = false;
		
	public void StartDrag()
	{
		if (!GameManager.instance.guiEnabled)
		{
			return;
		}
		Debug.Log ("StartDrag()" + (_hexCell!= null?""+_hexCell.location.x +":"+_hexCell.location.y:""));
		if (_hexCell != null)
		{
			_hexCell.placedMechanism = null;
		}
		_dragging = true;
	}
	public void StopDrag()
	{	
		if (!_dragging)
		{
			return;
		}
		Debug.Log ("StopDrag()" + (_hexCell!= null?""+_hexCell.location.x +":"+_hexCell.location.y:""));
		
		if (InputManager.instance.OverCell && InputManager.instance.OverHexCell.placedMechanism == null)
		{
			PlaceAtLocation(InputManager.instance.OverHexCell.location);
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
			if (_hexCell != null)
			{
				transform.position = _hexCell.transform.position;
				_hexCell.placedMechanism = this;
			}
		}
		else
		{
			if (_hexCell != null)
			{
				if (_hexCell.placedMechanism == this)
				{
					_hexCell.placedMechanism = null;
				}
				_hexCell = null;
			}
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
		Debug.Log ("HexCellPlacable Start");
		selected = false;
		if (_placeAtStart)
		{
			PlaceAtLocation(debugLocation);
		}
		
		PlaceableStart();
	}
	
	protected abstract void PlaceableStart();
	
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
