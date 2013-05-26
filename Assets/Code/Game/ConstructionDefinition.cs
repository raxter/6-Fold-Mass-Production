using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionDefinitionAsset : ScriptableObject
{
	public ConstructionDefinition constructionDefinition;
}

[System.Serializable]
public class ConstructionDefinition : System.IComparable<ConstructionDefinition>, System.IComparable<GrabbablePart>
{
	[System.Serializable]
	public class ConstructionElement
	{
		public ConstructionElement()
		{
		}
		
		public PartType partType = PartType.None;
		
		public HexMetrics.Direction orientation = HexMetrics.Direction.Up;
		
		public int id = 0;
		
//		public ConstructionElement [] weldedInDirection = new ConstructionElement [6];
		public int [] connectedParts = new int [6];
		public GrabbablePart.PhysicalConnectionType [] physicalConnectionType = new GrabbablePart.PhysicalConnectionType [6];
		public int [] auxilaryConnectionType = new int [6];
		
		public string Encode()
		{
			string code = ""+CharSerializer.ToCode(id)+CharSerializer.ToCode((int)partType)+CharSerializer.ToCode((int)orientation);
			for (int i = 0 ; i < 6 ; i++)
			{
				code += ""+CharSerializer.ToCode(connectedParts[i])+CharSerializer.ToCode((int)physicalConnectionType[i])+CharSerializer.ToCode((int)auxilaryConnectionType[i]);
			}
			
			return code;
		}
		public static ConstructionElement Decode(string code)
		{
			ConstructionElement constructionElement = new ConstructionElement();
			constructionElement.id          =                       CharSerializer.ToNumber(code[0]);
			constructionElement.partType    =             (PartType)CharSerializer.ToNumber(code[1]);
			constructionElement.orientation = (HexMetrics.Direction)CharSerializer.ToNumber(code[2]);
			for (int i = 0 ; i < 6 ; i++)
			{
				constructionElement.connectedParts[i]         =                                       CharSerializer.ToNumber(code[3+(i*3)+0]);
				constructionElement.physicalConnectionType[i] = (GrabbablePart.PhysicalConnectionType)CharSerializer.ToNumber(code[3+(i*3)+1]);
				constructionElement.auxilaryConnectionType[i] =                                       CharSerializer.ToNumber(code[3+(i*3)+2]);
			}
			
			return constructionElement;
		}
	}
	
	
	private ConstructionDefinition (IEnumerable<ConstructionElement> elements)
	{
		constructionElements = new List<ConstructionElement>(elements);
	}
	
	public List<ConstructionElement> constructionElements = new List<ConstructionElement>();
	
	#region IComparable[ConstructionDefinition] implementation
	public int CompareTo (ConstructionDefinition other)
	{
		Debug.Log (constructionElements.Count+":"+other.constructionElements.Count);
		
		
		return 0;
	}
	#endregion

	#region IComparable[GrabbablePart] implementation
	public int CompareTo (GrabbablePart other)
	{
		return CompareTo(ToConstructionDefinition(other));
	}
	#endregion
	
//	public static void SaveToObject(string name)
//	{
//		ConstructionDefinition toSave = ScriptableObject.CreateInstance<ConstructionDefinition>();
//		toSave.constructionElements = constructionElements;
//		System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo("Assets/Data");
//		if(!directory.Exists)
//		{
//			directory.Create();
//		}
//		
//		AssetDatabase.CreateAsset(toSave, "Assets/Data/" + name + ".asset");
//		AssetDatabase.SaveAssets();
//	}
	
	public static ConstructionDefinition ToConstructionDefinition(GrabbablePart toCopyPart)
	{
		List<ConstructionElement> constructionElements = new List<ConstructionElement>();
		Dictionary<GrabbablePart, int> partID = new Dictionary<GrabbablePart, int>();
//		partID[null] = 0;
		
//		Debug.Log ("logging "+0);
		int idCount = 1;
		foreach (GrabbablePart part in toCopyPart.GetAllConnectedPartsFromRoot())
		{
//			Debug.Log ("logging "+idCount);
			partID[part] = idCount;
			idCount += 1;
		}
		
		foreach (GrabbablePart part in toCopyPart.GetAllConnectedPartsFromRoot())
		{
			ConstructionElement element = new ConstructionElement();
			element.id = partID[part];
//			Debug.Log("Defining "+element.id);
			element.partType = part.partType;
			element.orientation = part.SimulationOrientation;
			
			for(int i = 0 ; i < 6 ; i++)
			{
				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
				GrabbablePart connPart = part.GetConnectedPart(iDir);
				element.connectedParts[i] = connPart == null ? 0 : partID[connPart];
				element.physicalConnectionType[i] = part.GetPhysicalConnectionType(iDir);
				element.auxilaryConnectionType[i] = part.GetAuxilaryConnectionTypes(iDir);
				
				if (element.physicalConnectionType[i] == GrabbablePart.PhysicalConnectionType.None)
				{
					element.connectedParts[i] = 0;
					element.auxilaryConnectionType[i] = 0;
				}
			}
			constructionElements.Add(element);
		}
		
		
		return new ConstructionDefinition(constructionElements);
	}
	
	
	public GrabbablePart GenerateConnectedParts ()
	{
		Dictionary<int, GrabbablePart> parts = new Dictionary<int, GrabbablePart>();
		
		foreach(ConstructionElement element in constructionElements)
		{
			parts[element.id] = GameObject.Instantiate(GameSettings.instance.GetPartPrefab(element.partType)) as GrabbablePart;
			parts[element.id].transform.localPosition = Vector3.zero;
			
		}
		foreach(ConstructionElement element in constructionElements)
		{
			for (int i = 0 ; i < 6 ; i++)
			{
				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
				if (element.connectedParts[i] != 0)
				{
					parts[element.id].SimulationOrientation = element.orientation;
					parts[element.id].ConnectPartAndPlaceAtRelativeDirection(parts[element.connectedParts[i]], iDir);
					parts[element.id].SetPhysicalConnection(iDir, element.physicalConnectionType[i]);
					parts[element.id].SetAuxilaryConnections(iDir, element.auxilaryConnectionType[i]);
				}
			}
		}
		
		return parts[1];
	}
	
	
	
	//=========================================================================================
	#region Construction tree encoding and decoding
	public static ConstructionDefinition Decode(string encoded)
	{
		List<ConstructionElement> elements = new List<ConstructionElement>();
		List<string> encodedElements = new List<string>(encoded.Split(','));
		
		foreach(string elementCode in encodedElements)
		{
			elements.Add(ConstructionElement.Decode(elementCode));
		}
		
		return new ConstructionDefinition(elements);
		
	}
	public string Encode()
	{
		List<string> encodedElements = new List<string>();
		// <Type><Orientation>,<PhysicalConn0>,<AuxConnect0>,<Child0??_>,<PhysicalConn1>...
		foreach (ConstructionElement element in constructionElements)
		{
			encodedElements.Add(element.Encode());
		}
		
		return string.Join(",", encodedElements.ToArray());
		
	}
	
	#endregion
	
	
}
