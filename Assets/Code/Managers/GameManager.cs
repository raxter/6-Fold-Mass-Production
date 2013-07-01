using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameManager : SingletonBehaviour<GameManager> 
{
	
	
	Dictionary<MechanismType, Mechanism> cellMechanismPrefabs;
	
	
	
	LevelSettings.Level _currentLevel = null;
	public LevelSettings.Level currentLevel
	{
		get
		{
			return _currentLevel;
		}
	}
	
	int _completedConstructions = -1;
	public int completedConstructions
	{
		get { return _completedConstructions; }
		set 
		{ 
			_completedConstructions = value; 
			if (ConstructionCompletedEvent != null)
			{
				ConstructionCompletedEvent();
			}
		}
	}
	public delegate void EventFunction();
	
	public event EventFunction ConstructionCompletedEvent;
	
	public event EventFunction InstructionStartedEvent;
	
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
	
//	public bool guiEnabled { get; private set; }
	
	public event EventFunction GameStateChangedEvent;
	
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
		if (GameStateChangedEvent != null)
		{
			GameStateChangedEvent();
		}
	}
	
	State _gameState;
	
	public string loadLevelOnStart = "";
	
	IEnumerator Start()
	{
		yield return null;
		
		LoadLevel(loadLevelOnStart);
		
		gameState = State.Construction;
		cellMechanismPrefabs = new Dictionary<MechanismType, Mechanism>();
		
		cellMechanismPrefabs.Add(MechanismType.None, null);
		
		foreach (Mechanism mechanismPrefab in GameSettings.instance.mechanismPrefabs)
		{
			cellMechanismPrefabs.Add(mechanismPrefab.MechanismType, mechanismPrefab);
		}
		StopSimulation();
		
		
		GridManager.instance.LoadLayout();
	}

	void LoadLevel (string leveName)
	{
		// clear all parts first when reloading! TODO
		
		_currentLevel = LevelSettings.instance.GetLevel(leveName);
		
		GridManager.instance.SetTarget(_currentLevel.targetConstruction);
		
//		foreach (LevelSettings.GeneratorDetails generatorDetails in _currentLevel.generators)
//		{
////			PartGenerator generator = (GameObject.Instantiate(GameSettings.instance.generatorPrefab.gameObject) as GameObject).GetComponent<PartGenerator>();
//			
//			generator.toGenerateConstruction = Construction.Decode(generatorDetails.toGenerate, (prefab) => Instantiate(prefab) as GameObject);
//			generator.toGenerateConstruction.ignoreCollisions = true;
//			generator.PlaceAtLocation(generatorDetails.location);
//		}
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
	
	public Mechanism InstantiateMechanism(MechanismType cellMechanismType)
	{
		Mechanism toReturn = ObjectPoolManager.GetObject(cellMechanismPrefabs[cellMechanismType]);
		if (toReturn is PartGenerator)
		{
			(toReturn as PartGenerator).toGenerateConstruction = Construction.CreateSimpleConstruction(PartType.Standard6Sided); // if it is a generator, we give it a default thing to generate
		}
		
		return toReturn;
	}
	
	public void CreateMechanismForDragging(MechanismType cellMechanismType)
	{
		if (cellMechanismType == MechanismType.None)
			return;
		
		Mechanism unplacedMechanism = InstantiateMechanism(cellMechanismType);
		
		InputManager.instance.StartDraggingUnplaced(unplacedMechanism);
		
	}
	
	
	HashSet<GrabbablePart> parts = new HashSet<GrabbablePart>();
	int stepsToDoThisFrame = 0;
	
	IEnumerator SimulationCoroutine()
	{
		float startFrameTime = 0;
		
		int programCounter = -1;
		List<HexCellPlaceable> placeables = new List<HexCellPlaceable>();
		List<Grabber> grabbers = new List<Grabber>();
		List<PartGenerator> generators = new List<PartGenerator>();
		HashSet<WeldingRig> welders = new HashSet<WeldingRig>();
		
		
		List<HexCell> finishCells = new List<HexCell>();
		
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
			if (hexCell.finishCell)
			{
				finishCells.Add(hexCell);
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
		
		parts.Clear();
		
		while (true)
		{
			
			
			/** TODO update this
			 * Description of events in an instruction loop:
			 * 
			 * Preamble: Each instruction is split into a number of steps, simulation continuously performs instructions until win or failure
			 * 
			 * PreStart 1:
			 * Generators generates a Part if there is space (stores this part)
			 * 
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
			 * Welders check if there are any Parts over it (dropped or held) and welds them together
			 * 
			 * PostStart 2:
			 * Grabber performs Grab instruction if it has one (picks up whatever Part on the Cell under the Clamp)
			 * 
			 * PostStart 3:
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
//			Debug.Log("Start Instruction");
			
			if (InstructionStartedEvent != null)
			{
				InstructionStartedEvent();
			}
			
//			parts.RemoveWhere((obj) => obj == null);
			foreach(GrabbablePart part in parts)
			{
				if (part != null)
				{
					part.RegisterLocationFromPosition();
				}
			}
			
//			Debug.Log("-------------------");
			foreach (PartGenerator generator in generators)
			{
				Construction newConstruction = generator.StepPreStart();
				if (newConstruction)
				{
					foreach (GrabbablePart newPart in newConstruction.Parts)
					{
						parts.Add(newPart);
						newPart.RegisterLocationFromPosition(); // must perform this second registration step here because generators must know when there is a part above/on it
						
					}
				}
			}
			
//			Debug.Log ("Welder post start");
			foreach (WeldingRig welder in welders)
			{
				welder.PerformPostStart();
//				Debug.Log("PerformPostInstruction "+grabber._instructionCounter);
			}
			
			// perform instruction
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformInstruction();
//				Debug.Log("PerformInstruction "+grabber._instructionCounter);
			}
			
//			// TODO check for multi grabs here ??
//			if (GameManager.instance.gameState == GameManager.State.Simulation)
//			{
//				if (heldAndMovingGrabber != null && otherConstruction.heldAndMovingGrabber != null)
//				{
//					Debug.LogWarning("Multiple grab, two parts welded together while held", this);
//					GameManager.instance.MultipleGrabOccured(heldAndMovingGrabber, otherConstruction.heldAndMovingGrabber);
//				}
//			}
			
			foreach (Grabber grabber in grabbers)
			{
				grabber.PerformPostInstruction();
//				Debug.Log("PerformPostInstruction "+grabber._instructionCounter);
			}
			
			
			// inbetween 'steps'
			
			if (_currentSpeed != SimulationSpeed.Fastest && _currentSpeed != SimulationSpeed.Paused)
			{
				yield return new WaitForSeconds(0.2f/_instructionsPerSecond);
			}
			
			
//			Debug.Log ("Checking " +finishCells.Count+ " finish cells");
			HashSet<GrabbablePart> partsOverFinishCell = new HashSet<GrabbablePart>();
			foreach (HexCell finishCell in finishCells)
			{
				GrabbablePart partOverCell = finishCell.partOverCell;
				if (partOverCell != null)
				{
					partsOverFinishCell.Add(partOverCell);
				}
			}
//			Debug.Log ("Found " +partsOverFinishCell.Count+ " parts over finish cells");
			
			while (partsOverFinishCell.Count > 0)
			{
//				Debug.Log ("Checking " +partsOverFinishCell.Count+ " parts");
				// get a part from the ones over the finish cells
				
//				GrabbablePart partOverFinish = null;
//				foreach(GrabbablePart firstPart in partsOverFinishCell) 
//				{
//					partOverFinish = firstPart;
//					break;
//				}
				
				GrabbablePart partOverFinish = partsOverFinishCell.First();
				
				Construction constructionOverFinish = partOverFinish.ParentConstruction;
//				Debug.Log ("Checking Part "+partOverFinish.name);
//				Debug.Log ("Checking Construction "+constructionOverFinish);
				bool allPartsOverFinish = true;
				foreach(GrabbablePart constructionPart in constructionOverFinish.Parts)
				{
					if (!partsOverFinishCell.Contains(constructionPart))
					{
						partsOverFinishCell.ExceptWith(constructionOverFinish.Parts);
						allPartsOverFinish = false;
						break;
					}
				}
				if (!allPartsOverFinish)
				{
					continue;
				}
				
				// and if they are the corrects construction
				bool correctConstruction = false;
				
				// only check this if it is not held by a grabber
				
				
				if (GrabberManager.instance.GetGrabbersHoldingCount(constructionOverFinish) == 0)
				{
					Debug.Log("Checking Construction "+constructionOverFinish.name, constructionOverFinish);
					
					Construction.CompareCode errorCode = (Construction.CompareCode)GridManager.instance.target.CompareTo(constructionOverFinish);
					Debug.Log("Error Code: "+errorCode);
					correctConstruction = errorCode == Construction.CompareCode.Equal;
					
				}
				else
				{
					Debug.Log("Ignoreing held constructions");
				}
				// in either case, these construction parts have been checked, remove them from the list
				// won't be destroyed until end of frame, phew <- omg pooling objects means that they were :/
//				Debug.Log("Before Count "+partsOverFinishCell.Count);
				partsOverFinishCell.ExceptWith(constructionOverFinish.Parts);
//				Debug.Log("After Count "+partsOverFinishCell.Count);
				
				if (correctConstruction)
				{
					completedConstructions += 1;
//					Debug.Log ("removing " +partOverFinish.name+ "\'s connected parts ("+constructionOverFinish.name+")");
				
//					foreach (GrabbablePart toRemove in construction.Parts)
//					{
//						GameObject.Destroy(toRemove.gameObject);
//					}
					foreach (GrabbablePart toRemove in constructionOverFinish.Parts)
					{
						parts.Remove(toRemove);
					}
					ObjectPoolManager.DestroyObject(constructionOverFinish);
				}
				
			}
//			Debug.Log("Done checking finished constructions");
			
			
			foreach(HexCell hc in GridManager.instance.GetAllCells())
			{
				hc.DeregisterPart();
			}
//			Debug.Log ("Instruction");
			int noGrabberSteps = 60;
			// perform steps
			while (true)
			{
//				Debug.Log ("Step "+noGrabberSteps);
				
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
//					Debug.Log ("Simulation ending");
					
					
					foreach (HexCell hc in GridManager.instance.GetAllCells())
					{
						hc.SetDebugText();
						
					}
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
				
				if (gameState != State.Simulation)
				{
					yield return null;
					continue;
				}
				
				stepsToDoThisFrame -= 1;
				
				
				bool allFinished = true;
				
				foreach (Grabber grabber in grabbers)
				{
//					Debug.Log("Grabber Construction ", grabber.heldPart);
					if (!grabber.PerformStep())
					{
						allFinished = false;
					}
//					Debug.Log("PerformStep "+grabber._stepCounter);
				}
				
				if (grabbers.Count == 0)
				{
					noGrabberSteps -= 1;
				}
				if (noGrabberSteps <= 0)
				{
					allFinished = true;
				}
				
				
				// check for collisions
			
				// if collision, pause and exit
				
				foreach (GrabbablePart part in parts)
				{
					GrabbablePart other = part.CheckForCollisions();
					if (other != null)
					{
						PartCollisionOccured(part, other);
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
	
	
	public void PartCollisionOccured (GrabbablePart part, GrabbablePart otherPart)
	{
		Debug.Log ("SimulationFailed");
		part.highlighted = true;
		otherPart.highlighted = true;
		gameState = State.SimulationFailed;
	}
	
	public void MultipleGrabOccured (Grabber grabber, Grabber otherGrabber)
	{
		grabber.selected = true;
		otherGrabber.selected = true;
		GrabberManager.instance.GetPartHeldBy(grabber).highlighted = true;
		GrabberManager.instance.GetPartHeldBy(otherGrabber).highlighted = true;
		gameState = State.SimulationFailed;
	}
	
	public enum SimulationSpeed {Stopped, Paused, Normal, Fast, Faster, Fastest};
	
	public SimulationSpeed currentSpeed
	{
		get
		{
			return _currentSpeed;
		}
		private set
		{
			_currentSpeed = value;
			if (SimulationSpeedChangedEvent != null)
			{
				SimulationSpeedChangedEvent();
			}
		}
	}
	SimulationSpeed _currentSpeed = SimulationSpeed.Stopped;
	
	public event EventFunction SimulationSpeedChangedEvent;
	
	public void PlaySimulationPaused()
	{
		PlaySimulation(SimulationSpeed.Paused);
	}
	
	public void PlaySimulationNormal()
	{
		PlaySimulation(SimulationSpeed.Normal);
	}
	
	public void PlaySimulationFast()
	{
		PlaySimulation(SimulationSpeed.Fast);
	}
	
	public void PlaySimulationFaster()
	{
		PlaySimulation(SimulationSpeed.Faster);
	}
	
	public void PlaySimulationFastest()
	{
		PlaySimulation(SimulationSpeed.Fastest);
	}
	
	
	public void PlaySimulation(SimulationSpeed newSimulationSpeed)
	{
		currentSpeed = newSimulationSpeed;
		switch (currentSpeed)
		{
		case SimulationSpeed.Paused:
			_instructionsPerSecond = 0;
			break;
		case SimulationSpeed.Normal:
			_instructionsPerSecond = instructionsPerSecondNormal;
			break;
		case SimulationSpeed.Fast:
			_instructionsPerSecond = instructionsPerSecondFast;
			break;
		case SimulationSpeed.Faster:
			_instructionsPerSecond = instructionsPerSecondFaster;
			break;
		case SimulationSpeed.Fastest:
			_instructionsPerSecond = instructionsPerSecondFastest;
			break;
		}
		
		Debug.Log (gameState);
		if (gameState != State.Construction)
		{
			return;
		}
		
		Debug.Log ("PlaySimulation");
		
		gameState = State.Simulation;
		
		StartCoroutine(SimulationCoroutine());
		
		
	}
	
	
	public void StopSimulation()
	{
		currentSpeed = SimulationSpeed.Stopped;
		
		gameState = State.Construction;
		
		Debug.Log ("StopSimulation");
		// Reenable Input
		
		
		foreach (GrabbablePart part in parts)
		{
			if (part != null)
			{
				ObjectPoolManager.DestroyObject(part.ParentConstruction);
			}
		}
		parts.Clear();
		
		
		completedConstructions = 0;
	}
	
	
}









