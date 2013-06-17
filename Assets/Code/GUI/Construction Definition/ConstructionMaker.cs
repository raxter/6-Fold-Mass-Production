using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionMaker : SingletonBehaviour<ConstructionMaker> 
{
	Construction construction = null;
	
	[SerializeField]
	UIButton _draggableButtonPrefab = null;
	
//	List<UIButton> _dragButtons = new List<UIButton>();
	
//	[SerializeField]
//	Camera _makerCamera = null;
	
	[SerializeField]
	GameObject _partHolder = null;

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
		button.SetDragDropDelegate((parms) => 
		{
			if (parms.evt == EZDragDropEvent.Begin)
			{
				Debug.Log ("Begin "+parms.dragObj/*+":"+parms.dragObj.transform.position+":"+parms.ptr.ray*/);
					
			}
			if (parms.evt == EZDragDropEvent.Update)
			{
				construction.IsConnectable(part);
			}
			if (parms.evt == EZDragDropEvent.Cancelled)
			{
				Debug.Log ("Cancelled "+parms.dragObj);
				Debug.Log ("DropTarget: "+parms.dragObj.DropTarget);
				
				List<Construction.PartSide> partSides = construction.IsConnectable(part);
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
		
		InputCatcher.instance.blockInput = true;
		
		construction = Construction.Decode(code, (prefab) => Instantiate(prefab) as GameObject);
		construction.transform.parent = transform;
		construction.transform.localPosition = Vector3.zero;
		construction.gameObject.SetLayerRecursively(gameObject.layer);
	}
	
	public void CloseMaker ()
	{
//		this.transform.localScale = Vector3.zero;
		this.gameObject.SetActive(false);
		InputCatcher.instance.blockInput = false;
		
		if (construction != null)
		{
			Destroy(construction.gameObject);
			construction = null;
		}
	}
}
