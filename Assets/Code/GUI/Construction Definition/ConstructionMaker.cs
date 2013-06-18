using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionMaker : SingletonBehaviour<ConstructionMaker> 
{
	Construction targetConstruction = null;
	
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
		foreach (PartType partType in GameSettings.instance.partPrefabs.ConvertAll<PartType>((input) => input.partType))
//		foreach (GrabbablePart prefab in GameSettings.instance.partPrefabs)
		{
			Construction partConstruction = Construction.CreateSimpleConstruction(partType, (prefab) => Instantiate(prefab) as GameObject);
			partConstruction.ignoreCollisions = true;
			
			GameObject dragButton = Instantiate(_draggableButtonPrefab.gameObject) as GameObject;
			
			SetUpDragDropBehaviour(dragButton.GetComponent<UIButton>(), partConstruction.CenterPart);
			dragButton.SetActive(true);
//			_dragButtons.Add(dragButton.GetComponent<UIButton>());
			
			dragButton.transform.parent = _partHolder.transform;
			dragButton.transform.localPosition = new Vector3 (0, -60f * counter, 0);
			
			partConstruction.transform.parent = dragButton.transform;
			partConstruction.transform.localPosition = Vector3.zero;
			
//			GameObject partObject = Instantiate(prefab.gameObject) as GameObject;
//			partObject.transform.parent = _partHolder.transform;
//			partObject.transform.position = Vector3.zero;
//			partObject.transform.localPosition = new Vector3 (0, -60f * counter, 0);
			
			counter += 1;
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
				targetConstruction.IsConnectable(part);
			}
			if (parms.evt == EZDragDropEvent.Cancelled)
			{
				Debug.Log ("Cancelled "+parms.dragObj);
				Debug.Log ("DropTarget: "+parms.dragObj.DropTarget);
				
				List<Construction.PartSide> partSides = targetConstruction.IsConnectable(part);
				if (partSides.Count > 0)
				{
					GameObject partObject = Instantiate(GameSettings.instance.GetPartPrefab(part.partType).gameObject) as GameObject;
					GrabbablePart newPart = partObject.GetComponent<GrabbablePart>();
					partSides[0].part.ConnectPartAndPlaceAtRelativeDirection(newPart, GrabbablePart.PhysicalConnectionType.Weld, partSides[0].relativeDirection);
				}
				
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
		
		
		targetConstruction = Construction.Decode(code, (prefab) => Instantiate(prefab) as GameObject);
		targetConstruction.transform.parent = _constructionHolder.transform;
		targetConstruction.transform.localPosition = Vector3.zero;
//		construction.gameObject.SetLayerRecursively(gameObject.layer);
	}
	
	public void CloseMaker ()
	{
//		this.transform.localScale = Vector3.zero;
		this.gameObject.SetActive(false);
		
		InputCatcher.instance.ReleaseInputOverride(HandleScreenPoint);
		
		if (targetConstruction != null)
		{
			Destroy(targetConstruction.gameObject);
			targetConstruction = null;
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
				
				if (hitPart.ParentConstruction == targetConstruction)
				{
					Debug.Log ("hit a part in our preview construction");
					
					targetConstruction.RemoveFromConstruction(hitPart);
					Destroy (hitPart.gameObject);
				}
			}
		}
		
	}
}










