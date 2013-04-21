using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : SingletonBehaviour<GameManager> 
{
	
	[SerializeField]
	List<HexCellPlaceable> placeablePrefabs = null;
	
	Dictionary<HexCellPlaceableType, HexCellPlaceable> cellMechanisms;
	
	HexCellPlaceable unplacedMechanism = null;
	
	[SerializeField]
	GUIEnabler [] _enableableGUIObjects = null;
	
	[SerializeField]
	UIButton _playButton = null;
	[SerializeField]
	UIButton _stopButton = null;
	
	public bool guiEnabled { get; private set; }
	
	void Start()
	{
		cellMechanisms = new Dictionary<HexCellPlaceableType, HexCellPlaceable>();
		
		cellMechanisms.Add(HexCellPlaceableType.None, null);
		
		foreach (HexCellPlaceable placeable in placeablePrefabs)
		{
			cellMechanisms.Add(placeable.MechanismType, placeable);
		}
		StopSimulation();
	}
	
	public void CreateMechanism(HexCellPlaceableType cellMechanismType)
	{
		if (cellMechanismType == HexCellPlaceableType.None)
			return;
		
		unplacedMechanism = (GameObject.Instantiate(cellMechanisms[cellMechanismType].gameObject) as GameObject).GetComponent<HexCellPlaceable>();
		
		InputManager.instance.StartDraggingUnplaced(unplacedMechanism);
		
	}
	
	public void PlaySimulation()
	{
		Debug.Log ("PlaySimulation");
		// Disable Input
		foreach (GUIEnabler guiElement in _enableableGUIObjects)
		{
			guiElement.EnableGUI(false);
			guiEnabled = false;
		}
		
		_playButton.transform.localScale = Vector3.zero;
		_stopButton.transform.localScale = Vector3.one;
		// change play to stop
		// start simulation
		
		List<HexCellPlaceable> mechanisms = new List<HexCellPlaceable>();
		foreach (HexCell hexCell in GridManager.instance.GetAllCells())
		{
			if (hexCell.placedMechanism != null)
			{
				mechanisms.Add(hexCell.placedMechanism);
			}
		}
		// MAKE THE THINGS DO THE THINGS!
	}
	public void StopSimulation()
	{
		Debug.Log ("StopSimulation");
		// Reenable Input
		
		foreach (GUIEnabler guiElement in _enableableGUIObjects)
		{
			guiElement.EnableGUI(true);
			guiEnabled = true;
		}
		
		// change stop to play
		_playButton.transform.localScale = Vector3.one;
		_stopButton.transform.localScale = Vector3.zero;
		
		// start simulation
	}
	
//	public void UnSelecteMechanistIcon ()
//	{
//		if (unplacedMechanism != null)
//		{
//			unplacedMechanism.StopDrag();
//			
//		}
//	}
	
}









