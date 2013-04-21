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
	
	public enum State {Construction, Simulation, PausedSimulation};
	
	public State gameState;
	
	void Start()
	{
		
		gameState = State.Construction;
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
	
	IEnumerator SimulationCoroutine()
	{
		int programCounter = -1;
		List<HexCellPlaceable> mechanisms = new List<HexCellPlaceable>();
		List<Grabber> grabbers = new List<Grabber>();
		foreach (HexCell hexCell in GridManager.instance.GetAllCells())
		{
			if (hexCell.placedMechanism != null)
			{
				mechanisms.Add(hexCell.placedMechanism);
				if (hexCell.placedMechanism is Grabber)
				{
					grabbers.Add(hexCell.placedMechanism as Grabber);
				}
			}
		}
		foreach (Grabber grabber in grabbers)
		{
			grabber.instructionCounter = 0;
		}
		
		// perform instruction
		
		// wait for sucess or failure
		
		// repeat
		
		yield break;
	}
	
	public void PlaySimulation()
	{
		gameState = State.Simulation;
		
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
		
		// MAKE THE THINGS DO THE THINGS!
		
		StartCoroutine(SimulationCoroutine());
	}
	public void StopSimulation()
	{
		gameState = State.Construction;
		
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









