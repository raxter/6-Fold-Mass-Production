using UnityEngine;
using System.Collections;

public class ConstructionPreview : MonoBehaviour 
{
	[SerializeField]
	UIButton _previewWindow;
	
	// Use this for initialization
	void Start () 
	{
		_previewWindow.gameObject.SetActive(false);
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
	
	public void OnSaveEvent(Construction construction)
	{
		if (saveFunction != null)
		{
			saveFunction(construction);
			SetPreviewedConstructionInternal(construction);
		}
	}
	
	#region EZGUI Button calls
	void ToggleMaker()
	{
		if (!LevelEditorGUI.hasActiveInstance)
			return;
		
		if (PreviewedConstruction != null)
		{
			if (ConstructionMaker.instance.Open)
			{
				ConstructionMaker.instance.CloseMaker();
			}
			else
			{
				ConstructionMaker.instance.OpenMaker(PreviewedConstruction);
			}
		}
		
	}
	
	
	void SelectTarget()
	{
		Debug.Log ("Selecting Target"+GridManager.instance.target);
		InputManager.instance.ClearSelection();
		SetPreviewedConstruction(GridManager.instance.target, 
		(construction) => 
		{
			GridManager.instance.SetTarget(construction);
		});
		
		_previewWindow.gameObject.SetActive(true);
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
	
	void SetPreviewedConstructionInternal(Construction construction)
	{
		if (construction == null)
		{
			PreviewedConstruction = null;
		}
		else 
		{
			
			PreviewedConstruction = Construction.DecodeCopy(construction);
		}
	}
	
	public void SetPreviewedConstruction(Construction construction, ConstructionSavedDelegate saveDelegate)
	{
		SetPreviewedConstructionInternal(construction);
		
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
			selectedGenerator == null ? null : selectedGenerator.toGenerateConstruction, 
			(construction) => 
			{
				Debug.LogWarning("encoding "+construction.name,this);
				ObjectPoolManager.DestroyObject(selectedGenerator.toGenerateConstruction);
				selectedGenerator.toGenerateConstruction = Construction.DecodeCopy(construction);
				selectedGenerator.toGenerateConstruction.ignoreCollisions = true;
			});
		
		_previewWindow.gameObject.SetActive(selectedGenerator != null);
		
		if (PreviewedConstruction == null)
		{
			ConstructionMaker.instance.CloseMaker();
		}
	}
}
