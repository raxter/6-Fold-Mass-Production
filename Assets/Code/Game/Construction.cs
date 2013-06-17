using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate GameObject InstantiatePrefabDelegate (GameObject prefab);

public class Construction : MonoBehaviour, System.IComparable<Construction>
{

		//=========================================================================================
	
	
	public int idNumber = -1;
	
	public bool HasChild
	{
		get 
		{
			return FirstPart != null;
		}
	}
	
	public GrabbablePart FirstPart
	{
		get 
		{
			foreach (GrabbablePart part in Parts)
			{
				return part;
			}
			return null;
		}
	}
	
	public IEnumerable<GrabbablePart> Parts
	{
		get 
		{
//			for (int c = 0 ; c < transform.childCount ; c ++)
			foreach (Transform child in transform)
			{
//				Transform child = transform.GetChild(c);
				GrabbablePart childPart = child.gameObject.GetComponent<GrabbablePart>();
				if (childPart != null)
				{
					yield return childPart;
				}
			}
		}
	}
	
	public List<GrabbablePart> PartsList
	{
		get 
		{
			return new List<GrabbablePart> (Parts);
		}
	}
	
	public Grabber heldAndMovingGrabber { get; set; }
	
	
	public void AddToConstruction(GrabbablePart newPart)
	{
		Construction otherConstruction = newPart.ParentConstruction;
		
		if (otherConstruction != null && otherConstruction != this)
		{
			// move all otherConstruction's parts here
			
			foreach(GrabbablePart otherPart in otherConstruction.PartsList)
			{
				otherPart.transform.parent = this.transform;
//				Debug.Log("Transferring "+otherPart.idNumber+" from "+otherConstruction.idNumber+" to "+this.idNumber);
			}
//			Debug.Log (otherConstruction.HasChild);
			
			// delete old construction
			if (Application.isPlaying)
			{
				Destroy(otherConstruction.gameObject);
//				Debug.Log("Destroying construction "+otherConstruction.idNumber);
			}
#if UNITY_EDITOR
			else
			{
				Debug.Log("Deleting other construction "+otherConstruction.name);
				DestroyImmediate(otherConstruction.gameObject);
			}
#endif
		}
		else
		{
			newPart.transform.parent = this.transform;
			newPart.ConnectUnconnectedParts();
		}
		
	}
	
	
	public void RemoveFromConstruction(GrabbablePart partToRemove)
	{
		// remove part from construction
		
		// TODO split if necessary
		CheckForSplit();
	}
	
