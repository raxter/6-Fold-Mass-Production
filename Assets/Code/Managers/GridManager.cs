using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class HexCellRow 
{ 
	public HexCellRow (int length)
	{
		row = new HexCell [length];
	}
	public HexCell [] row;
	
	public HexCell this [int i]
	{
		get { return row[i]; }
		set { row[i] = value; }
	}
	
	public IEnumerator GetEnumerator()
	{
		return row.GetEnumerator();
	}
	
	public int Length
	{
		get { return row.Length; }
	}
}

public class GridManager : SingletonBehaviour<GridManager> 
{
	
#if UNITY_EDITOR
	
	[MenuItem("Custom/Clear Editor Save")]
	static void ClearEditorSave()
	{
		LevelDataManager.DeleteAll();
		
	}
	
#endif
	
	[SerializeField]
	GameObject _hexCellHolder;
	
	
	[SerializeField]
	GameObject _targetHolder;
	
	[SerializeField]
	HexCellRow [] _hexCellRows;
	
	Dictionary<IntVector2, HexCell> _hexCellLocations;
	
	public int targetConstructions { get; private set; }
	
	public HexCell GetHexCell(IntVector2 location)
	{
		
		if (_hexCellLocations == null)
			BuildDictionary();
		
		if (_hexCellLocations.ContainsKey(location))
			return _hexCellLocations[location];
		
		return null;
	}
	
	public Construction target { get; private set; }
	
	public void SetTarget(Construction targetConstruction)
	{
		if (target != null)
		{
			Debug.Log ("Destroying pooled "+target.name);
			target.DestroySelf();
			target = null;
		}
		Debug.Log ("Setting Target "+Encoding.Encode(targetConstruction));
		target = Construction.DecodeCopy(targetConstruction);
		Debug.Log ("Set Target "+Encoding.Encode(target));
		target.ignoreCollisions = true;
//		GrabbablePart targetPart = target.GenerateConnectedParts();
		target.transform.parent = _targetHolder.transform;
		target.transform.localPosition = Vector3.zero;
		
		GridChangedEvent();
	}
	
	void Start()
	{
		SetTarget(Construction.CreateSimpleConstruction(PartType.None));
		GridChangedEvent += PerformAutosave;
	}
	
	public event System.Action GridChangedEvent;
	
	public void PerformAutosave()
	{
		Debug.Log ("Autosaveing");
		// put autosave system here
		
		if (LevelEditorGUI.hasActiveInstance)
		{
			// save editor level if editor is enabled
			SaveType saveType = LevelEditorGUI.instance.EditorEnabled ? SaveType.Level : SaveType.Solution;
			string levelEncoding = LevelEncoding();
			
			LevelDataManager.instance.Save(LevelDataManager.EditorSaveName, LevelEncoding(), SaveType.Level, AutoSaveType.AutoSave);
			
			LevelDataManager.instance.Save(LoadedLevelName, LevelEncoding(), SaveType.Level, AutoSaveType.AutoSave);
				
		}
		
		// save autosave layout for level (reference by level name? AUTOSAVE_<lvlname>)
		
	}

	
	public void SaveCurrentLevelAs(string levelName)
	{
		LevelDataManager.instance.Save(levelName, LevelEncoding(), SaveType.Level, AutoSaveType.Named);
	}
	
