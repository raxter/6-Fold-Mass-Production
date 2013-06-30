using UnityEngine;
using System.Collections;

public class ConstructionPreview : MonoBehaviour 
{
	
	
	// Use this for initialization
	void Start () 
	{
		InputManager.instance.OnSelectionChange += OnSelectionChange;
		ConstructionMaker.instance.saveEvent += OnSaveEvent;
	}
	
	void OnDestroy()
	{
		if (InputManager.hasInstance)
		{
			InputManager.instance.OnSelectionChange -= OnSelectionChange;
		}
		if (ConstructionMaker.hasInstance)
		{
			ConstructionMaker.instance.saveEvent -= OnSaveEvent;
		}
	}
	
//	public delegate void CreateConstructionDelegate(string encoded);
	ConstructionSavedDelegate saveFunction = null;
	
	public void OnSaveEvent(string encoded)
	{
		if (saveFunction != null)
		{
			saveFunction(encoded);
			SetPreviewedConstructionInternal(encoded);
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
	
	
	void SelectTarget()
	{
		InputManager.instance.ClearSelection();
		SetPreviewedConstruction(GridManager.instance.target.Encode(), 
		(encoded) => 
		{
			ObjectPoolManager.DestroyObject(GridManager.instance.target);
			GridManager.instance.SetTarget(encoded);
		});
		
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
				ObjectPoolManager.DestroyObject(_previewedConstruction);
//				_previewedConstruction.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Default"));
			}
			_previewedConstruction = value;
			if (_previewedConstruction != null)
			{
				_previewedConstruction.transform.position = transform.position;
				_previewedConstruction.transform.parent = transform;
//				_previewedConstruction.gameObject.SetLayerRecursively(LayerMask.NameToLayer("GUI"));
			}
			
		}
		
	}
	
	void SetPreviewedConstructionInternal(string encoded)
	{
		if (encoded == "")
		{
			PreviewedConstruction = null;
		}
		else 
		{
			PreviewedConstruction = Construction.Decode(encoded);
		}
	}
	
	public void SetPreviewedConstruction(string encoded, ConstructionSavedDelegate saveDelegate)
	{
		SetPreviewedConstructionInternal(encoded);
		
		saveFunction = PreviewedConstruction == null ? null : saveDelegate;
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
			selectedGenerator == null ? "" : selectedGenerator.toGenerateConstruction == null ? "" : selectedGenerator.toGenerateConstruction.Encode(), 
			(encoded) => 
			{
				Debug.LogWarning("encoding "+encoded,this);
				ObjectPoolManager.DestroyObject(selectedGenerator.toGenerateConstruction);
				selectedGenerator.toGenerateConstruction = Construction.Decode(encoded);
				selectedGenerator.toGenerateConstruction.ignoreCollisions = true;
			});
		
		if (PreviewedConstruction == null)
		{
			ConstructionMaker.instance.CloseMaker();
		}
	}
}