	public void CheckForSplit()
	{
		// TODO split if necessary
		HashSet<GrabbablePart> remainingParts = new HashSet<GrabbablePart>(PartsList);
		foreach (GrabbablePart part in remainingParts)
		{
			part.transform.parent = null;
		}
		
		// remove all the parts connected to here
//		HashSet<GrabbablePart> theseConnectedParts = new HashSet<GrabbablePart>(remainingParts.First().GetAllConnectedParts());
//		remainingParts.ExceptWith(connectedParts);
		
		
		Debug.Log("Starting with "+remainingParts.Count+ " parts");
		
		bool usedThisConstruction = false;
		int splitId = 0;
		// create new constructionf for each split
		while(remainingParts.Count != 0)
		{
			splitId += 1;
			GrabbablePart basePart = remainingParts.First();
			HashSet<GrabbablePart> connectedParts = new HashSet<GrabbablePart>(basePart.GetAllConnectedParts());
			remainingParts.ExceptWith(connectedParts);
			
			Debug.Log("Found "+connectedParts.Count+ " connected parts ("+remainingParts.Count+" remain)");
			
			Construction splitConstruction = null;
			if (usedThisConstruction)
			{
				Debug.Log ("Creating Construction "+splitId);
				GameObject constructionObject = new GameObject(this.name+"."+splitId);
				splitConstruction = constructionObject.AddComponent<Construction>();
			}
			else
			{
				splitConstruction = this;
				usedThisConstruction = true;
			}
//			Debug.Log ("Moving "+splitConstruction.name+" from "+splitConstruction.transform.position +" to"+ basePart.transform.position);
			splitConstruction.transform.position = basePart.transform.position;
			foreach(GrabbablePart part in connectedParts)
			{
				part.transform.parent = splitConstruction.transform;
			}
			splitConstruction.transform.parent = this.transform.parent;
		}
	}
	
	
	#region Construction tree encoding and decoding
	public static Construction Decode(string encoded, InstantiatePrefabDelegate instantiateFunction)
//	public static Construction Decode(string encoded)
	{
		
		Dictionary<int, GrabbablePart> idParts = new Dictionary<int, GrabbablePart>();
		Dictionary<GrabbablePart, string> partEncodings = new Dictionary<GrabbablePart, string>();
//		
//		List<GrabbablePart> elements = new List<GrabbablePart>();
		List<string> encodedElements = new List<string>(encoded.Split(','));
//		
		for (int i = 0 ; i < encodedElements.Count ; i++)
		{
			int id = CharSerializer.ToNumber(encodedElements[i][0]);
			PartType partType = (PartType)CharSerializer.ToNumber(encodedElements[i][1]);
			
			GameObject partObject = instantiateFunction(GameSettings.instance.GetPartPrefab(partType).gameObject);

			idParts[id] = partObject.GetComponent<GrabbablePart>();
			partEncodings[idParts[id]] = encodedElements[i];
			
			Debug.Log ("Creating part "+id);
		}
		
		foreach(int id in idParts.Keys)
		{
			idParts[id].SimulationOrientation = (HexMetrics.Direction)CharSerializer.ToNumber(partEncodings[idParts[id]][2]);

		}
		
		GameObject constructionObject = new GameObject("Decoded Construction");
		Construction construction = constructionObject.AddComponent<Construction>();
		
		
		HashSet<GrabbablePart> exploredParts = new HashSet<GrabbablePart>();
		System.Action<GrabbablePart> addToConstructionRecursively = null;
		
		addToConstructionRecursively = (newPart) => 
		{
			exploredParts.Add(newPart);
			construction.AddToConstruction(newPart);
			
			string code = partEncodings[newPart];
			int id = CharSerializer.ToNumber(code[0]);
			
			Debug.Log ("Connecting part "+id);
			
			for (int i = 0 ; i < 6 ; i++)
			{
				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
				int otherId = CharSerializer.ToNumber(code[3+(i*3)+0]);
				GrabbablePart.PhysicalConnectionType physicalConnType = (GrabbablePart.PhysicalConnectionType)CharSerializer.ToNumber(code[3+(i*3)+1]);
				int auxilaryConnectionType = CharSerializer.ToNumber(code[3+(i*3)+2]);
				
				
				GrabbablePart otherPart = otherId == 0 ? null : idParts[otherId];
				if (otherPart != null)
				{
					Debug.Log ("Connecting part "+id+" to "+otherId+" in direction "+iDir +"("+physicalConnType+", "+auxilaryConnectionType+")");
					newPart.ConnectPartAndPlaceAtRelativeDirection(otherPart, physicalConnType, iDir);
					newPart.SetPhysicalConnection(iDir, physicalConnType);
					newPart.SetAuxilaryConnections(iDir, auxilaryConnectionType);
					
					if (!exploredParts.Contains(otherPart))
					{
						addToConstructionRecursively(otherPart);
					}
					
				}
				else
				{
					newPart.SetPhysicalConnection(iDir, GrabbablePart.PhysicalConnectionType.None);
					newPart.SetAuxilaryConnections(iDir, 0);
				}
				
			}
		};
		
		
		if (idParts.ContainsKey(1))
		{
			idParts[1].transform.position = construction.transform.position;
			addToConstructionRecursively(idParts[1]);
		}
		
		// do this in such a way that they are placed properly
//		for (int i = 1 ; i < encodedElements.Count ; i++)
//		{
//			construction.AddToConstruction(idParts[i+1]);
//		}
			
		return construction;
		
	}
	
				
	
