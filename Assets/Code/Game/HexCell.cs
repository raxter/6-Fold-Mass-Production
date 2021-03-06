using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HexCell : MonoBehaviour 
{
	public UIButton button;
	
	[SerializeField]
	Color finishCellColor;
	
	public SpriteText debugText;
	
	public void SetDebugText()
	{
		debugText.gameObject.SetActive(GameSettings.instance.debugOutput);
			
		debugText.Text = ""+(partOverCell == null ? "_" : ""+partOverCell.idNumber)
			+ "\n" + (placedMechanism == null? "_":""+(int)placedMechanism.MechanismType);
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
//			Debug.Log ("Placing "+value+" at "+location.x+":"+location.y);
			_placedPlaceable = value;
			SetDebugText();
		}
	}

	public void RegisterPart (GrabbablePart newPart)
	{
		_partOverCell = newPart;
	}

	public void DeregisterPart ()
	{
		if (_partOverCell != null)
		{
//			Debug.Log ("DeregisterPart "+location.x+":"+location.y+" "+_partOverCell.name);
		}
		_partOverCell = null;
	}
	
	GrabbablePart _partOverCell;
	public GrabbablePart partOverCell
	{
		get
		{
			return _partOverCell;
		}
	}
	
	
	public Mechanism placedMechanism 
	{
		get
		{
			return placedPlaceable as Mechanism; 
		}
		set
		{
			placedPlaceable = value;
		}
	}
	
	public IntVector2 location;
	
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
//		switch (ptr.evt)
//		{
//			case POINTER_INFO.INPUT_EVENT.PRESS:
//			{
//				if (placedPlaceable is Mechanism)
//				{
////					Debug.Log("StartDrag()");
//					placedMechanism.StartDrag();
//				}
//				break;
//			}
//			case POINTER_INFO.INPUT_EVENT.TAP:
//			case POINTER_INFO.INPUT_EVENT.RELEASE:
//			case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
//				if (placedPlaceable is Mechanism)
//				{
////					Debug.Log("StopDrag()");
//					placedMechanism.StopDrag();
//				}
//				break;
//			
//		}
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
