using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public delegate GameObject InstantiatePrefabDelegate (GameObject prefab);
//public delegate T InstantiatePrefabDelegate<T> (T prefab) where T : MonoBehaviour;

public class Construction : MonoBehaviour, System.IComparable<Construction>, IPooledObject, IEncodable
{

		//=========================================================================================
	
	
	public int idNumber = -1;
	
	public bool ignoreCollisions = false;
	
	public bool HasChild
	{
		get 
		{
			return FirstPart != null;
		}
	}
	public GrabbablePart CenterPart
	{
		get 
		{
			foreach (GrabbablePart part in Parts)
			{
				if (part.transform.localPosition == Vector3.zero)
				{
					return part;
				}
			}
			return null;
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
	public int Count
	{
		get 
		{
			int count = 0;
			
			foreach (GrabbablePart part in Parts)
			{
				count+=1;
			}
			return count;
		}
	}
	
	public bool Contains(GrabbablePart isPart)
	{
		foreach (GrabbablePart part in Parts)
		{
			if (isPart == part)
			{
				return true;
			}
		}
		return false;
	}
	
	public List<GrabbablePart> PartsList
	{
		get 
		{
			return new List<GrabbablePart> (Parts);
		}
	}
	
//	public Grabber heldAndMovingGrabber { get; set; }

	#region IPooledObject implementation
	public void OnPoolActivate ()
	{
		ignoreCollisions = false;
	}

	public void OnPoolDeactivate ()
	{
		ignoreCollisions = true;
		// need to disconnect parts from construction and delete(pool) them
		foreach (GrabbablePart part in PartsList)
		{
			part.transform.parent = null;
			ObjectPoolManager.DestroyObject(part);
		}
	}
	#endregion	

	public void AddToConstruction(GrabbablePart newPart)
	{
		Construction otherConstruction = newPart.ParentConstruction;
		
		
		if (otherConstruction != null && otherConstruction != this)
		{
			// move all otherConstruction's parts here
			
			foreach(GrabbablePart otherPart in otherConstruction.PartsList)
			{
				
				GrabberManager.instance.TransferConstruction(otherPart, this);
//				otherPart.transform.parent = this.transform;
//				Debug.Log("Transferring "+otherPart.idNumber+" from "+otherConstruction.idNumber+" to "+this.idNumber);
			}

			
			ObjectPoolManager.DestroyObject(otherConstruction);
			
		}
		else
		{
			newPart.transform.parent = this.transform;
		}
		
	}
	
	
	public IEnumerable<Construction> RemoveFromConstruction(GrabbablePart partToRemove)
	{
		// disconnect part from construction
		for (int i = 0 ; i < 6 ; i++)
		{
			partToRemove.SetPhysicalConnection((HexMetrics.Direction)i, GrabbablePart.PhysicalConnectionType.None, GrabbablePart.SplitOptions.DoNotSplit);
		}
		
		
		// split if necessary
		return CheckForSplitsOrJoins();
	}
	
	public void CenterConstruction (GrabbablePart childPart)
	{
		if (childPart.ParentConstruction != this)
		{
			return;
		}
		
		Vector3 diff = childPart.transform.localPosition;
		
		foreach (GrabbablePart part in Parts)
		{
			part.transform.localPosition -= diff;
		}
		
	}
	
	public HashSet<Construction> CheckForSplitsOrJoins()
	{
//		GrabberManager.instance.RegisterConstructionAboutToReform(this);
		HashSet<GrabbablePart> remainingParts = new HashSet<GrabbablePart>(PartsList);
//		foreach (GrabbablePart part in remainingParts)
//		{
//			part.transform.parent = null;
//		}
		
		// remove all the parts connected to here
//		HashSet<GrabbablePart> theseConnectedParts = new HashSet<GrabbablePart>(remainingParts.First().GetAllConnectedParts());
//		remainingParts.ExceptWith(connectedParts);
		
		
		
		
		HashSet<Construction> newConstructions = new HashSet<Construction>();
		
//		if (remainingParts.Count == 1) return newConstructions;
		
//		Debug.Log("Splitting with "+remainingParts.Count+ " parts");
		
		int splitId = -1;
		// create new constructionf for each split
		while(remainingParts.Count != 0)
		{
			splitId += 1;
			GrabbablePart basePart = remainingParts.First();
			HashSet<GrabbablePart> connectedParts = new HashSet<GrabbablePart>(basePart.GetAllConnectedParts());
			remainingParts.ExceptWith(connectedParts);
//			Debug.Log("Found "+connectedParts.Count+ " connected parts ("+remainingParts.Count+" remain) (contructions "+newConstructions.Count+")");
			
			if (newConstructions.Count == 0 && remainingParts.Count == 0)
			{
				return newConstructions; // if there is only one construction (this one) 
			}
			
//			Debug.Log("Found "+connectedParts.Count+ " connected parts ("+remainingParts.Count+" remain)");
			
			Construction splitConstruction = null;
			
//			Debug.Log ("Creating Construction "+splitId+" ("+remainingParts.Count+")");
			splitConstruction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
//			
//			splitConstruction.name = this.name+"."+splitId;
			splitConstruction.ignoreCollisions = ignoreCollisions;
			
//			Debug.Log ("Moving "+splitConstruction.name+" from "+splitConstruction.transform.position +" to"+ basePart.transform.position);
			splitConstruction.transform.position = basePart.transform.position;
			foreach(GrabbablePart part in connectedParts)
			{
				GrabberManager.instance.TransferConstruction(part, splitConstruction);
				
			}
			splitConstruction.transform.parent = this.transform.parent;
			
			newConstructions.Add(splitConstruction);
		}
		
		ObjectPoolManager.DestroyObject(this);
		
//		GrabberManager.instance.RegisterConstructionPostReform();
		return newConstructions;
	}
	
	public struct PartSide
	{
		public GrabbablePart part;
		public HexMetrics.Direction relativeDirection;
		public int offsetFromSide;
	}
	
	public IEnumerable<PartSide> IsConnectable(GrabbablePart otherPart)
	{
		Vector3 otherPartLocation = otherPart.PartSphereCollider.transform.position;
		float radSq = otherPart.PartSphereCollider.radius * otherPart.PartSphereCollider.radius;
		foreach (GrabbablePart part in Parts)
		{
			Vector3 partLocation = part.transform.position;
			if (Vector3.SqrMagnitude(otherPartLocation - partLocation) <  radSq)
			{
				yield break;
			}
		}
//		List<PartSide> partSides = new List<PartSide>();
		foreach (GrabbablePart part in Parts)
		{
			for (int i = 0 ; i < 6 ; i ++)
			{
				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
				HexMetrics.Direction iDirRelative = part.Relative(iDir);
				if (part.GetConnectedPart(iDirRelative) == null)
				{
					
					Vector3 partLocation = part.transform.position;
					Vector3 potentialPartLocation = partLocation+(Vector3)GameSettings.instance.hexCellPrefab.GetDirection(iDir);
					
					if (Vector3.SqrMagnitude(otherPartLocation - potentialPartLocation) <  radSq)
					{
						HashSet<HexMetrics.Direction> weldableRotations = new HashSet<HexMetrics.Direction>(part.IsWeldableWithRotationFactor(iDirRelative, otherPart));
//						Debug.Log(string.Join(", ", new List<HexMetrics.Direction>(weldableRotations).ConvertAll<string>((input) => ""+input).ToArray()));
//						Color debugColor = Color.red;
						
						if (weldableRotations.Count > 0)
						{
							PartSide partSide = new PartSide();
							foreach (HexMetrics.Direction weldableDir in new HexMetrics.Direction[]{
								HexMetrics.Direction.Down, 
								HexMetrics.Direction.LeftDown,
								HexMetrics.Direction.RightDown,
								HexMetrics.Direction.LeftUp,
								HexMetrics.Direction.RightUp,
								HexMetrics.Direction.Up
							})
							{
								if (weldableRotations.Contains(weldableDir))
								{
									partSide.offsetFromSide = GrabbablePart.RotationDifference(weldableDir);
								}
							}
							partSide.part = part;
							partSide.relativeDirection = iDirRelative;
							
							yield return partSide;
						}
//							debugColor = Color.green;
//						}
						
						
//						Debug.Log("Found at "+part.name+" in "+iDir);
//						Debug.DrawLine(part.transform.position, potentialPartLocation, debugColor);
						
						
					}
				}
			}
		}
		
	}
	
	
	#region Construction tree encoding and decoding
	
	public static Construction CreateSimpleConstruction(PartType partType)	
	{
		Construction singleConstruction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
		
		if (partType != PartType.None)
		{
			GrabbablePart singlePart = ObjectPoolManager.GetObject(GameSettings.instance.GetPartPrefab(partType));
			singleConstruction.AddToConstruction(singlePart);
			singlePart.transform.localPosition = Vector3.zero;
		}
		singleConstruction.transform.position = Vector3.zero;
		return singleConstruction;
	}
		
	public static Construction DecodeCopy(Construction toCopy)
	{
		Construction newConstruction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
		Encoding.DecodeCopy(newConstruction, toCopy);
		return newConstruction;
	}
	public static Construction DecodeCreate(Encoding encodedElements)
	{
		Construction newConstruction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
		Encoding.Decode(newConstruction, encodedElements);
		return newConstruction;
	}
	public static Construction DecodeCreate(string encodedString)
	{
		Construction newConstruction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
		Encoding.Decode(newConstruction, encodedString);
		return newConstruction;
	}
	
	
	public bool Decode(Encoding encodedElements)
	{
//		List<string> encodedElements = new List<string>(CharSerializer.AllStrings(encoding));
		
//		Encoding encodedElements = new Encoding(encodings);
		
//		string fullEncoding = string.Join (" | ", encodedElements.ToArray());
		
		Debug.Log ("Decoding Construction: \n"+encodedElements.DebugString());
		if (encodedElements.Count == -1 || encodedElements.Count == 0 || (encodedElements.Count == 1 && encodedElements.IsInt(0)))
		{
			return true;
		}
		
//		if (encodedElements.Count == 1 && encodedElements.IsInt(0))
//		{
//			PartType partType = (PartType)encodedElements.Int(0);
//			return CreateSimpleConstruction(partType);
//		}
		
		
		Dictionary<int, GrabbablePart> idParts = new Dictionary<int, GrabbablePart>();
		Dictionary<GrabbablePart, Encoding> partEncodings = new Dictionary<GrabbablePart, Encoding>();
//		
//		List<GrabbablePart> elements = new List<GrabbablePart>();
//		List<string> encodedElements = new List<string>(encoded.Split(','));
		
		
//		
		int centerId = -1;
		
		for (int i = 0 ; i < encodedElements.Count ; i++)
		{
			Encoding partEncoding = encodedElements.SubEncoding(i);
			int id = partEncoding.Int(0);
//			Debug.Log ("Encoding "+encodedElements[i][0]+" ("+id+")");
			if (centerId == -1)
			{
				centerId = id;
			}
			PartType partType = (PartType)partEncoding.Int(1);
			

			idParts[id] = ObjectPoolManager.GetObject(GameSettings.instance.GetPartPrefab(partType));
			partEncodings[idParts[id]] = partEncoding;
			
//			Debug.Log ("Creating part "+id);
		}
		
		foreach(int id in idParts.Keys)
		{
			idParts[id].SetSimulationOrientation(partEncodings[idParts[id]].Int(2));

		}
		
//		Construction construction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
//		construction.name = "Decoded Construction";
		
		
		HashSet<GrabbablePart> exploredParts = new HashSet<GrabbablePart>();
		System.Func<GrabbablePart, bool> addToConstructionRecursively = null;
		
		addToConstructionRecursively = (newPart) => 
		{
			exploredParts.Add(newPart);
			this.AddToConstruction(newPart);
			
			Encoding code = partEncodings[newPart];
			int id = code.Int(0);
			PartType type = (PartType) code.Int(1);
			
//			Debug.Log ("Connecting part "+id+"("+type+")");
			
			for (int i = 0 ; i < 6 ; i++)
			{
				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
				int otherId =  code.Int(3+(i*3)+0);
				GrabbablePart.PhysicalConnectionType physicalConnType = (GrabbablePart.PhysicalConnectionType)code.Int(3+(i*3)+1);
				int auxilaryConnectionType = code.Int(3+(i*3)+2);
				
//				Debug.Log("Connection Def: "+id+"->"+otherId+":"+iDir+":"+physicalConnType);
				
				if (otherId != 0 && !idParts.ContainsKey(otherId))
				{
					Debug.LogError ("idParts does not contain part with id "+otherId+"\n"+string.Join(", ", new List<int>(idParts.Keys).ConvertAll((e) => ""+e).ToArray()));
					return false;
					
				}
				GrabbablePart otherPart = otherId == 0 ? null : idParts[otherId];
				if (otherPart != null)
				{
//					Debug.Log ("Connecting part "+id+" to "+otherId+" in direction "+iDir +"("+physicalConnType+", "+auxilaryConnectionType+")");
					newPart.ConnectPartAndPlaceAtRelativeDirection(otherPart, physicalConnType, iDir);
					newPart.SetPhysicalConnection(iDir, physicalConnType);
					newPart.SetAuxilaryConnections(iDir, auxilaryConnectionType);
					
					if (!exploredParts.Contains(otherPart))
					{
						if (!addToConstructionRecursively(otherPart))
						{
							return false;
						}
					}
					
				}
				else
				{
					newPart.SetPhysicalConnection(iDir, GrabbablePart.PhysicalConnectionType.None, GrabbablePart.SplitOptions.DoNotSplit);
					newPart.SetAuxilaryConnections(iDir, 0);
				}
				
			}
			return true;
		};
		
		
		if (idParts.ContainsKey(centerId))
		{
			idParts[centerId].transform.position = this.transform.position;
			if (!addToConstructionRecursively(idParts[centerId]))
			{
				foreach(GrabbablePart part in idParts.Values) 
				{
					ObjectPoolManager.DestroyObject(part);
				}
				ObjectPoolManager.DestroyObject(this);
				
				return Construction.CreateSimpleConstruction(PartType.Standard6Sided);
			}
				
		}
		this.CenterConstruction(idParts[centerId]);
		
		// do this in such a way that they are placed properly
//		for (int i = 1 ; i < encodedElements.Count ; i++)
//		{
//			construction.AddToConstruction(idParts[i+1]);
//		}
		
		return true;
	}
				
	
	public IEnumerable<IEncodable> Encode()
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
		
		if (CenterPart != null)
		{
			// <Type><Orientation>,<PhysicalConn0>,<AuxConnect0>,<Child0??_>,<PhysicalConn1>...
			foreach (GrabbablePart element in CenterPart.GetAllConnectedParts())
	//		foreach (GrabbablePart element in Parts)
			{
				yield return new EncodableSubGroup(element.EncodeWithContext(partID));
			}
		}
		
//		return string.Join(",", encodedElements.ToArray());
		
	}
			
	class ConstructionElement
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
	
	public enum CompareCode {Equal = 0, NumElementDiffer = -1, ConnectionsDiffer = -2, PartConnectionsDiffer = -3, PartMakeupDiffer = -4};
	
	#region IComparable[Construction] implementation
	public int CompareTo (Construction other)
	{
		List<GrabbablePart> partList = PartsList;
		List<GrabbablePart> compList = other.PartsList;
		if (partList.Count != compList.Count) // different number of elements, can't be the same
		{
			return (int)CompareCode.NumElementDiffer;
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
				return (int)CompareCode.ConnectionsDiffer;
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
					return (int)CompareCode.PartConnectionsDiffer;
				}
				compListConnections[num].Remove(equalConnParts[part]);
			}
		}
		
		return (int)CompareCode.Equal;
		
		int numberOfRarestTypes = partList.Count + 1;
		PartType rarestType = PartType.None;
		
		foreach(PartType type in partListMakeup.Keys)
		{
			// if the comparison construction does not contain this type or it contains a different number of that type
			if (!compListMakeup.ContainsKey(type) || compListMakeup[type] != partListMakeup[type])
			{
				return (int)CompareCode.PartMakeupDiffer;
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
		
		
		return (int)CompareCode.Equal;
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
