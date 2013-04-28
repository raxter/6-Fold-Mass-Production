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
	
//	[SerializeField]
//	UIButton _playButton = null;
	[SerializeField]
	UIButton _stopButton = null;
	
	[SerializeField]
	SpriteText _targetText = null;
	
	LevelSettings.Level _currentLevel = null;
	
	int _completedConstructions = -1;
	int completedConstructions
	{
		get { return _completedConstructions; }
		set 
		{ 
			_completedConstructions = value; 
			_targetText.Text = "Target\n"+_completedConstructions+"/"+_currentLevel.targetConstructions;
		}
	}
	
	enum SimulationSpeed {Stopped, Paused, Normal, Fast, Faster, Fastest};
	
	SimulationSpeed _currentSpeed = SimulationSpeed.Stopped;
	
	[SerializeField]
	float instructionsPerSecondNormal = 1;
	[SerializeField]
	float instructionsPerSecondFast = 2;
	[SerializeField]
	float instructionsPerSecondFaster = 4;
	[SerializeField]
	float instructionsPerSecondFastest = 16;
	
	float _instructionsPerSecond = 1;
	int _stepsPerInstruction = 60;
	
	float stepsPerSecond { get { return _stepsPerInstruction * _instructionsPerSecond; } }
	
	public bool guiEnabled { get; private set; }
	
	public enum State {Construction, Simulation, SimulationFailed};
	
	public State gameState
	{
		get 
		{ 
			return _gameState; 
		}
		set 
		{
			SetState(value);
//			_gameState = value;
		}
	}
	
	void SetState(State newState)
	{
		_gameState = newState;
	}
	
	State _gameState;
	
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
		
		_currentLevel = LevelSettings.instance.GetLevel(leveName);
		
		foreach (LevelSettings.GeneratorDetails generatorDetails in _currentLevel.generators)
		{
			PartGenerator generator = (GameObject.Instantiate(generatorPrefab.gameObject) as GameObject).GetComponent<PartGenerator>();
			generator.toGeneratePrefab = generatorDetails.toGeneratePrefab;
			generator.PlaceAtLocation(generatorDetails.location);
		}
		completedConstructions = 0;
	}

	public void AddCompletedConstruction ()
	{
		completedConstructions += 1;
		
		if (_completedConstructions >= _currentLevel.targetConstructions)
		{
		}
	}

	public void IncompleteConstruction ()
	{
		throw new System.NotImplementedException ();
	}
	
	
	public void CreateMechanism(MechanismType cellMechanismType)
	{
		if (cellMechanismType == MechanismType.None)
			return;
		
		unplacedMechanism = (GameObject.Instantiate(cellMechanisms[cellMechanismType].gameObject) as GameObject).GetComponent<Mechanism>();
		
		InputManager.instance.StartDraggingUnplaced(unplacedMechanism);
		
	}
	
	
	int stepsToDoThisFrame = 0;
	
	IEnumerator SimulationCoroutine()
	{
		float startFrameTime = 0;
		
		int programCounter = -1;
		List<HexCellPlaceable> placeables = new List<HexCellPlaceable>();
		List<Grabber> grabbers = new List<Grabber>();
		List<PartGenerator> generators = new List<PartGenerator>();
		HashSet<WeldingRig> welders = new HashSet<WeldingRig>();
		
		HashSet<GrabbablePart> parts = new HashSet<GrabbablePart>();
		
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
				if (hexCell.placedPlaceable is WeldingRig)
				{
					welders.Add(hexCell.placedPlaceable as WeldingRig);
				}
			}
		}
		
		
		stepsToDoThisFrame = 0;
		float spareTime = 0;
		
		foreach (PartGenerator generator in generators)
		{
			generator.StartSimulation();
		}
		foreach (Grabber grabber in grabbers)
		{
			grabber.StartSimulation(_stepsPerInstruction);
		}
		
		while (true)
		{
			
			
			/**
			 * Description of events in an instruction loop:
			 * 
			 * Preamble: Each instruction is split into a number of steps, simulation continuously performs instructions until win or failure
			 * 
			 * PreStart 1:
			 * Generators generates a Part if there is space (stores this part)
			 * 
			 * PreStart 2:
			 * Welders check if there are any Parts over it (dropped or held) and welds them together
			 * 
			 * Start:
			 * Grabber sets up instructions:
			 *		Cell held by the Grabber is deregistered with the Cell below it
			 *		For Movement and Rotate, it sets up where it'll move
			 *		For Drop, it immeditaely drops whatever Part is held by Clamp onto the Cell:
			 *			The dropped Part is checked to see if it is on the finish cell and part of a completed construction
			 *			If the Part is completed successfullly is is destroyed
			 *			If the part is dropped onto nothing, the simulation fails and the offending Part is highlighted
			 *		For Grab, it does nothing
			 * 
			 * PostStart 1:
			 * Grabber performs Grab instruction if it has one (picks up whatever Part on the Cell under the Clamp)
			 * 
			 * PostStart 2:
		 	 * The Parts list is checked for null (destroyed) Parts
			 * 
			 * MainSteps: Performs the intermedite steps to do the Movement and Rotation instructions:
			 * 		if simulation failed, force a loop
			 * 		if simulation was exited, reset all grabbers
			 * 
			 * 		Grabber moves their arms one step (set up in Start phase):
			 * 			If the step is finished the Cell held by the Grabber is registered with the Cell below it *********
			 * 		All parts are checked for collisions:
			 * 			If there is a collision, the offending Parts are highlighted and the simulation is set to a failure state
			 * 
			 **/
			
			foreach (PartGenerator generator in generators)
			{
				GrabbablePart newPart = generator.StepPreStart();
				if (newPart)
				{
					parts.Add(newPart);
				}
			}
			
			
			foreach (WeldingRig welder in welders)
			{
				welder.PerformPreStart();
//				Debug.Log("PerformPostInstruction "+grabber._instructionCounter);
			}
			
			// perform instruction
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformInstruction();
//				Debug.Log("PerformInstruction "+grabber._instructionCounter);
			}
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformPostInstruction();
//				Debug.Log("PerformPostInstruction "+grabber._instructionCounter);
			}
			
			parts.RemoveWhere((obj) => obj == null);
			
			
			// inbetween 'steps'
			
			if (_currentSpeed != SimulationSpeed.Fastest && _currentSpeed != SimulationSpeed.Paused)
			{
				yield return new WaitForSeconds(0.2f/_instructionsPerSecond);
			}
			
			
			// perform steps
			while (true)
			{
				
				bool debugForceExit = false;
				
				if (debugForceExit) // make this true in debugger to get out of an infinite loop
				{
					yield break;
				}
				
				
				if (gameState == State.SimulationFailed)
				{
					yield return null;
					continue;
				}
				
				if (gameState == State.Construction)
				{
					Debug.Log ("Simulation ending");
					
					foreach (Grabber grabber in grabbers)
					{
						grabber.EndSimulation();
					}
					yield break;
				}
				
				
				
//				Debug.Log (Time.timeSinceLevelLoad);
//				Debug.Log (Time.realtimeSinceStartup +"-"+ startFrameTime+"="+(Time.realtimeSinceStartup - startFrameTime) +">"+ Time.fixedDeltaTime);
				if ((Time.realtimeSinceStartup - startFrameTime) > (Time.fixedDeltaTime))
				{
					stepsToDoThisFrame = 0;
				}
				
				if (stepsToDoThisFrame <= 0)
				{
					yield return null;
					
					if (_instructionsPerSecond == 0)
					{
						continue;
					}
					
					// in new frame!
					startFrameTime = Time.realtimeSinceStartup;
					
					float timeThisFrame = Time.deltaTime+spareTime;
					float stepTime = 1f/stepsPerSecond;
					stepsToDoThisFrame += (int)(timeThisFrame/stepTime);
					spareTime = timeThisFrame - stepsToDoThisFrame*stepTime;
//					Debug.Log (timeThisFrame+"("+spareTime+")");
				}
				
				
				
				stepsToDoThisFrame -= 1;
				
				
				bool allFinished = true;
				
				foreach (Grabber grabber in grabbers)
				{
					if (!grabber.PerformStep())
					{
						allFinished = false;
					}
//					Debug.Log("PerformStep "+grabber._stepCounter);
				}
				
				
//				foreach (Grabber grabber in grabbers)
//				{
//					if (!grabber.StepFinished())
//					{
//						allFinished = false;
//					}
//				}
				// check for collisions
			
				// if collision, pause and exit
				
//				parts.RemoveWhere((obj) => obj == null);
				
				
				
				foreach (GrabbablePart part in parts)
				{
					GrabbablePart other = part.CheckForCollisions();
					if (other != null)
					{
						part.selected = true;
						other.selected = true;
						gameState = State.SimulationFailed;
						continue;
					}
				}
				
				
				if (allFinished)
				{
				
					break;
				}
				
				
			}
			
		}
		
		
		
		// repeat
		
		yield break; 
	}
	
	
	public void PlaySimulationPaused()
	{
		_instructionsPerSecond = 0;
		_currentSpeed = SimulationSpeed.Paused;
		PlaySimulation();
	}
	
	public void PlaySimulationNormal()
	{
		_instructionsPerSecond = instructionsPerSecondNormal;
		_currentSpeed = SimulationSpeed.Normal;
		PlaySimulation();
	}
	
	public void PlaySimulationFast()
	{
		_instructionsPerSecond = instructionsPerSecondFast;
		_currentSpeed = SimulationSpeed.Fast;
		PlaySimulation();
	}
	
	public void PlaySimulationFaster()
	{
		_instructionsPerSecond = instructionsPerSecondFaster;
		_currentSpeed = SimulationSpeed.Faster;
		PlaySimulation();
	}
	
	public void PlaySimulationFastest()
	{
		_instructionsPerSecond = instructionsPerSecondFastest;
		_currentSpeed = SimulationSpeed.Fastest;
		PlaySimulation();
	}
	
	
	public void PlaySimulation()
	{
		if (_gameState != State.Construction)
		{
			return;
		}
		
		Debug.Log ("PlaySimulation");
		// Disable Input
		foreach (GUIEnabler guiElement in _enableableGUIObjects)
		{
			guiElement.EnableGUI(false);
			guiEnabled = false;
		}
		
//		_playButton.transform.localScale = Vector3.zero;
		_stopButton.transform.localScale = Vector3.one;
		
		_currentSpeed = SimulationSpeed.Stopped;
		
		gameState = State.Simulation;
		
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
			GrabbablePart part = hexCell.partOnCell;
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
//		_playButton.transform.localScale = Vector3.one;
		_stopButton.transform.localScale = Vector3.zero;
		
		
		completedConstructions = 0;
	}
	
//	public void UnSelecteMechanistIcon ()
//	{
//		if (unplacedMechanism != null)
//		{
//			unplacedMechanism.StopDrag();
//			
//		}
//	}
	
	public void PartCollisionOccured (GrabbablePart part, GrabbablePart otherPart)
	{
		Debug.Log ("SimulationFailed");
//		gameState = State.SimulationFailed;
	}
}