	public string LoadedLevelName { get; private set;}
	
	
	public void LoadEditorSolution()
	{
		LevelDataManager.instance.Save(LevelDataManager.EditorSaveName, LevelEncoding(), SaveType.Level, AutoSaveType.AutoSave);
		
		string encodedSolution = LevelDataManager.instance.Load(LevelDataManager.EditorSaveName, SaveType.Solution, AutoSaveType.AutoSave);
		
		ClearSolution();
		Encoding.Decode(new EncodableSolution(), encodedSolution);
	}
	public void LoadBlankEditor()
	{
		ClearSolution();
		ClearLevel();
		LoadedLevelName = LevelDataManager.EditorSaveName;
		GridManager.instance.target = Construction.CreateSimpleConstruction(PartType.None);
	}
	public void LoadEditorLevel()
	{
		LevelDataManager.instance.Save(LevelDataManager.EditorSaveName, SolutionEncoding(), SaveType.Solution, AutoSaveType.AutoSave);
		ClearSolution();
		string encodedLevel = LevelDataManager.instance.Load(LevelDataManager.EditorSaveName, SaveType.Level, AutoSaveType.AutoSave);
		if (encodedLevel != "")
		{
			Debug.Log ("Loading Editor "+encodedLevel);
			ClearLevel();
			Encoding.Decode(new EncodableLevel(), encodedLevel);
			
			LoadedLevelName = LevelDataManager.EditorSaveName;
		}
		else
		{
			LoadBlankEditor();
		}
	}
	
	
	public void LoadLevel(string levelName)
	{
		Debug.Log ("Loading "+levelName);
		LoadedLevelName = "";
		// save order -> n Generators : Target1 (: Target2)
		string encodedLevel = LevelDataManager.instance.Load(levelName, SaveType.Level, AutoSaveType.Named);
		if (encodedLevel != "")
		{
			Debug.Log ("Loading "+encodedLevel);
			ClearLevel();
			Encoding.Decode(new EncodableLevel(), encodedLevel);
			
			LoadedLevelName = levelName;
		}
		return;
		
		
		// save layout for editor is editor is enabled
		
		// save autosave layout for level (reference by level name? AUTOSAVE_<lvlname>)
		
	}
	
	public void ClearSolution()
	{
		InputManager.instance.ClearSelection();
		// clear movable mechanisms
		foreach(HexCell hc in GridManager.instance.GetAllCells())
		{
			Mechanism mechanism = hc.placedMechanism;
			
			if (mechanism != null && mechanism.isSolutionMechanism)
			{
				hc.placedMechanism = null;
				ObjectPoolManager.DestroyObject(mechanism);
			}
		}
	}
	
	public void ClearLevel()
	{
		// clear targets? (they get overridden if necessary)
		// clear all hexcellplacables
		foreach(HexCell hc in GridManager.instance.GetAllCells())
		{
			HexCellPlaceable placeable = hc.placedPlaceable;
			
			if (placeable != null)
			{
				hc.placedPlaceable = null;
				ObjectPoolManager.DestroyObject(placeable);
			}
		}
	}
	
	public string LevelEncoding()
	{
		return Encoding.Encode(new EncodableLevel());
	}
	
	public string SolutionEncoding()
	{
		return Encoding.Encode(new EncodableSolution());
	}
	class EncodableSolution : IEncodable
	{
		public IEnumerable<IEncodable> Encode ()
		{
			List<PartGenerator> movableGenerators = new List<PartGenerator>();
			List<IEncodable> grabbers = new List<IEncodable>();
			List<IEncodable> weldingRigs = new List<IEncodable>();
			
			foreach(HexCell hc in GridManager.instance.GetAllCells())
			{
				PartGenerator generator = hc.placedMechanism as PartGenerator;
				Grabber grabber = hc.placedMechanism as Grabber;
				WeldingRig weldingRig = hc.placedMechanism as WeldingRig;
				
				if (generator != null && generator.isSolutionMechanism)
					movableGenerators.Add (generator);
				
				if (grabber != null)
					grabbers.Add (grabber);
				
				if (weldingRig != null)
					weldingRigs.Add (weldingRig);
				
			}
			
			
			yield return (EncodableInt)movableGenerators.Count;
			yield return new EncodableSubGroup(movableGenerators.ConvertAll<IEncodable>(
				(movableGenerator) => new EncodableSubGroup(new List<IEncodable>()
				{
					(EncodableInt)movableGenerator.Location.x,
					(EncodableInt)movableGenerator.Location.y,
					movableGenerator.toGenerateConstruction,
				} )
			) );
			
			
			foreach (List<IEncodable> list in new List<IEncodable> [] {grabbers, weldingRigs})
			{
				yield return (EncodableInt)list.Count;
				yield return new EncodableSubGroup(list);
			}
			
		}
		
		