	public string Encode()
	{
		
		Dictionary<GrabbablePart, int> partID = new Dictionary<GrabbablePart, int>();

//		Debug.Log ("logging "+0);
		int idCount = 1;
		foreach (GrabbablePart part in Parts)
		{
//			Debug.Log ("logging "+idCount);
			partID[part] = idCount;
			idCount += 1;
		}
		
		
		List<string> encodedElements = new List<string>();
		// <Type><Orientation>,<PhysicalConn0>,<AuxConnect0>,<Child0??_>,<PhysicalConn1>...
		foreach (GrabbablePart element in Parts)
		{
			
			encodedElements.Add(element.Encode(partID));
			
		}
		
		return string.Join(",", encodedElements.ToArray());
		
	}
			
	public class ConstructionElement
	{
		public ConstructionElement()
		{
		}
		
		public ConstructionElement(GrabbablePart part, Dictionary<GrabbablePart, int> partIDs)
		{
			
		}
		
		public PartType partType = PartType.None;
		
		public HexMetrics.Direction orientation = HexMetrics.Direction.Up;
		
		public int id = 0;
		
//		public ConstructionElement [] weldedInDirection = new ConstructionElement [6];
		public int [] connectedParts = new int [6];
		public GrabbablePart.PhysicalConnectionType [] physicalConnectionType = new GrabbablePart.PhysicalConnectionType [6];
		public int [] auxilaryConnectionType = new int [6];
		
		
	}
	
	#region IComparable[Construction] implementation
	public int CompareTo (Construction other)
	{
		List<GrabbablePart> partList = PartsList;
		List<GrabbablePart> compList = other.PartsList;
		if (partList.Count != compList.Count) // different number of elements, can't be the same
		{
			return -1;
		}
		
		System.Action<IEnumerable<GrabbablePart>, Dictionary<PartType, int>, Dictionary<int, HashSet<GrabbablePart>>> determineMakeup = (parts, makeup, connLists) => 
		{
			foreach (GrabbablePart part in parts)
			{
				if (makeup.ContainsKey(part.partType))
				{
					makeup[part.partType] += 1;
				}
				else
				{
					makeup[part.partType] = 1;
				}
				int numberOfConnections = 0;
				for (int i = 0 ; i < 6 ; i++) 
				{
					if (part.GetConnectedPart((HexMetrics.Direction)i) != null) 
						numberOfConnections += 1;
				}
				if (!connLists.ContainsKey(numberOfConnections))
				{
					connLists[numberOfConnections] = new HashSet<GrabbablePart>();
				}
				connLists[numberOfConnections].Add(part);
			}
		};
		
		Dictionary<PartType, int> partListMakeup = new Dictionary<PartType, int>();
		Dictionary<int, HashSet<GrabbablePart>> partListConnections = new Dictionary<int, HashSet<GrabbablePart>>();
		Dictionary<PartType, int> compListMakeup = new Dictionary<PartType, int>();
		Dictionary<int, HashSet<GrabbablePart>> compListConnections = new Dictionary<int, HashSet<GrabbablePart>>();
		
		determineMakeup(partList, partListMakeup, partListConnections);
		determineMakeup(compList, compListMakeup, compListConnections);
		
		foreach(int num in partListConnections.Keys)
		{
			// if the comparison construction does not contain any parts with this number of connections or it contains a different of parts with equal connections
			if (!compListConnections.ContainsKey(num) || compListConnections[num].Count != partListConnections[num].Count)
			{
				return -2;
			}
		}
		Dictionary<GrabbablePart, GrabbablePart> equalConnParts = new Dictionary<GrabbablePart, GrabbablePart>();
		foreach(int num in partListConnections.Keys)
		{
			foreach(GrabbablePart part in partListConnections[num])
			{
				// find an associated part in the other list
				foreach(GrabbablePart comp in compListConnections[num])
				{
					if (HasSimilarConnectionsTo(part, comp))
					{
						equalConnParts[part] = comp;
						break;
					}
				}
				if (!equalConnParts.ContainsKey(part))
				{
					return -3;
				}
				compListConnections[num].Remove(equalConnParts[part]);
			}
		}
		
		return 0;
		
		int numberOfRarestTypes = partList.Count + 1;
		PartType rarestType = PartType.None;
		
		foreach(PartType type in partListMakeup.Keys)
		{
			// if the comparison construction does not contain this type or it contains a different number of that type
			if (!compListMakeup.ContainsKey(type) || compListMakeup[type] != partListMakeup[type])
			{
				return -4;
			}
			
			if (partListMakeup[type] < numberOfRarestTypes)
			{
				numberOfRarestTypes = partListMakeup[type];
				rarestType = type;
			}
		}
		
		// now we can compare (start with the rarest type to reduce number of comparisons)
		
		// for each part, find one with the same connections taking symmetry into account
		// (there might be more than one in the comparison construction, but each will have one and only one associated part)
		
		
		return 0;
	}
	
