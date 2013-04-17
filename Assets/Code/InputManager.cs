using UnityEngine;
using System.Collections;

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
	
	Color debugDrawColor = Color.white;
	
	public HexCell GetClosestHexCell ()
	{
		return ClosestHexCell;
	}
	
	
	public const float tapThreshold = 20f;
	
	
	HexCellPlaceable draggingObject = null;
	
	// Update is called once per frame
	void Update () 
	{
		debugDrawColor = Input.GetMouseButton(0) ? Color.green : Color.red;
		
//		if (Input.GetMouseButtonDown(0))
//		{
//			Debug.Log("Input.GetMouseButtonDown(0)");
//		}
		
		Ray	inputRay = gameCamera.ScreenPointToRay(Input.mousePosition);
		
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
