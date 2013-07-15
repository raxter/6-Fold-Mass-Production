using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		target = Construction.DecodeCopy(targetConstruction);
		target.ignoreCollisions = true;
//		GrabbablePart targetPart = target.GenerateConnectedParts();
		target.transform.parent = _targetHolder.transform;
		target.transform.localPosition = Vector3.zero;
	}
	
	void Start()
	{
		MechanismChangedEvent += PerformAutosave;
	}
	
	public event System.Action MechanismChangedEvent;
	
	public void PerformAutosave()
	{
		Debug.Log ("Autosaveing");
		// put autosave system here
		
		// save editor level if editor is enabled
		if (LevelEditorGUI.instance.editorEnabled)
		{
			// save order -> n Generators : Target1 (: Target2)
			LevelDataManager.instance.Save(LevelDataManager.editorSaveName, LevelEncoding, SaveType.Level);
			// TODO save grabbers/welders etc
		}
		
		if (LevelEditorGUI.instance.editorEnabled)
		{
		
			// save layout for editor is editor is enabled
		}
		
		// save autosave layout for level (reference by level name? AUTOSAVE_<lvlname>)
		
	}
	
	
	public void SaveCurrentLevelAs(string levelName)
	{
		LevelDataManager.instance.Save(levelName, LevelEncoding, SaveType.Level);
	}
	
	
	public void LoadEditor()
	{
		string encodedLevel = LevelDataManager.instance.Load(LevelDataManager.editorSaveName, SaveType.Level);
		if (encodedLevel != "")
		{
			Debug.Log ("Loading "+encodedLevel);
			Encoding.Decode(new EncodableLevel(), encodedLevel);
		}
		else
		{
			GridManager.instance.target = Construction.CreateSimpleConstruction(PartType.None);
		}
	}
	
	public void LoadLevel(string levelName)
	{
		Debug.Log ("Loading "+levelName);
		
		// save order -> n Generators : Target1 (: Target2)
		string encodedLevel = LevelDataManager.instance.Load(levelName, SaveType.Level);
		if (encodedLevel != "")
		{
			Debug.Log ("Loading "+encodedLevel);
			Encoding.Decode(new EncodableLevel(), encodedLevel);
		}
		return;
		
		
		// save layout for editor is editor is enabled
		
		// save autosave layout for level (reference by level name? AUTOSAVE_<lvlname>)
		
	}
	
	public string LevelEncoding
	{
		get
		{
			return Encoding.Encode(new EncodableLevel());
		}
	}
	
	public string SolutionEncoding
	{
		get
		{
			return Encoding.Encode(new EncodableLevelSolution());
		}
	}
	class EncodableLevelSolution : IEncodable
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
				
				if (generator != null && generator.movable)
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
			// find moveable grabbers in the level that match the save file
			List<PartGenerator> generators = new List<PartGenerator>();
			foreach(HexCell hc in GridManager.instance.GetAllCells())
			{
				PartGenerator generator = hc.placedMechanism as PartGenerator;
				if (generator != null && generator.movable)
					generators.Add (generator);
				
			}
			PartGenerator [] movableGenerators = new PartGenerator [encodings.Int(0)];
			for (int i = 0 ; i < movableGenerators.Length ; i++)
			{
				Encoding movableGeneratorEncoding = encodings.SubEncoding(1).SubEncoding(i);
				Construction movableGeneratorConstruction = Construction.DecodeCreate(movableGeneratorEncoding.SubEncoding(2));
				
				PartGenerator moveableGenerator = generators.Find((con) => con.toGenerateConstruction.CompareTo(movableGeneratorConstruction) == 0);
				
				if (moveableGenerator != null)
				{
					moveableGenerator.PlaceAtLocation(new IntVector2(movableGeneratorEncoding.Int (0), movableGeneratorEncoding.Int (1)));
				}
				else
					Debug.LogWarning("Couldn't find movable generator that was saved");
				
				movableGeneratorConstruction.DestroySelf();
			}
			
			// TODO
			// place grabbers
			
			// TODO
			// place welding rigs
			
			return true;
		}
	}
	
	class EncodableLevel : IEncodable
	{
		public IEnumerable<IEncodable> Encode ()
		{
			
			List<IEncodable> generators = new List<IEncodable>();
			foreach(HexCell hc in GridManager.instance.GetAllCells())
			{
				PartGenerator generator = hc.placedMechanism as PartGenerator;
				
				if (generator != null)
				{
					generators.Add (generator);
				}
			}
			
			yield return (EncodableInt)generators.Count;
			foreach (IEncodable generator in generators)
				yield return generator;
			
			yield return (EncodableInt)10; // target # 1
			yield return (EncodableInt)10; // target # 2
			
			yield return GridManager.instance.target; // target 1
			yield return (EncodableInt)0;// target 2
		}

		public bool Decode (Encoding encodings)
		{
			Debug.Log ("GridManager Encoding Data\n"+ encodings.DebugString());
			int count = encodings.Int(0);
			for (int i = 0 ; i < count ; i++)
			{
				PartGenerator generator = ObjectPoolManager.GetObject(GameSettings.instance.partGeneratorPrefab);
				generator.Decode(encodings.SubEncoding(1+i));
			}
			
//			targetConstructions = encodings.Int(1);
			GridManager.instance.targetConstructions = encodings.Int(1+count);
//			GridManager.instance.targetConstructions2 = encodings.Int(1+count+1);
			GridManager.instance.SetTarget(Construction.DecodeCreate(encodings.SubEncoding(1+count+3)));
//			GridManager.instance.SetTarget2(encodings.SubEncoding(1+count+1));
			
			return true;
		}
	}
	
	public void RegisterMechanismChange ()
	{
		if (MechanismChangedEvent != null)
			MechanismChangedEvent();
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
	
	private char GetMechanismCode(MechanismType mechanismType)
	{
		return new Dictionary<MechanismType, char>()
		{
			{MechanismType.None, 	   '!'},
			{MechanismType.Grabber,    'G'},
			{MechanismType.WeldingRig, 'W'},
			{MechanismType.Generator,  'N'}
		}[mechanismType];
	
	}
	
	private MechanismType GetMechanismType(char c)
	{
		foreach(MechanismType type in System.Enum.GetValues(typeof(MechanismType)))
		{
			if (c == GetMechanismCode(type))
			{
				return type;
			}
		}
		
		return MechanismType.None;
	}
	
//	public void SaveLayout(string name)
//	{
//		HashSet<Mechanism> savedMechanisms = new HashSet<Mechanism>();
//		List<string> encodings = new List<string>();
//		foreach(HexCell hc in GetAllCells())
//		{
//			Mechanism mech = hc.placedMechanism;
//			if (mech != null && !savedMechanisms.Contains(mech))
//			{
//				encodings.Add(GetMechanismCode(mech.MechanismType)+";"+mech.Location.x+";"+mech.Location.y+";"+mech.Encode());
//				
//				savedMechanisms.Add(mech);
//			}
//		}
//		
//		string saveString = string.Join(":", encodings.ToArray());
////		Debug.Log("Saveing: "+saveString);
//		
//		PlayerPrefs.SetString("save string", saveString);
//		PlayerPrefs.Save();
//	}
	
//	public void LoadLayout()
//	{
//		string saveString = PlayerPrefs.GetString("save string");
//		if (saveString == "")
//		{
//			return;
//		}
//		
//		Debug.Log("Loading: "+saveString);
//		
//		string [] mechanismCodes = saveString.Split(':');
//		
//		foreach (string code in mechanismCodes)
//		{
//			MechanismType codeType = GetMechanismType(code[0]);
//			
//			string [] codeData = code.Split(';');
//			
//			Mechanism newMechanism = LevelManager.instance.InstantiateMechanism(codeType);
//			newMechanism.Decode(codeData[3]);
//			newMechanism.PlaceAtLocation(new IntVector2(int.Parse(codeData[1]), int.Parse(codeData[2])));
//		}
//	}
	
}













