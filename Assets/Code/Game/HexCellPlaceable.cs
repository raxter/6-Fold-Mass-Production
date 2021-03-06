using UnityEngine;
using System.Collections;


public abstract class HexCellPlaceable : MonoBehaviour
{
	public IntVector2 Location
	{
		get; protected set;
	}
	
	[SerializeField]
	GameObject _selectionItem;
	
	public bool selected
	{
		get
		{
			return _selectionItem.activeSelf;
		}
		set
		{
//			Debug.Log(gameObject+" selected = "+value);
			_selectionItem.SetActive(value);
		}
	}
	
	[SerializeField]
	bool _debug_placeAtStart = false;
	
	[SerializeField]
	IntVector2 debugLocation;
	
	
	
	protected HexCell hexCell;
	
	
	
	public virtual bool PlaceAtLocation(IntVector2 location)
	{
		Location = location;
		if (location != null)
		{
			hexCell = GridManager.instance.GetHexCell(location);
			if (hexCell != null)
			{
				transform.position = hexCell.transform.position;
				hexCell.placedPlaceable = this;
				return true;
			}
		}
		else
		{
			if (hexCell != null)
			{
				if (hexCell.placedPlaceable == this)
				{
					hexCell.placedPlaceable = null;
				}
				hexCell = null;
			}
		}
		return false;
	}
	
	protected void PlaceOverLocation(IntVector2 location)
	{
		if (location != null)
		{
			hexCell = GridManager.instance.GetHexCell(location);
			if (hexCell)
			{
				transform.position = hexCell.transform.position;
			}
		}
	}
	
	void Start()
	{
//		Debug.Log ("HexCellPlacable Start");
		selected = false;
		if (_debug_placeAtStart)
		{
			PlaceAtLocation(debugLocation);
		}
		
		PlaceableStart();
	}
	
	protected abstract void PlaceableStart();
	
	void Update()
	{
		PlaceableUpdate();
	}
	
	protected abstract void PlaceableUpdate();


}