		public bool Decode (Encoding encodings)
		{
			Debug.Log ("EncodableSolution Encoding Data\n"+ encodings.DebugString());
			
			// find moveable grabbers in the level that match the save file
			List<PartGenerator> generators = new List<PartGenerator>();
			foreach(HexCell hc in GridManager.instance.GetAllCells())
			{
				PartGenerator generator = hc.placedMechanism as PartGenerator;
				if (generator != null && generator.isSolutionMechanism)
					generators.Add (generator);
				
			}
			PartGenerator [] movableGenerators = new PartGenerator [encodings.Int(0)];
			for (int i = 0 ; i < movableGenerators.Length ; i++)
			{
				Encoding movableGeneratorEncoding = encodings.SubEncoding(1).SubEncoding(i);
				
				PartGenerator moveableGenerator = generators.Find((con) => con.toGenerateConstruction.CompareTo(movableGeneratorEncoding.SubEncoding(2)) == 0);
				
				if (moveableGenerator != null)
				{
					moveableGenerator.PlaceAtLocation(new IntVector2(movableGeneratorEncoding.Int (0), movableGeneratorEncoding.Int (1)));
				}
				else
					Debug.LogWarning("Couldn't find movable generator that was saved");
				
			}
			
			int index = 2;
			foreach (MechanismType type in new MechanismType [] {MechanismType.Grabber, MechanismType.WeldingRig})
			{
				if (index >= encodings.Count)
				{
					break;
				}
				
				Mechanism [] mechanisms = new Mechanism [encodings.Int (index)];
				index++;
				
				Encoding mechanismEncodingGroup = encodings.SubEncoding(index);
				index++;
				
				for (int i = 0 ; i < mechanisms.Length ; i++)
				{
					mechanisms[i] = ObjectPoolManager.GetObject<Mechanism>(GameSettings.instance.GetMechanism(type));
					mechanisms[i].Decode(mechanismEncodingGroup.SubEncoding(i));
				}
			}
			
			
			return true;
		}
	}
	
	class EncodableLevel : IEncodable
	{
		public IEnumerable<IEncodable> Encode ()
		{
			Dictionary<MechanismType, List<IEncodable>> immovableParts = new Dictionary<MechanismType, List<IEncodable>>();
			MechanismType [] mechanismTypes = new MechanismType [] {MechanismType.Generator, MechanismType.Grabber, MechanismType.WeldingRig};
			foreach (MechanismType type in mechanismTypes)
				immovableParts[type] = new List<IEncodable>();
				
			
			List<IEncodable> generators = new List<IEncodable>();
			foreach(HexCell hc in GridManager.instance.GetAllCells())
			{
//				PartGenerator generator = hc.placedMechanism as PartGenerator;
//				
//				if (generator != null)
//				{
//					generators.Add (generator);
//				}
				
				Mechanism movableMechanism = hc.placedMechanism;
				
				if (movableMechanism != null && !movableMechanism.isSolutionMechanism)
				{
					immovableParts[movableMechanism.MechanismType].Add(movableMechanism);
				}
			}
			
			foreach (MechanismType type in mechanismTypes)
			{
				yield return (EncodableInt)immovableParts[type].Count;
				foreach (IEncodable immoveablePart in immovableParts[type])
					yield return immoveablePart;
			}
			yield return (EncodableInt)10; // target # 1
			yield return (EncodableInt)0;  // target # 2
			
			yield return GridManager.instance.target; // target 1
			yield return (EncodableInt)0;             // target 2
		}

		public bool Decode (Encoding encodings)
		{
			Debug.Log ("EncodableLevel Encoding Data\n"+ encodings.DebugString());
//			int count = encodings.Int(0);
//			for (int i = 0 ; i < count ; i++)
//			{
//				PartGenerator generator = ObjectPoolManager.GetObject(GameSettings.instance.partGeneratorPrefab);
//				generator.Decode(encodings.SubEncoding(1+i));
//			}
			int i = 0;
			MechanismType [] mechanismTypes = new MechanismType [] {MechanismType.Generator, MechanismType.Grabber, MechanismType.WeldingRig};
			foreach (MechanismType type in mechanismTypes)
			{
				int count = encodings.Int(i);
				i++;
				for (int j = 0 ; j < count ; j++)
				{
					Mechanism newMechanism = ObjectPoolManager.GetObject<Mechanism>(GameSettings.instance.GetMechanism(type));
					newMechanism.Decode(encodings.SubEncoding(i));
					i++;
				}
			}
			
			GridManager.instance.targetConstructions = encodings.Int(i+0);
//			GridManager.instance.targetConstructions2 = encodings.Int(i+1);
			Construction target0 = Construction.DecodeCreate(encodings.SubEncoding(i+2));
			Debug.Log ("Setting Target0 "+encodings.SubEncoding(i+2).DebugString());
			Debug.Log ("Setting Target0 "+Encoding.Encode(target0));
			GridManager.instance.SetTarget(target0);
			target0.DestroySelf();
//			Construction target0 = Construction.DecodeCreate(encodings.SubEncoding(i+3));
//			CoroutineUtils.WaitOneFrameAndDo(() => GridManager.instance.SetTarget(target0));
			
			return true;
		}
	}
	
