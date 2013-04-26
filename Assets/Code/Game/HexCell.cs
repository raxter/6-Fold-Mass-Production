using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HexCell : MonoBehaviour 
{
	public UIButton button;
	
//	[System.NonSerialized]
	public HexCellPlaceable placedPlaceable = null;
	public GrabbablePart part = null;
	
	public Mechanism placedMechanism { get { return placedPlaceable as Mechanism; } }
	
	public IntVector2 location;
	
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
