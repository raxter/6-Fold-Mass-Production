using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : SingletonBehaviour<GameManager> 
{
	
	[SerializeField]
	List<Mechanism> mechanismPrefabs = null;
	
	
	[SerializeField]
	PartGenerator generatorPrefab = null;
	
	Dictionary<MechanismType, Mechanism> cellMechanisms;
	
	Mechanism unplacedMechanism = null;
	
	[SerializeField]
	GUIEnabler [] _enableableGUIObjects = null;
	
	[SerializeField]
	UIButton _playButton = null;
	[SerializeField]
	UIButton _stopButton = null;
	
	
	float _stepsPerSecond = 120f;
	int _stepsPerInstruction = 60;
	
	public bool guiEnabled { get; private set; }
	
	public enum State {Construction, Simulation, SimulationFailed};
	
	public State gameState;
	
	public string loadLevelOnStart = "";
	
	void Start()
	{
		LoadLevel(loadLevelOnStart);
		
		gameState = State.Construction;
		cellMechanisms = new Dictionary<MechanismType, Mechanism>();
		
		cellMechanisms.Add(MechanismType.None, null);
		
		foreach (Mechanism mechanism in mechanismPrefabs)
		{
			cellMechanisms.Add(mechanism.MechanismType, mechanism);
		}
		StopSimulation();
	}

	void LoadLevel (string leveName)
	{
		// clear all parts first when reloading! TODO
		
		LevelSettings.Level lvl = LevelSettings.instance.GetLevel(leveName);
		
		foreach (LevelSettings.GeneratorDetails generatorDetails in lvl.generators)
		{
			PartGenerator generator = (GameObject.Instantiate(generatorPrefab.gameObject) as GameObject).GetComponent<PartGenerator>();
			generator.toGeneratePrefab = generatorDetails.toGeneratePrefab;
			generator.PlaceAtLocation(generatorDetails.location);
		}
		
	}
	
	public void CreateMechanism(MechanismType cellMechanismType)
	{
		if (cellMechanismType == MechanismType.None)
			return;
		
		unplacedMechanism = (GameObject.Instantiate(cellMechanisms[cellMechanismType].gameObject) as GameObject).GetComponent<Mechanism>();
		
		InputManager.instance.StartDraggingUnplaced(unplacedMechanism);
		
	}
	
	IEnumerator SimulationCoroutine()
	{
		int programCounter = -1;
		List<HexCellPlaceable> placeables = new List<HexCellPlaceable>();
		List<Grabber> grabbers = new List<Grabber>();
		List<PartGenerator> generators = new List<PartGenerator>();
		foreach (HexCell hexCell in GridManager.instance.GetAllCells())
		{
			if (hexCell.placedPlaceable != null)
			{
				placeables.Add(hexCell.placedPlaceable);
				if (hexCell.placedPlaceable is Grabber)
				{
					grabbers.Add(hexCell.placedPlaceable as Grabber);
				}
				if (hexCell.placedPlaceable is PartGenerator)
				{
					generators.Add(hexCell.placedPlaceable as PartGenerator);
				}
			}
		}
		
		
		int stepsToDoThisFrame = 0;
		float spareTime = 0;
		
		foreach (Grabber grabber in grabbers)
		{
			grabber.StartSimulation(_stepsPerInstruction);
		}
		
		while (true)
		{
			
			foreach (PartGenerator generator in generators)
			{
				generator.StepPreStart();
			}
			// perform instruction
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformInstruction();
				Debug.Log("PerformInstruction "+grabber._instructionCounter);
			}
			
			// perform step
			while (true)
			{
				if (gameState == State.Construction)
				{
					Debug.Log ("Simulation ending");
					foreach (Grabber grabber in grabbers)
					{
						grabber.EndSimulation();
					}
					yield break;
				}
				
				
				if (stepsToDoThisFrame == 0)
				{
					yield return null;
					// in new frame!
					float timeThisFrame = Time.deltaTime+spareTime;
					float stepTime = 1f/_stepsPerSecond;
					stepsToDoThisFrame = (int)(timeThisFrame/stepTime);
					spareTime = timeThisFrame - stepsToDoThisFrame*stepTime;
//					Debug.Log (timeThisFrame+"("+spareTime+")");
				}
				
				stepsToDoThisFrame -= 1;
					
				foreach (Grabber grabber in grabbers)
				{
					grabber.PerformStep();
//					Debug.Log("PerformStep "+grabber._stepCounter);
				}
				
				bool allFinished = true;
				
				foreach (Grabber grabber in grabbers)
				{
					if (!grabber.StepFinished())
					{
						allFinished = false;
					}
				}
				// check for collisions
			
				// if collision, pause and exit
				
				if (allFinished)
				{
				
					break;
				}
				
				
				
				
				bool debugForceExit = false;
				
				if (debugForceExit) // make this true in debugger to get out of an infinite loop
				{
					yield break;
				}
				
			}
			
		}
		
		
		
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
	
//	IEnumerator SimulationStopCoroutine()
//	{
//		yield return null;
//	}
	
	public void StopSimulation()
	{
		gameState = State.Construction;
		
		Debug.Log ("StopSimulation");
		// Reenable Input
		
//		StartCoroutine(SimulationStopCoroutine());
		
		foreach (HexCell hexCell in GridManager.instance.GetAllCells())
		{
			GrabbablePart part = hexCell.part;
			if (part != null)
			{
				part.PlaceAtLocation(null);
				GameObject.Destroy(part.gameObject);
			}
			
			Grabber grabber = hexCell.placedMechanism as Grabber;
			if (grabber != null)
			{
				grabber.ClearClamp();
			}
		}
		
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









