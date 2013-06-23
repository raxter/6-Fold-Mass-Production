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
	
	ConstructionMaker.ConstructionSavedDelegate saveFunction = null;
	
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
				ConstructionMaker.instance.OpenMaker(PreviewedConstruction.Encode(), saveFunction);
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
		
	}
	
	public void SetPreviewedConstruction(Construction construction, ConstructionMaker.ConstructionSavedDelegate saveDelegate)
	{
		if (_previewedConstruction != null)
		{
			_previewedConstruction.transform.position = Vector3.zero;
//				_previewedConstruction.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Default"));
		}
		_previewedConstruction = construction;
		
		saveFunction = saveDelegate;
		if (_previewedConstruction != null)
		{
			_previewedConstruction.transform.position = transform.position;
			_previewedConstruction.transform.parent = transform;
//				_previewedConstruction.gameObject.SetLayerRecursively(LayerMask.NameToLayer("GUI"));
		}
		else
		{
			saveFunction = null;
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
		
		SetPreviewedConstruction(
			selectedGenerator == null ? null : selectedGenerator.toGenerateConstruction, 
			(encoded) => 
			{
				Destroy(selectedGenerator.toGenerateConstruction.gameObject);
				selectedGenerator.toGenerateConstruction = Construction.Decode(encoded, (prefab) => Instantiate(prefab) as GameObject);
			});
		
		if (PreviewedConstruction == null)
		{
			ConstructionMaker.instance.CloseMaker();
		}
	}
}