	public void RegisterMechanismChange ()
	{
		if (GridChangedEvent != null)
			GridChangedEvent();
	}
	
	
	public void DestroyHexCellMap()
	{
		if (_hexCellRows == null)
		{
			_hexCellLocations = null;
			return;
		}
		
		foreach (HexCellRow hexCellRow in _hexCellRows)
		{
			foreach (HexCell hexCell in hexCellRow)
			{
				if (hexCell && hexCell.gameObject)
				{
					ObjectPoolManager.DestroyObject(hexCell);
				}
			}
		}
		_hexCellRows = null;
		_hexCellLocations = null;
	}
	
	class IntVector2Comparer : IEqualityComparer<IntVector2>
	{
		public bool Equals(IntVector2 a, IntVector2 b)
	    {
	        return a.x == b.x && a.y == b.y;
	    }


	    public int GetHashCode(IntVector2 a)
	    {
	        int hCode = a.x ^ a.y;
	        return hCode.GetHashCode();
	    }	
	};
	
	public IEnumerable<HexCell> GetAllCells()
	{
		if (_hexCellRows == null)
			yield break;
		
		foreach (HexCellRow hexCellRow in _hexCellRows)
		{
			foreach(HexCell hexCell in hexCellRow.row)
			{
				yield return hexCell;
			}
		}
	}
	
	public IEnumerable<HexCell> GetOutsideCells()
	{
		if (_hexCellRows == null)
			yield break;
		
		foreach (HexCell hexCell in _hexCellRows[0])
		{
			yield return hexCell;
		}
		foreach (HexCellRow hexCellRow in _hexCellRows)
		{
			yield return hexCellRow[0];
			yield return hexCellRow[hexCellRow.Length-1];
		}
		foreach (HexCell hexCell in _hexCellRows[_hexCellRows.Length-1])
		{
			yield return hexCell;
		}
	}
	
	public void BuildDictionary()
	{
		
		_hexCellLocations = new Dictionary<IntVector2, HexCell>(new IntVector2Comparer());
		
		foreach (HexCellRow hexCellRow in _hexCellRows)
		{
			foreach (HexCell hexCell in hexCellRow.row)
			{
				_hexCellLocations[hexCell.location] = hexCell;
			}
		}
	}
	
	public void CreateHexCellMap()
	{
		GameSettings gameSettings = GameSettings.instance;
		DestroyHexCellMap();
		_hexCellRows = new HexCellRow [gameSettings.gridWidth];
		
		for (int w = 0 ; w < gameSettings.gridWidth ; w++)
		{
			_hexCellRows[w] = new HexCellRow(gameSettings.gridHeight);
			for (int h = 0 ; h < gameSettings.gridHeight/*_hexCellRows[w].row.Length*/ ; h++)
			{
				
				HexCell hexCell = ObjectPoolManager.GetObject(gameSettings.hexCellPrefab);
				
				hexCell.location = new IntVector2(w, h-w/2);
				
				hexCell.name += ""+hexCell.location.x+":"+hexCell.location.y;
				hexCell.transform.parent = _hexCellHolder.transform;
				
				hexCell.transform.localPosition = hexCell.GetDirection(HexMetrics.UpDirection)*hexCell.location.y 
												+ hexCell.GetDirection(HexMetrics.RightDirection)*hexCell.location.x;
				
				hexCell.gameObject.layer = _hexCellHolder.layer;
				_hexCellRows[w][h] = hexCell;
				
				if (w >= gameSettings.gridFinalCellsFromWidth && h > gameSettings.gridFinalCellsFromHeight)
				{
					Debug.Log ("Finish cell "+w+"/"+gameSettings.gridFinalCellsFromWidth+":"+h+"/"+gameSettings.gridFinalCellsFromHeight);
					_hexCellRows[w][h].finishCell = true;
				}
			}
		}
		
		BuildDictionary();
	}
	
	
}













