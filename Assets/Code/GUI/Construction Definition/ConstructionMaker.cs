using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public delegate void ConstructionSavedDelegate(string encoded);

public class ConstructionMaker : SingletonBehaviour<ConstructionMaker> 
{
	List<Construction> targetConstructions = null;
	
	[SerializeField]
	UIButton _draggableButtonPrefab = null;
	
	[SerializeField]
	SpriteBase _innerArrowPrefab = null;
	
	SpriteBase [] _innerArrows = new SpriteBase [6];
	
	Dictionary<PartType, UIButton> dragButtons = new Dictionary<PartType, UIButton>();
	Dictionary<UIButton, GrabbablePart> buttonToPart = new Dictionary<UIButton, GrabbablePart>();
	
//	List<UIButton> _dragButtons = new List<UIButton>();
	
//	[SerializeField]
//	Camera _makerCamera = null;
	
	[SerializeField]
	GameObject _partHolder = null;
	[SerializeField]
	GameObject _constructionHolder = null;
	
	
	GrabbablePart partMarkedToDestroy = null;

	public bool Open
	{
		get 
		{
			return this.gameObject.activeSelf;
		}
	}
	
	public override void Awake()
	{
		base.Awake();
		for (int i = 0 ; i < 6 ; i++)
		{
			_innerArrows[i] = (Instantiate(_innerArrowPrefab.gameObject) as GameObject).GetComponent<SpriteBase>();
			_innerArrows[i].transform.localRotation = Quaternion.Euler(0,0,-60*i);
			_innerArrows[i].transform.parent = transform;
			_innerArrows[i].transform.localScale = Vector3.zero;
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
			dragButton.transform.localPosition = new Vector3 (0, -60f * counter, 0);
			
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
	
	private void ClearInnerArrows()
	{
		for (int i = 0 ; i < 6 ; i++)
		{
			_innerArrows[i].transform.localScale = Vector3.zero;
		}
	}
	
	private int GetSuggestedRotation(GrabbablePart part)
	{
		List<Construction.PartSide> partSides = new List<Construction.PartSide>();
		targetConstructions.ForEach((con) => partSides.AddRange(con.IsConnectable(part)));
		
		ClearInnerArrows();
		
		foreach(Construction.PartSide partSide in partSides)
		{
			Color drawColor = new Color [] {Color.green, Color.yellow, Color.blue, Color.red}[Mathf.Abs(partSide.offsetFromSide)];
			Vector3 offset = GameSettings.instance.hexCellPrefab.GetDirection(partSide.part.Absolute(partSide.relativeDirection));
			Debug.DrawLine(partSide.part.transform.position, partSide.part.transform.position + offset, drawColor);
			
			if (partSide.offsetFromSide == 0)
			{
				HexMetrics.Direction absoluteDir = partSide.part.Absolute(partSide.relativeDirection);
				_innerArrows[(int)absoluteDir].transform.localScale = Vector3.one;
				_innerArrows[(int)absoluteDir].transform.position = partSide.part.transform.position;
			}
//			Debug.Log ("is connectable "+partSide.part.name +" in "+partSide.relativeDirection+" ("+partSide.offsetFromSide+")");
			
		}
		
		
//		partSides.Sort((x, y) => Mathf.Abs(part.SimulationRotationDifference(x.relativeDirection)) - Mathf.Abs(part.SimulationRotationDifference(y.relativeDirection)));
		partSides.Sort((x, y) => Mathf.Abs(x.offsetFromSide) - Mathf.Abs(y.offsetFromSide));
		
//		Debug.Log (string.Join (",", partSides.ConvertAll((input) => ""+input.offsetFromSide).ToArray()));
		
		if (partSides.Count > 0)
		{	
			Construction.PartSide partSide = partSides[0];
//			Debug.Log ("rotating towards "+partSides[0].part.name+" ("+partSides[0].offsetFromSide+")");
			return partSide.offsetFromSide;
		}
		
		return 0;
	}
	
	void SetUpDragDropBehaviour(UIButton button, GrabbablePart part)
	{
		dragButtons[part.partType] = button;
		buttonToPart[button] = part;
		Debug.Log ("Setting up Drag Drop behaviour");
//		Debug.DebugBreak();
		button.SetDragDropDelegate((parms) => 
		{
			if (parms.evt == EZDragDropEvent.Begin)
			{
				Debug.Log ("Begin "+parms.dragObj/*+":"+parms.dragObj.transform.position+":"+parms.ptr.ray*/);
				
				
				DestroyMarkedPart();
				
					
			}
			if (parms.evt == EZDragDropEvent.Update)
			{
				
				bool rotateByOne = Input.GetMouseButtonUp(1);
					
				
				int rotationOffset = GetSuggestedRotation(part);
				part.RotatateSimulationOrientation(rotationOffset);
				
				if (rotateByOne)
				{
					RotatePart(part);
				}
//				Debug.Log (string.Join(",", partSides.ConvertAll((input) => ""+input.offsetFromSide).ToArray()));
			}
			if (parms.evt == EZDragDropEvent.Cancelled)
			{
				Debug.Log ("Cancelled "+parms.dragObj);
				Debug.Log ("DropTarget: "+parms.dragObj.DropTarget);
				
				if (targetConstructions.Count > 0)
				{
					ClearInnerArrows();
					
					ConnectPartWithBestRotation(part);
					
				}
				else
				{
					targetConstructions.Add(Construction.CreateSimpleConstruction(part.partType, (prefab) => Instantiate(prefab) as GameObject));
					targetConstructions[0].FirstPart.SimulationOrientation = part.SimulationOrientation;
					targetConstructions[0].transform.parent = _constructionHolder.transform;
					targetConstructions[0].transform.localPosition = Vector3.zero;
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
	
	private void DestroyMarkedPart()
	{
		if (partMarkedToDestroy != null)
		{
			if (targetConstructions.Contains(partMarkedToDestroy.ParentConstruction))
			{
				Debug.Log ("hit a part in our preview construction");
				
				foreach(Construction construction in partMarkedToDestroy.ParentConstruction.RemoveFromConstruction(partMarkedToDestroy))
				{
					Debug.Log(construction.name+" created "+(targetConstructions.Contains(construction)?"already in":"not in"));
					if (!targetConstructions.Contains(construction))
					{
						targetConstructions.Add(construction);
					}
				}
				targetConstructions.Remove(partMarkedToDestroy.ParentConstruction);
				
				
				Destroy (partMarkedToDestroy.ParentConstruction.gameObject);
				partMarkedToDestroy = null;
			}
		}
		if (targetConstructions.Count > 1)
		{
			targetConstructions.Sort( (x, y) => y.Count - x.Count );
			Construction firstConstruction = targetConstructions[0];
			for (int i = 1 ; i < targetConstructions.Count ; i++)
			{
				Destroy (targetConstructions[i].gameObject);
			}
			targetConstructions.Clear();
			targetConstructions.Add (firstConstruction);
		}
	}
	
	private void RotatePart(GrabbablePart part)
	{
		for (int i = 1 ; i < 6 ; i++)
		{
			part.RotatateSimulationOrientation(-i);
			
			
			int rotationTurnOffset = GetSuggestedRotation(part);
			if (rotationTurnOffset == 0)
			{
				break;
//				part.RotatateSimulationOrientation(rotationOffset);
			}
			
			part.RotatateSimulationOrientation(i);
		}
	}
	
	private void ConnectPartWithBestRotation(GrabbablePart part)
	{
		List<Construction.PartSide> partSides = new List<Construction.PartSide>();
		targetConstructions.ForEach((con) => partSides.AddRange(con.IsConnectable(part)));
//		partSides.Sort((x, y) => Mathf.Abs(x.offsetFromSide) - Mathf.Abs(y.offsetFromSide));
		
		if (partSides.Count == 0)
		{
			return;
		}
		
		GameObject partObject = Instantiate(GameSettings.instance.GetPartPrefab(part.partType).gameObject) as GameObject;
		GrabbablePart newPart = partObject.GetComponent<GrabbablePart>();
		newPart.SimulationOrientation = part.SimulationOrientation;
		
		partSides.RemoveAll((x) => x.offsetFromSide != 0);
//		if (partSides.Count > 0)
		foreach(Construction.PartSide partSide in partSides)
		{
			partSide.part.ConnectPartAndPlaceAtRelativeDirection(
				newPart, 
				GrabbablePart.PhysicalConnectionType.Weld, 
				partSide.relativeDirection,
				(con) => 
				{
					targetConstructions.Remove(con);
					Destroy(con.gameObject);
				});
		}
	}
	
	
	public event ConstructionSavedDelegate saveEvent;
	
	ConstructionSavedDelegate saveFunction = null;
	
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
		saveFunction = null;
	}
	
#region EZ GUI
	public void SaveConstruction ()
	{
		Debug.Log ("Saving");
		if (targetConstructions.Count > 0 && saveEvent != null)
		{
			saveEvent(targetConstructions[0].Encode());
		}
	}
#endregion
	
	IEnumerable<IEnumerable<GrabbablePart>> FindConstructionComponentsWithout(GrabbablePart ignorePart)
	{
		HashSet<GrabbablePart> unexploredParts = new HashSet<GrabbablePart>(ignorePart.ParentConstruction.Parts);
		unexploredParts.Remove(ignorePart);
		
		foreach(GrabbablePart connPart in ignorePart.GetConnectedParts())
		{
			if (unexploredParts.Contains(connPart))
			{
				yield return ExplorePart(connPart, unexploredParts);
			}
		}
		
	}
			
	IEnumerable<GrabbablePart> ExplorePart(GrabbablePart toExplore, HashSet<GrabbablePart> unexploredParts)
	{
		if (!unexploredParts.Contains(toExplore))
		{
			yield break;
		}
		unexploredParts.Remove(toExplore);
		yield return toExplore;
		
		foreach(GrabbablePart connPart in toExplore.GetConnectedParts())
		{
			foreach (GrabbablePart explored in ExplorePart(connPart, unexploredParts))
			{
				yield return explored;
			}
		}
	}
	
	public void HandleScreenPoint(POINTER_INFO pointerInfo, ControlState pressState, ControlState dragState)
	{
		
		Ray	inputRay = UIManager.instance.rayCamera.ScreenPointToRay(pointerInfo.devicePos);
		
//		HandleRay(inputRay, pressState, dragState);
//	}
	
//	public void HandleRay(Ray inputRay, ControlState pressState, ControlState dragState)
//	{
//		Debug.Log(inputRay+":"+pressState);
		
		Debug.DrawRay(inputRay.origin, inputRay.direction*100, Color.red);
		
//		for (int i = 0 ; i < 5 ; i++)
//		{
//			if (Input.GetMouseButton(i))
//			{
//				Debug.Log("mouse "+i);
//			}
//		}
		
		GrabbablePart hitPart = null;
		foreach (RaycastHit hitInfo in Physics.RaycastAll(inputRay, 1000, 1 << LayerMask.NameToLayer("GrabbablePart")))
		{
//			Debug.Log ("hit somthing "+hitInfo.collider.gameObject.name);
			Transform hitParent = hitInfo.collider.transform.parent;
			
			if (hitParent == null) continue;
			
			hitPart = hitParent.gameObject.GetComponent<GrabbablePart>();
			
			if (hitPart == null) continue;
//			Debug.Log ("hit a part");
			
		}
		
		if (targetConstructions.Count == 0)
		{
			return;
		}
		
		foreach(GrabbablePart part in targetConstructions[0].Parts)
		{
			part.highlighted = false;
		}
		
		
		if (hitPart == null)
		{
			return;
		}
		
		List<List<GrabbablePart>> partLists = new List<List<GrabbablePart>>();
		foreach (IEnumerable<GrabbablePart> partList in FindConstructionComponentsWithout(hitPart))
		{
			partLists.Add(new List<GrabbablePart>(partList));
		}
		partLists.Sort((x, y) => x.Count - y.Count);
		
//		Debug.Log(string.Join(", ", partLists.ConvertAll((input) => ""+input.Count).ToArray()));
		
		hitPart.highlighted = true;
		if (partLists.Count > 1)
		{
			foreach(GrabbablePart part in partLists[0])
			{
				part.highlighted = true;
			}
		}
		
		bool destroyPart = false;
		bool rotatePart = false;
		
		bool createAndDragPart = false;
		if (pressState == ControlState.Ending && dragState == ControlState.Inactive)
		{
			Debug.Log("Tap destroy "+Input.GetMouseButtonUp(0)+":"+Input.GetMouseButtonUp(1));
			
			if (Input.GetMouseButtonUp(1))
			{
				rotatePart = true;
			}
			else
			{
				destroyPart = true;
			}
		}
		if (Input.GetMouseButton(0) && dragState == ControlState.Starting)
		{
			Debug.Log("Drag");
			createAndDragPart = true;
		}
		
		
		if ( createAndDragPart )
		{
			UIButton dragButton = dragButtons[hitPart.partType];
			
			InputCatcher.instance.Retarget(dragButton);
			buttonToPart[dragButton].SimulationOrientation = hitPart.SimulationOrientation;

			partMarkedToDestroy = hitPart;
		}
		
		if (rotatePart)
		{
			for (int i = 1 ; i < 6 ; i++)
			{
				Debug.Log ("Checking direction "+(-i));
				if (!hitPart.WillSimulationOrientationRotatateSplitConstruction(-i))
				{
					hitPart.RotatateSimulationOrientation(-i);
					break;
				}
			}
		}
		
		if (destroyPart)
		{
			partMarkedToDestroy = hitPart;
			DestroyMarkedPart();

		}
		
		
		
	}
}










