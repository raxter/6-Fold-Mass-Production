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
	
	public HexCell GetHexCell(IntVector2 location)
	{
		
		if (_hexCellLocations == null)
			BuildDictionary();
		
		if (_hexCellLocations.ContainsKey(location))
			return _hexCellLocations[location];
		
		return null;
	}
	
	public Construction target { get; private set; }
	
	public void SetTarget(string encodedTarget)
	{
		target = Construction.Decode(encodedTarget);
		target.ignoreCollisions = true;
//		GrabbablePart targetPart = target.GenerateConnectedParts();
		target.transform.parent = _targetHolder.transform;
		target.transform.localPosition = Vector3.zero;
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
	
	public void SaveLayout()
	{
		HashSet<Mechanism> savedMechanisms = new HashSet<Mechanism>();
		List<string> encodings = new List<string>();
		foreach(HexCell hc in GetAllCells())
		{
			Mechanism mech = hc.placedMechanism;
			if (mech != null && !savedMechanisms.Contains(mech))
			{
				encodings.Add(GetMechanismCode(mech.MechanismType)+";"+mech.Location.x+";"+mech.Location.y+";"+mech.Encode());
				
				savedMechanisms.Add(mech);
			}
		}
		
		string saveString = string.Join(":", encodings.ToArray());
//		Debug.Log("Saveing: "+saveString);
		
		PlayerPrefs.SetString("save string", saveString);
		PlayerPrefs.Save();
	}
	
	public void LoadLayout()
	{
		string saveString = PlayerPrefs.GetString("save string");
		if (saveString == "")
		{
			return;
		}
		
		Debug.Log("Loading: "+saveString);
		
		string [] mechanismCodes = saveString.Split(':');
		
		foreach (string code in mechanismCodes)
		{
			MechanismType codeType = GetMechanismType(code[0]);
			
			string [] codeData = code.Split(';');
			
			Mechanism newMechanism = GameManager.instance.InstantiateMechanism(codeType);
			newMechanism.Decode(codeData[3]);
			newMechanism.PlaceAtLocation(new IntVector2(int.Parse(codeData[1]), int.Parse(codeData[2])));
		}
	}
	
}













