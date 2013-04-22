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
	
	List<HexCellPlaceable> selectedMechanisms = new List<HexCellPlaceable>();
	
	void SelectUniqueMechanism(HexCellPlaceable mechanism)
	{
		foreach (HexCellPlaceable selectedMechanism in selectedMechanisms)
		{
			selectedMechanism.selected = false;
		}
		selectedMechanisms.Clear();
		
		
		SelectMechanism(mechanism);
	}
	
	void SelectMechanism(HexCellPlaceable mechanism)
	{
		Debug.Log ("Selecting "+mechanism);
		selectedMechanisms.Add(mechanism);
		mechanism.selected = true;
		
		
		if (selectedMechanisms.Count == 1)
		{
			if (selectedMechanisms[0] is Grabber)
			{
				GrabberProgramUI.instance.DisplayedGrabber = selectedMechanisms[0] as Grabber;
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
		selectedMechanisms.Remove(mechanism);
		mechanism.selected = false;
	}
	
	void ToggleSelectMechanism(HexCellPlaceable mechanism)
	{
		if (selectedMechanisms.Contains(mechanism))
		{
			DeselectMechanism(mechanism);
		}
		else
		{
			SelectMechanism(mechanism);
		}
	}
	
	Color debugDrawColor = Color.white;
	
	public HexCell GetClosestHexCell ()
	{
		return ClosestHexCell;
	}
	
	
	public const float tapThreshold = 20f;
	
	
	HexCellPlaceable draggingObject = null;
	
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
		_debug_OverHexCellMechanism = OverHexCell == null?null:OverHexCell.placedMechanism;
		
		
		if (Input.GetMouseButtonDown(0))
		{
			if (OverHexCell != null && OverHexCell.placedMechanism != null)
			{
				SelectUniqueMechanism(OverHexCell.placedMechanism);
				
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
	
	
	public void StartDraggingUnplaced(HexCellPlaceable hexCellPlacable)
	{
		draggingObject = hexCellPlacable;
		if (draggingObject != null)
		{
			draggingObject.StartDrag();
		}
	}
	
	void StartDragging()
	{
		if (OverCell)
		{
			draggingObject = OverHexCell == null ? null : OverHexCell.placedMechanism;
			if (draggingObject != null)
			{
				draggingObject.StartDrag();
			}
		}
	}
	
	
	void StopDragging()
	{
		if (draggingObject != null)
		{
			draggingObject.StopDrag();
			draggingObject = null;
		}
	}
	
}