	public bool HasSimilarConnectionsTo (GrabbablePart part1, GrabbablePart part2)
	{
//		if (part1.symmetry != part2.symmetry)
//		{
//			return false;
//		}
		
		
		if (part1.symmetry == Symmetry.None)
		{
			return HasSimilarConnectionsTo(part1, part2, 0);
		}
		else if (part1.symmetry == Symmetry.TwoWay)
		{
			return 
				HasSimilarConnectionsTo(part1, part2, 0) ||
				HasSimilarConnectionsTo(part1, part2, 3);
		}
		else if (part1.symmetry == Symmetry.ThreeWay)
		{
			return 
				HasSimilarConnectionsTo(part1, part2, 0) ||
				HasSimilarConnectionsTo(part1, part2, 2) ||
				HasSimilarConnectionsTo(part1, part2, 4);
		}
		else if (part1.symmetry == Symmetry.SixWay)
		{
			return 
				HasSimilarConnectionsTo(part1, part2, 0) ||
				HasSimilarConnectionsTo(part1, part2, 1) ||
				HasSimilarConnectionsTo(part1, part2, 2) ||
				HasSimilarConnectionsTo(part1, part2, 3) ||
				HasSimilarConnectionsTo(part1, part2, 4) ||
				HasSimilarConnectionsTo(part1, part2, 5);
		}
		
		
		return false;
	}
	
	private bool HasSimilarConnectionsTo (GrabbablePart part1, GrabbablePart part2, int part2DirectionOffset)
	{
		for (int i = 0 ; i < 6 ; i++)
		{
			int p2i = (i+part2DirectionOffset)%6;
			HexMetrics.Direction p1Dir = (HexMetrics.Direction)i;
			HexMetrics.Direction p2Dir = (HexMetrics.Direction)p2i;
			
			GrabbablePart p1ConnectedPart = part1.GetConnectedPart(p1Dir);
			GrabbablePart p2ConnectedPart = part2.GetConnectedPart(p2Dir);
			if (p1ConnectedPart == null && p2ConnectedPart == null)
				continue;
			
			if (p1ConnectedPart != null && p2ConnectedPart != null)
			{
				if (p1ConnectedPart.partType != p2ConnectedPart.partType)
				{
					return false;
				}
				if (part1.GetPhysicalConnectionType(p1Dir) != part2.GetPhysicalConnectionType(p2Dir))
				{
					return false;
				}
				if (part1.GetAuxilaryConnectionTypes(p1Dir) != part2.GetAuxilaryConnectionTypes(p2Dir))
				{
					return false;
				}
			}
			else // one is null other is connected
			{
				return false;
			}
			
		}
	
		return true;
	}
	
	
	#endregion
	#endregion
	
	
	
