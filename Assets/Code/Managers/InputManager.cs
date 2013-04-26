using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : SingletonBehaviour<InputManager>
{
	[SerializeField]
	Camera gameCamera;
	
	
	public HexCell ClosestHexCell { get; private set; }
	public HexCell OverHexCell { get { return OverCell ? ClosestHexCell : null; } }
	
	public HexCell _debug_ClosestHexCell;
	public HexCell _debug_OverHexCell;
	public HexCellPlaceable _debug_OverHexCellMechanism;
	
	public bool OverCell { get; private set; }
	
	List<HexCellPlaceable> selectedPlacables = new List<HexCellPlaceable>();
	
	void SelectUniqueMechanism(HexCellPlaceable mechanism)
	{
		foreach (HexCellPlaceable selectedMechanism in selectedPlacables)
		{
			selectedMechanism.selected = false;
		}
		selectedPlacables.Clear();
		
		
		SelectPlacable(mechanism);
	}
	
	void SelectPlacable(HexCellPlaceable mechanism)
	{
		Debug.Log ("Selecting "+mechanism);
		selectedPlacables.Add(mechanism);
		mechanism.selected = true;
		
		
		if (selectedPlacables.Count == 1)
		{
			if (selectedPlacables[0] is Grabber)
			{
				GrabberProgramUI.instance.DisplayedGrabber = selectedPlacables[0] as Grabber;
			}
			else
			{
				GrabberProgramUI.instance.DisplayedGrabber = null;
			}
		}
		else
		{
			GrabberProgramUI.instance.DisplayedGrabber = null;
		}
	}
	
	void DeselectMechanism(HexCellPlaceable mechanism)
	{
		selectedPlacables.Remove(mechanism);
		mechanism.selected = false;
	}
	
	void ToggleSelectMechanism(HexCellPlaceable mechanism)
	{
		if (selectedPlacables.Contains(mechanism))
		{
			DeselectMechanism(mechanism);
		}
		else
		{
			SelectPlacable(mechanism);
		}
	}
	
	Color debugDrawColor = Color.white;
	
	public HexCell GetClosestHexCell ()
	{
		return ClosestHexCell;
	}
	
	
	public const float tapThreshold = 20f;
	
	
	Mechanism draggingMechanism = null;
	
	IEnumerator Start () 
	{
		yield return null;
		GrabberProgramUI.instance.DisplayedGrabber = null;
	}
	
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	// Up and Down are for mouse released and pressed ( single frame), Released and Pressed are for constant states (multiple frames)
	public enum PressState {Down, Up, Pressed, Released};
	
	public void HandleScreenPoint(Vector3 screenPos, PressState pressState)
	{
		Ray	inputRay = gameCamera.ScreenPointToRay(screenPos);
		
		HandleRay(inputRay, pressState);
	}
	
	public void HandleRay(Ray inputRay, PressState pressState)
	{
		if (pressState != PressState.Released)
		{
			GrabberProgramUI.instance.CloseAllSlots();
		}
			
		debugDrawColor = Input.GetMouseButton(0) ? Color.green : Color.red;
		
//		if (Input.GetMouseButtonDown(0))
//		{
//			Debug.Log("Input.GetMouseButtonDown(0)");
//		}
		
		
//		Debug.DrawRay(inputRay.origin, inputRay.direction*1000f, debugDrawColor, 1f);
		
		RaycastHit hitInfo = new RaycastHit();
		if (Physics.Raycast(inputRay, out hitInfo, 1000, 1 << LayerMask.NameToLayer("Hex Cell")))
		{
			OverCell = true;
			ClosestHexCell = hitInfo.collider.gameObject.GetComponent<HexCell>();
		}
		else
		{
			OverCell = false;
			foreach(HexCell outsideCell in GridManager.instance.GetOutsideCells())
			{
				ClosestHexCell = ClosestHexCell ?? outsideCell;
				
				float distanceToOutsideCell = Vector3.Distance(outsideCell.transform.position, inputRay.origin);
				float closestDistance = Vector3.Distance(ClosestHexCell.transform.position, inputRay.origin);
				
				ClosestHexCell = distanceToOutsideCell < closestDistance ? outsideCell : ClosestHexCell;
			}
		}
		_debug_ClosestHexCell = ClosestHexCell;
		_debug_OverHexCell = OverHexCell;
		_debug_OverHexCellMechanism = OverHexCell == null?null:OverHexCell.placedPlaceable;
		
		
		if (Input.GetMouseButtonDown(0))
		{
			if (OverHexCell != null && OverHexCell.placedPlaceable != null)
			{
				SelectUniqueMechanism(OverHexCell.placedPlaceable);
				
			}
			StartDragging();
		}
		
		if (Input.GetMouseButton(0))
		{
				
		}
		if (Input.GetMouseButtonUp(0))
		{
			StopDragging();
		}
		if (!Input.GetMouseButton(0))
		{
			StopDragging();
		}
		
		Debug.DrawLine(inputRay.origin, ClosestHexCell.transform.position, debugDrawColor, 1f);
//		Debug.Log(closestHexCell.location.x+":"+closestHexCell.location.y);
		
		
	}
	
	
	public void StartDraggingUnplaced(Mechanism mechanism)
	{
		draggingMechanism = mechanism;
		if (draggingMechanism != null)
		{
			draggingMechanism.StartDrag();
		}
	}
	
	void StartDragging()
	{
		if (OverCell)
		{
			draggingMechanism = OverHexCell == null ? null : OverHexCell.placedMechanism;
			if (draggingMechanism != null)
			{
				draggingMechanism.StartDrag();
			}
		}
	}
	
	
	void StopDragging()
	{
		if (draggingMechanism != null)
		{
			draggingMechanism.StopDrag();
			draggingMechanism = null;
		}
	}
	
}
