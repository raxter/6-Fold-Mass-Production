using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HexCell : MonoBehaviour 
{
	public UIButton button;
	
	[SerializeField]
	Color finishCellColor;
	
	public SpriteText debugText;
	
	void SetDebugText()
	{
		debugText.Text = ""+(_partOnCell == null ? "_" : ""+_partOnCell.idNumber) +">"+(_partHeldOverCell == null ? "_" : ""+_partHeldOverCell.idNumber)
			+ "\n" + (placedMechanism == null? " ":""+(int)placedMechanism.MechanismType);
	}
	
//	[System.NonSerialized]
	HexCellPlaceable _placedPlaceable = null;
	public HexCellPlaceable placedPlaceable 
	{
		get 
		{ 
			return _placedPlaceable; 
		}
		set
		{
			Debug.Log ("Placing "+value+" at "+location.x+":"+location.y);
			_placedPlaceable = value;
			SetDebugText();
		}
	}
	
	public GrabbablePart partOverCell
	{
		get { return _partOnCell ?? partHeldOverCell; }
	}
	
	public GrabbablePart _partHeldOverCell;
	public GrabbablePart partHeldOverCell
	{
		get { return _partHeldOverCell; }
		set 
		{
			// Must deregister overLocation with Part
			if (_partHeldOverCell != null)
			{
				partOverCell.heldOverLocation = null;
			}
			_partHeldOverCell = value;
			if (_partHeldOverCell != null)
			{
				partOverCell.heldOverLocation = location;
			}
			SetDebugText();
		}
	}
	
//	GrabbablePart _part;
	GrabbablePart _partOnCell;
	public GrabbablePart partOnCell
	{
		get {return _partOnCell;}
		set 
		{
			_partOnCell = value;
			if (_partOnCell != null)
			{
				button.SetColor(Color.red);
			}
			else
			{
				finishCell = _finishCell;
			}
			SetDebugText();
		}
	}
	
	public Mechanism placedMechanism { get { return placedPlaceable as Mechanism; } }
	
	public IntVector2 location;
	
	[HideInInspector]
	[SerializeField]
	bool _finishCell;
	public bool finishCell
	{
		get { return _finishCell; }
		set 
		{
			_finishCell = value;
			button.SetColor(_finishCell ? finishCellColor : Color.white);
		}
	}
	
	public float SideLength
	{
		get { return button.width/2f; }	
	}
	
	public Vector2 RightUp
	{
		get { return new Vector2(button.width-(SideLength/2f),  button.height/2f); }
	}
	
	public Vector2 RightDown
	{
		get { return new Vector2(button.width-(SideLength/2f), -button.height/2f); }
	}
	
	public Vector2 Up
	{
		get { return new Vector2(0,button.height); }
	}
	
	public float Height
	{
		get { return button.height; }
	}
	
	void Start()
	{
		SetDebugText();
		button.AddInputDelegate(GrabberInputDelegate);
	}
	
	public void GrabberInputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
			case POINTER_INFO.INPUT_EVENT.PRESS:
			{
				if (placedPlaceable is Mechanism)
				{
//					Debug.Log("StartDrag()");
					placedMechanism.StartDrag();
				}
				break;
			}
			case POINTER_INFO.INPUT_EVENT.TAP:
			case POINTER_INFO.INPUT_EVENT.RELEASE:
			case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
				if (placedPlaceable is Mechanism)
				{
//					Debug.Log("StopDrag()");
					placedMechanism.StopDrag();
				}
				break;
			
		}
	}
	
	public Vector2 GetDirection (HexMetrics.Direction direction)
	{
		return new Dictionary<HexMetrics.Direction, System.Func<Vector2>>()
		{
			{HexMetrics.Direction.Up, 		() =>  Up},
			{HexMetrics.Direction.RightUp, 	() =>  RightUp},
			{HexMetrics.Direction.RightDown,() =>  RightDown},
			{HexMetrics.Direction.Down, 	() => -Up},
			{HexMetrics.Direction.LeftDown,	() => -RightUp},
			{HexMetrics.Direction.LeftUp,	() => -RightDown}
			
		}[direction]();
	}
	
}