	#region Construction tree traveral and access
//	
//	public IEnumerable<LocatedPart> GetAllPartsWithLocation()
//	{
//		foreach (LocatedPart loactedPart in RootPart.GetAllPartWithLocationFromThisPart(IntVector2.zero, new HashSet<GrabbablePart>()))
//		{
//			yield return loactedPart;
//		}
//	}
//	
//	private IEnumerable<LocatedPart> GetAllPartWithLocationFromThisPart(IntVector2 thisLocation, HashSet<GrabbablePart> exploredParts)
//	{
//		if (exploredParts.Contains(this))
//		{
//			yield break;
//		}
//		exploredParts.Add(this);
//		
//		yield return new LocatedPart(this, thisLocation);
////		Debug.Log ("adding "+thisLocation.x+":"+thisLocation.y);
//		
//		GrabbablePart parentPart = ParentPart;
//		
//		for (int i = 0 ; i < 6 ; i++)
//		{
//			GrabbablePart connectedPart = _connectedParts[i].connectedPart;
//			if (connectedPart != null && connectedPart != parentPart)
//			{
//				HexMetrics.Direction relativeDirection = (HexMetrics.Direction)i;
//				HexMetrics.Direction absoluteDirection = AbsoluteDirectionFromRelative(relativeDirection);
//				IntVector2 newLocation = HexMetrics.GetGridOffset(absoluteDirection) + thisLocation;
////				Debug.Log (thisLocation.x+":"+thisLocation.y+" -> "+newLocation.x+":"+newLocation.y+" (A:"+absoluteDirection+", R:"+relativeDirection+")");
//				foreach (LocatedPart loactedPart in connectedPart.GetAllPartWithLocationFromThisPart(newLocation, exploredParts))
//				{
//					yield return loactedPart;
//				}
//			}
//		}
//	}
	
//	public delegate void PartAtLocation(GrabbablePart part, IntVector2 location);
//	
//	void WalkChildrenWithLocation(PartAtLocation partAtLocationFunction, IntVector2 thisLocation)
//	{
//		partAtLocationFunction(this, thisLocation);
//		
//		GrabbablePart parentPart = ParentPart;
//		
//		for (int i = 0 ; i < 6 ; i++)
//		{
//			ConnectionDescription connectionDesc = connectedParts[i];
//			if (connectionDesc != null && connectionDesc.connectedPart != parentPart)
//			{
//				HexMetrics.Direction absoluteDirection = GetAbsoluteDirection((HexMetrics.Direction)i);
//				
//				WalkChildrenWithLocation(partAtLocationFunction, HexMetrics.GetRelativeLocation(absoluteDirection) + thisLocation);
//			}
//		}
//	}
	
//	public void ForEachChild(PartAtLocation partAtLocationFunction)
//	{
//		IntVector2 rootLocation = RootLocation;
////		GrabbablePart rootPart = RootPart;
//		
//		RootPart.WalkChildrenWithLocation(partAtLocationFunction, rootLocation);
//	}
//	void WalkChildrenWithLocation(PartAtLocation partAtLocationFunction, IntVector2 thisLocation)
//	{
//		partAtLocationFunction(this, thisLocation);
//		
//		GrabbablePart parentPart = ParentPart;
//		
//		for (int i = 0 ; i < 6 ; i++)
//		{
//			ConnectionDescription connectionDesc = connectedParts[i];
//			if (connectionDesc != null && connectionDesc.connectedPart != parentPart)
//			{
//				HexMetrics.Direction absoluteDirection = GetAbsoluteDirection((HexMetrics.Direction)i);
//				
//				WalkChildrenWithLocation(partAtLocationFunction, HexMetrics.GetRelativeLocation(absoluteDirection) + thisLocation);
//			}
//		}
//	}
	
	#endregion
	//=========================================================================================
	
	
	
	
}
