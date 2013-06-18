using UnityEngine;
using System.Collections;

public class ConstructionPreview : MonoBehaviour {
	
	
	// Use this for initialization
	void Start () 
	{
		InputManager.instance.OnSelectionChange += OnSelectionChange;
	}
	
	void OnDestroy()
	{
		if (InputManager.hasInstance)
		{
			InputManager.instance.OnSelectionChange -= OnSelectionChange;
		}
	}
	
	#region EZGUI Button calls
	void ToggleMaker()
	{
		if (PreviewedConstruction != null)
		{
			if (ConstructionMaker.instance.Open)
			{
				ConstructionMaker.instance.CloseMaker();
			}
			else
			{
				ConstructionMaker.instance.OpenMaker(PreviewedConstruction.Encode());
			}
		}
		
	}
		
	#endregion
	
	Construction _previewedConstruction = null;
	
	Construction PreviewedConstruction
	{
		get
		{
			return _previewedConstruction;
		}
		set
		{
			if (_previewedConstruction != null)
			{
				_previewedConstruction.transform.position = Vector3.zero;
//				_previewedConstruction.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Default"));
			}
			_previewedConstruction = value;
			
			if (_previewedConstruction != null)
			{
				_previewedConstruction.transform.position = transform.position;
//				_previewedConstruction.gameObject.SetLayerRecursively(LayerMask.NameToLayer("GUI"));
			}
			
			
		}
	}
	
	void OnSelectionChange(System.Collections.Generic.List<HexCellPlaceable> selectedPlacables) 
	{
		PartGenerator selectedGenerator = null;
		foreach (HexCellPlaceable placable in selectedPlacables)
		{
			if (placable is PartGenerator)
			{
				if (selectedGenerator != null)
				{
					selectedGenerator = null;
					break;
				}
				selectedGenerator = placable as PartGenerator;
			}
		}
		
		PreviewedConstruction = selectedGenerator == null ? null : selectedGenerator.toGenerateConstruction;
		
		if (PreviewedConstruction == null)
		{
			ConstructionMaker.instance.CloseMaker();
		}
	}
}
