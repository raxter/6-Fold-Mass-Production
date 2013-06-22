using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionMaker : SingletonBehaviour<ConstructionMaker> 
{
	List<Construction> targetConstructions = null;
	
	[SerializeField]
	UIButton _draggableButtonPrefab = null;
	
//	List<UIButton> _dragButtons = new List<UIButton>();
	
//	[SerializeField]
//	Camera _makerCamera = null;
	
	[SerializeField]
	GameObject _partHolder = null;
	[SerializeField]
	GameObject _constructionHolder = null;

	public bool Open
	{
		get 
		{
			return this.gameObject.activeSelf;
		}
	}
	
	IEnumerator Start()
	{
		yield return null;
		
		int counter = 0;
		
		List<Construction> draggableParts = new List<Construction>();
		
		foreach (PartType partType in GameSettings.instance.partPrefabs.ConvertAll<PartType>((input) => input.partType))
//		foreach (GrabbablePart prefab in GameSettings.instance.partPrefabs)
		{
			Construction partConstruction = Construction.CreateSimpleConstruction(partType, (prefab) => Instantiate(prefab) as GameObject);
			partConstruction.ignoreCollisions = true;
			
			GameObject dragButton = Instantiate(_draggableButtonPrefab.gameObject) as GameObject;
			UIButton dragUIButton = dragButton.GetComponent<UIButton>();
			SetUpDragDropBehaviour(dragUIButton, partConstruction.CenterPart);
			dragButton.SetActive(true);
//			_dragButtons.Add(dragButton.GetComponent<UIButton>());
			
			dragButton.transform.parent = _partHolder.transform;
			dragButton.transform.localPosition = new Vector3 (0, -60f * counter, dragUIButton.dragOffset);
			
			partConstruction.transform.parent = dragButton.transform;
			partConstruction.transform.localPosition = Vector3.zero;
			
			draggableParts.Add(partConstruction);
			
//			GameObject partObject = Instantiate(prefab.gameObject) as GameObject;
//			partObject.transform.parent = _partHolder.transform;
//			partObject.transform.position = Vector3.zero;
//			partObject.transform.localPosition = new Vector3 (0, -60f * counter, 0);
			
			counter += 1;
		}
		
		
		yield return null;
		foreach (Construction construction in draggableParts)
		{
			construction.CenterPart.PartSphereCollider.gameObject.SetActive(false);
		}
		
		CloseMaker ();
		
	}
	
	void SetUpDragDropBehaviour(UIButton button, GrabbablePart part)
	{
		Debug.Log ("Setting up Drag Drop behaviour");
//		Debug.DebugBreak();
		button.SetDragDropDelegate((parms) => 
		{
			if (parms.evt == EZDragDropEvent.Begin)
			{
				Debug.Log ("Begin "+parms.dragObj/*+":"+parms.dragObj.transform.position+":"+parms.ptr.ray*/);
					
			}
			if (parms.evt == EZDragDropEvent.Update)
			{
				List<Construction.PartSide> partSides = new List<Construction.PartSide>();
				targetConstructions.ForEach((con) => partSides.AddRange(con.IsConnectable(part)));
				foreach(Construction.PartSide partSide in partSides)
				{
					Color drawColor = new Color [] {Color.green, Color.yellow, Color.blue, Color.red}[Mathf.Abs(partSide.offsetFromSide)];
					Vector3 offset = GameSettings.instance.hexCellPrefab.GetDirection(partSide.part.Absolute(partSide.relativeDirection));
					Debug.DrawLine(partSide.part.transform.position, partSide.part.transform.position + offset, drawColor);
					
//					Debug.Log ("is connectable "+partSide.part.name +" in "+partSide.relativeDirection+" ("+partSide.offsetFromSide+")");
					
				}
				
				partSides.Sort((x, y) => Mathf.Abs(x.offsetFromSide) - Mathf.Abs(y.offsetFromSide));
				if (partSides.Count > 0)
				{
					Debug.Log ("rotating towards "+partSides[0].part.name+" ("+partSides[0].offsetFromSide+")");
					part.SimulationOrientation = (HexMetrics.Direction)(((int)part.SimulationOrientation-partSides[0].offsetFromSide+6)%6);
				}
//				Debug.Log (string.Join(",", partSides.ConvertAll((input) => ""+input.offsetFromSide).ToArray()));
			}
			if (parms.evt == EZDragDropEvent.Cancelled)
			{
				Debug.Log ("Cancelled "+parms.dragObj);
				Debug.Log ("DropTarget: "+parms.dragObj.DropTarget);
				
				
				
				List<Construction.PartSide> partSides = new List<Construction.PartSide>();
				targetConstructions.ForEach((con) => partSides.AddRange(con.IsConnectable(part)));
				partSides.Sort((x, y) => Mathf.Abs(x.offsetFromSide) - Mathf.Abs(y.offsetFromSide));
				if (partSides.Count > 0)
				{
					GameObject partObject = Instantiate(GameSettings.instance.GetPartPrefab(part.partType).gameObject) as GameObject;
					GrabbablePart newPart = partObject.GetComponent<GrabbablePart>();
					newPart.SimulationOrientation = part.SimulationOrientation;
					partSides[0].part.ConnectPartAndPlaceAtRelativeDirection(
						newPart, 
						GrabbablePart.PhysicalConnectionType.Weld, 
						partSides[0].relativeDirection,
						(con) => 
						{
							targetConstructions.Remove(con);
							Destroy(con.gameObject);
						});
				}
				
				part.SimulationOrientation = HexMetrics.Direction.Up;
				
//				targetConstructions.
				
			}
			if (parms.evt == EZDragDropEvent.CancelDone)
			{
				Debug.Log ("CancelDone "+parms.dragObj);
			}
		});
	}
	
	

	public void OpenMaker (string code)
	{
//		this.transform.localScale = Vector3.one;
		this.gameObject.SetActive(true);
		
//		_dragButtons.ForEach((obj) => obj.SetCamera(_makerCamera));
		
		InputCatcher.instance.RequestInputOverride(HandleScreenPoint);
		
		targetConstructions = new List<Construction>();
		targetConstructions.Add(Construction.Decode(code, (prefab) => Instantiate(prefab) as GameObject));
		targetConstructions[0].transform.parent = _constructionHolder.transform;
		targetConstructions[0].transform.localPosition = Vector3.zero;
//		construction.gameObject.SetLayerRecursively(gameObject.layer);
	}
	
	public void CloseMaker ()
	{
//		this.transform.localScale = Vector3.zero;
		this.gameObject.SetActive(false);
		
		InputCatcher.instance.ReleaseInputOverride(HandleScreenPoint);
		
		if (targetConstructions != null)
		{
			targetConstructions.ForEach((con) => Destroy(con.gameObject));
			targetConstructions = null;
		}
	}
	
	public void HandleScreenPoint(Vector3 screenPos, PressState pressState)
	{
		
		Ray	inputRay = UIManager.instance.rayCamera.ScreenPointToRay(screenPos);
		
		HandleRay(inputRay, pressState);
	}
	
	public void HandleRay(Ray inputRay, PressState pressState)
	{
//		Debug.Log(inputRay+":"+pressState);
		
		Debug.DrawRay(inputRay.origin, inputRay.direction*100, Color.red);
		
		if (pressState == PressState.Up)
		{
			foreach (RaycastHit hitInfo in Physics.RaycastAll(inputRay, 1000, 1 << LayerMask.NameToLayer("GrabbablePart")))
			{
	//			Debug.Log ("hit somthing "+hitInfo.collider.gameObject.name);
				Transform hitParent = hitInfo.collider.transform.parent;
				
				if (hitParent == null) continue;
				
				GrabbablePart hitPart = hitParent.gameObject.GetComponent<GrabbablePart>();
				
				if (hitPart == null) continue;
	//			Debug.Log ("hit a part");
				
				if (targetConstructions.Contains(hitPart.ParentConstruction))
				{
					Debug.Log ("hit a part in our preview construction");
					
					foreach(Construction construction in hitPart.ParentConstruction.RemoveFromConstruction(hitPart))
					{
						if (!targetConstructions.Contains(construction))
						{
							targetConstructions.Add(construction);
						}
					}
					targetConstructions.Remove(hitPart.ParentConstruction);
					Destroy (hitPart.ParentConstruction.gameObject);
				}
			}
		}
		
	}
}










