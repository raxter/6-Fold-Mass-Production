using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// this is really selection and gridinput manager
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
	
	public delegate void OnSelectionChangeDelegate (List<HexCellPlaceable> selectedPlacables);
	
	public event OnSelectionChangeDelegate OnSelectionChange = (selectionList) => {};
	
	List<HexCellPlaceable> selectedPlacables = new List<HexCellPlaceable>();
	
	public void SelectUniqueMechanism(HexCellPlaceable mechanism)
	{
		ClearSelection();
		
		SelectPlacable(mechanism);
	}

	public void ClearSelection ()
	{
		foreach (HexCellPlaceable selectedMechanism in selectedPlacables)
		{
			if (selectedMechanism != null)
			{
				selectedMechanism.selected = false;
			}
		}
		selectedPlacables.Clear();
	}
	
	void SelectPlacable(HexCellPlaceable mechanism)
	{
		Debug.Log ("Selecting "+mechanism);
		selectedPlacables.Add(mechanism);
		mechanism.selected = true;
		
		OnSelectionChange(selectedPlacables);
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
	
	void Start()
	{
		InputCatcher.instance.OnInputEvent += HandleScreenPoint;
	}
	
	public override void OnDestroy()
	{
		if (InputCatcher.hasInstance)
		{
			InputCatcher.instance.OnInputEvent -= HandleScreenPoint;
		}
		base.OnDestroy();
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	
	
	public void HandleScreenPoint(POINTER_INFO pointerInfo, ControlState pressState, ControlState dragState)
	{
		Ray	inputRay = gameCamera.ScreenPointToRay(pointerInfo.devicePos);
		
		HandleRay(inputRay, pressState, dragState);
	}
	
	public void HandleRay(Ray inputRay, ControlState pressState, ControlState dragState)
	{
			
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
