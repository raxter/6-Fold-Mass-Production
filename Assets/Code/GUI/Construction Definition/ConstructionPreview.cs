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
		InputManager.instance.OnSelectionChange -= OnSelectionChange;
	}
	
	Construction previewedConstruction = null;
	
	Construction PreviewedConstruction
	{
		get
		{
		}
		set
		{
			if (previewedConstruction != null)
			{
				previewedConstruction.transform.position = Vector3.zero;
			}
			previewedConstruction = value;
			
			if (previewedConstruction != null)
			{
				previewedConstruction.transform.position = transform.position;
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
		
		PreviewedConstruction = selectedGenerator.toGenerateConstruction;
		
	}
}
