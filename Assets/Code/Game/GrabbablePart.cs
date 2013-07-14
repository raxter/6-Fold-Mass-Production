using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LocatedPart
{
	public LocatedPart(GrabbablePart __part, IntVector2 __location)
	{
		part = __part;
		location = __location;
	}
	
	public GrabbablePart part;
	public IntVector2 location;
}



public enum PartType {None, Standard6Sided, Standard3Sided, Wheel3Sided}

public enum Symmetry {None, TwoWay, ThreeWay, SixWay}

public class GrabbablePart : MonoBehaviour, IPooledObject
{
	

	
	public Construction ParentConstruction
	{
		get
		{
			Transform parent = transform.parent;
			if (parent != null)
			{
				return parent.gameObject.GetComponent<Construction>();
			}
			else
			{
				return null;
			}
		}
	}
	
	public SphereCollider PartSphereCollider
	{
		get 
		{
			return _sphereCollider;
		}
	}
	
	SphereCollider _sphereCollider;
	
	public PartType partType;
	
	public Symmetry symmetry = Symmetry.None;
	
	public bool weldsUp = false;
	public bool weldsRightUp = false;
	public bool weldsRightDown = false;
	public bool weldsDown = false;
	public bool weldsLeftDown = false;
	public bool weldsLeftUp = false;
	
	public bool Weldable(HexMetrics.Direction dir)
	{
		switch (dir)
		{
		case HexMetrics.Direction.Up:
			return weldsUp;
		case HexMetrics.Direction.RightUp:
			return weldsRightUp;
		case HexMetrics.Direction.LeftUp:
			return weldsLeftUp;
		case HexMetrics.Direction.Down:
			return weldsDown;
		case HexMetrics.Direction.RightDown:
			return weldsRightDown;
		case HexMetrics.Direction.LeftDown:
			return weldsLeftDown;
		}
		return false;
	}
	
	
	public bool Weldable(int i)
	{
		return Weldable((HexMetrics.Direction)i);
	}
	
	
	[SerializeField]
	GameObject _highlightItem;
	
	public bool highlighted
	{
		get
		{
			return _highlightItem.activeSelf;
		}
		set
		{
//			Debug.Log(gameObject+" selected = "+value);
			_highlightItem.SetActive(value);
		}
	}
	
	
	IntVector2 GetGridLocationFromPosition()
	{
		HexCell zeroCell = GridManager.instance.GetHexCell(IntVector2.zero);
		Vector3 relativeLocation =  gameObject.transform.position - zeroCell.gameObject.transform.position;
//		Debug.Log ("regestering at "+relativeLocation);
		float sideLength = zeroCell.SideLength;
		int x = (int)((relativeLocation.x+1)/(sideLength*1.5f));
//		Debug.Log ("x "+x);
//		float yRelative = relativeLocation.y-x*
		int y = ((int)((relativeLocation.y+1)/(zeroCell.Height/2)) - x)/2;
//		Debug.Log ("y "+y);
		
		return new IntVector2(x,y);
	}

	public void RegisterLocationFromPosition ()
	{
		
//		Debug.Log ("registering at "+x+":"+oy);
		//find hex cell, register
		HexCell cellUnderPart = GridManager.instance.GetHexCell(GetGridLocationFromPosition());
		if (cellUnderPart!= null)
		{
			cellUnderPart.RegisterPart(this);
		}
	}
	
	
	public int idNumber = -1;
	
	bool finished = false;
	
	public bool IsFinished { get { return finished; } }
	
	
	
	//=========================================================================================
	#region Direction Functions
	
	/*
	 * SimulationOrientation - Orientation the cell is oriented relative to up
	 * RelativeDirection - a direction relative to SimulationOrientation
	 * AbsoluteDirection - a direction relative to Up
	 * 
	 * */
	
	public void RotatateSimulationOrientation(HexMetrics.Direction offset)
	{
		RotatateSimulationOrientation((int)offset);
	}
	
	public void RotatateSimulationOrientation(int offset)
	{
		SetSimulationOrientation(((int)SimulationOrientation-offset+6)%6);
	}
	
	
	public bool WillSimulationOrientationRotatateSplitConstruction(int offset)
	{
		bool hasNoConnections = true;
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction relativeI = (HexMetrics.Direction)i;
			HexMetrics.Direction newRelativeI = (HexMetrics.Direction)((i+offset+6)%6);
			
			// if the new direction will connect to a part, then the construction will not split
			if (GetConnectedPart(relativeI) != null)
			{
				Debug.Log (GetConnectedPart(relativeI).name+":"+Weldable(newRelativeI));
				// it is connected in this direction
				if (Weldable(newRelativeI))
				{
					return false;
				}
				hasNoConnections = false;
			}
		}
		
		return !hasNoConnections;
	}
		
	public HexMetrics.Direction SimulationOrientation
	{
		get 
		{
			float epsilon = 1;
			int directionInt = (int)((-transform.rotation.eulerAngles.z-epsilon)/60);
			directionInt = (directionInt + 6) % 6;
			return (HexMetrics.Direction)directionInt;
		}
	}
	public int SimulationOrientationInt
	{
		get 
		{
			return (int)SimulationOrientation;
		}
	}
	public HashSet<Construction> SetSimulationOrientation(int orientation)
	{
		return SetSimulationOrientation((HexMetrics.Direction)orientation);
	}
	public HashSet<Construction> SetSimulationOrientation(HexMetrics.Direction orientation)
	{
		
		for (int i = 0 ; i < 6 ; i++)
		{
			if ( _connectedParts[i] == null)
			{
				 _connectedParts[i] = new ConnectionDescription();
			}
		}
		
//			adjacent.ForEach((obj) => obj.parent = null);
		int directionChange = ((int)orientation - (int)SimulationOrientation + 6)%6;
		transform.rotation = Quaternion.Euler(0, 0, (int)orientation * -60);
		
		ConnectionDescription [] newParts = new ConnectionDescription [6];
		PhysicalConnectionType [] physicalConnections = new PhysicalConnectionType [6];
		int [] auxilaryConnections = new int [6];
		
//			adjacent.ForEach((obj) => obj.parent = transform);
		
		for (int i = 0 ; i < 6 ; i++)
		{
			int newDirection = (i+directionChange)%6;
			newParts[i] = _connectedParts[newDirection];
			
			physicalConnections[i] = _connectedParts[newDirection].connectionType;
			auxilaryConnections[i] = _connectedParts[newDirection].auxConnectionTypes;
		}
		for (int i = 0 ; i < 6 ; i++)
		{
			int newDirection = (i+directionChange)%6;
			_connectedParts[i] = newParts[i];
			
			if (_connectedParts[i].connectedPart == null)
			{
				physicalConnections[i] = PhysicalConnectionType.None;
				auxilaryConnections[i] = 0;
			}
//			Debug.Log ("Replacing Connection ("+(HexMetrics.Direction)newDirection+") "+ _connectedParts[i].connectionType +" -> ("+(HexMetrics.Direction)i+") " + physicalConnections[i]);
			SetPhysicalConnection((HexMetrics.Direction)i, physicalConnections[i], SplitOptions.DoNotSplit);
			SetAuxilaryConnections((HexMetrics.Direction)i, auxilaryConnections[i]);
			
		}
		
		if (ParentConstruction != null)
		{
			return ParentConstruction.CheckForSplitsOrJoins();
		}
		return new HashSet<Construction>();
	}
	
	
	
	public HexMetrics.Direction Relative(HexMetrics.Direction absoluteDirection)
	{
		return RelativeDirectionFromAbsolute(absoluteDirection);
	}
	public HexMetrics.Direction RelativeDirectionFromAbsolute(HexMetrics.Direction absoluteDirection)
	{
		return (HexMetrics.Direction)( ((int)absoluteDirection + 6 - (int)SimulationOrientation) % 6);
	}
	public HexMetrics.Direction Absolute(HexMetrics.Direction relativeDirection)
	{
		return AbsoluteDirectionFromRelative(relativeDirection);
	}
	public HexMetrics.Direction AbsoluteDirectionFromRelative(HexMetrics.Direction relativeDirection)
	{
		return (HexMetrics.Direction)( ((int)relativeDirection + (int)SimulationOrientation) % 6);
	}
	
	public static HexMetrics.Direction Reverse(HexMetrics.Direction direction)
	{
		return ReverseDirection(direction);
	}
	public static HexMetrics.Direction ReverseDirection(HexMetrics.Direction direction)
	{
		return (HexMetrics.Direction)( ((int)direction+3)%6 );
	}
	
	public HexMetrics.Direction ConnectedsOpposite(HexMetrics.Direction relativeDirection)
	{
		return ConnectedPartsOppositeDirection(relativeDirection);
	}
	public HexMetrics.Direction ConnectedPartsOppositeDirection(HexMetrics.Direction relativeDirection)
	{
		GrabbablePart connectedPart = GetConnectedPart(relativeDirection);
		if (connectedPart == null)
			return (HexMetrics.Direction)(-1);
		
		return OtherPartsOppositeDirection(relativeDirection, connectedPart);
	}
	public HexMetrics.Direction OtherPartsOppositeDirection(HexMetrics.Direction relativeDirection, GrabbablePart otherPart)
	{
		return ReverseDirection( otherPart.RelativeDirectionFromAbsolute(AbsoluteDirectionFromRelative(relativeDirection)));
	}
	
	#endregion
	//=========================================================================================
	
	#region Connection Descriptions (all relative to Up)
	
	public enum PhysicalConnectionType {None = 0, Weld = 1, Magentic = 2};
	public enum AuxilaryConnectionType {Electric = 1, Belt = 2, Hydraulic = 4};
	// weld is a normal connection, will physically keep the parts together
	// magnet is a connection with string magnets, will physically keep the parts together
	// wired means it is electrically connected (though not physically, just wired connections alone won't keep them together)
	// belt means that a rotary part is connected (engine, wheel, gun, anything operated by an engine)
	
	
	[System.Serializable]
	class ConnectionDescription
	{
		public GrabbablePart connectedPart;
		
		
		public PhysicalConnectionType connectionType = GrabbablePart.PhysicalConnectionType.None;
		public int auxConnectionTypes = 0;

		public void Reset ()
		{
			connectedPart = null;
			connectionType = GrabbablePart.PhysicalConnectionType.None;
			auxConnectionTypes = 0;
		}
	}
	
	
	// parts are stored by relative direction
	[SerializeField]
	ConnectionDescription [] _connectedParts = new ConnectionDescription [6];
	
	GameObject [] _weldSpriteObjects = new GameObject [6];
	
	#endregion
	
	#region Connection Access
	
//	public GrabbablePart GetConnectedPartAtRelative(HexMetrics.Direction relativeDirection)
//	{
//		return _connectedParts[Absolute(relativeDirection)].connectedPart;
//	}
	
	public GrabbablePart GetConnectedPart(HexMetrics.Direction relativeDirection)
	{
		return _connectedParts[(int)relativeDirection].connectedPart;
	}
	
	public struct PartSide
	{
		public GrabbablePart part;
		public HexMetrics.Direction relativeDirection;
	}
	public IEnumerable<PartSide> GetConnectedPartsWithDirection()
	{
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			GrabbablePart connPart = GetConnectedPart(iDir);
			if (connPart != null)
			{
				yield return new PartSide() { part = connPart, relativeDirection = iDir, };
			}
		}
	}
	
	public IEnumerable<GrabbablePart> GetConnectedParts()
	{
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			GrabbablePart connPart = GetConnectedPart(iDir);
			
			if (connPart != null)
			{
				yield return connPart;
			}
		}
	}
	public GrabbablePart RemoveConnectedPart(HexMetrics.Direction relativeDirection)
	{
		GrabbablePart ret = _connectedParts[(int)relativeDirection].connectedPart;
		
//		if ( transform.parent != null && ret != null && ret.transform == transform.parent )
//		{
//			transform.parent = null;
//		}
		_connectedParts[(int)relativeDirection].connectedPart = null;
		_connectedParts[(int)relativeDirection].connectionType = PhysicalConnectionType.None;
		_connectedParts[(int)relativeDirection].auxConnectionTypes = 0;
		
		return ret;
	}
	
//	public void ConnectPartAndPlaceAtAbsoluteDirection(GrabbablePart otherPart, PhysicalConnectionType connectionType, HexMetrics.Direction absoluteDirection)
//	{
//		ConnectPartAndPlaceAtRelativeDirection(otherPart, connectionType, Relative(absoluteDirection));
//	}
	
	public void ConnectPartAndPlaceAtRelativeDirection(GrabbablePart otherPart, PhysicalConnectionType connectionType, HexMetrics.Direction relativeDirection)
	{
		HexMetrics.Direction absoluteDirection = Absolute(relativeDirection);
		ConnectionDescription connDesc = _connectedParts[(int)relativeDirection];
		
		connDesc.connectedPart = otherPart; // connect the part
		
		ConnectionDescription otherConnDesc = otherPart._connectedParts[(int)ConnectedsOpposite(relativeDirection)]; // get the other parts opposite side that is connected to this
		
		otherConnDesc.connectedPart = this; // connect that side
		
		connDesc.connectedPart.transform.position = transform.position + (Vector3)GameSettings.instance.hexCellPrefab.GetDirection(absoluteDirection);
		
		SetPhysicalConnection(relativeDirection, connectionType);
		
		if (ParentConstruction != null)
		{
//			Debug.Log("Adding to construction "+ParentConstruction.name);
			ParentConstruction.AddToConstruction(otherPart);
		}
		
	}
	
	
	
	#endregion
	
	
	public PhysicalConnectionType GetPhysicalConnectionType(HexMetrics.Direction relativeDirection)
	{
		if (GetConnectedPart(relativeDirection) == null)
		{
			return PhysicalConnectionType.None;
		}
		return _connectedParts[(int)relativeDirection].connectionType;
	}
	
	private void SetWeldSprite(HexMetrics.Direction relativeDirection, bool show)
	{
		GameObject weldSprite = _weldSpriteObjects[(int)relativeDirection];
		if (weldSprite != null)
		{
			weldSprite.transform.localScale = Vector3.one * (show ? 1 : 0);
		}
	}
	
	public enum SplitOptions {SplitIfNecessary, DoNotSplit};
	
	public HashSet<Construction> SetPhysicalConnection(HexMetrics.Direction relativeDirection, 
													PhysicalConnectionType newConnectionType)
	{
		return SetPhysicalConnection(relativeDirection, newConnectionType, SplitOptions.SplitIfNecessary);
	}
	public HashSet<Construction> SetPhysicalConnection(HexMetrics.Direction relativeDirection, 
													PhysicalConnectionType newConnectionType, 
													SplitOptions splitOption)
	{
		ConnectionDescription connDesc = _connectedParts[(int)relativeDirection];
//		PhysicalConnectionType originalConnection = connDesc.connectionType;
		if (connDesc.connectedPart == null)
		{
			
			// disconnecting part
			connDesc.connectionType = PhysicalConnectionType.None;
			SetWeldSprite(relativeDirection, false);
			
			if (ParentConstruction != null)
			{
				if (splitOption == SplitOptions.SplitIfNecessary)
				{
					return ParentConstruction.CheckForSplitsOrJoins();
				}
				return new HashSet<Construction> { ParentConstruction };
			}
			
			return new HashSet<Construction> ();
		}
		
		HexMetrics.Direction oppositeDirection = ConnectedsOpposite(relativeDirection);
		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[(int)oppositeDirection];
		
		bool weldableHere = Weldable(relativeDirection);
		bool weldableThere = connDesc.connectedPart.Weldable(oppositeDirection);
//		Debug.Log("Checking weldability: "+direction+"("+weldableHere+") <-> "+oppositeDirection+"("+weldabelThere+")");
		if (weldableHere && weldableThere)
		{
			connDesc.connectionType = newConnectionType;
			otherConnDesc.connectionType = newConnectionType;
		}
		else
		{
			connDesc.connectionType = PhysicalConnectionType.None;
			otherConnDesc.connectionType = PhysicalConnectionType.None;
		}
		
		// update the weld sprites
		SetWeldSprite(relativeDirection, connDesc.connectionType != PhysicalConnectionType.None);
		connDesc.connectedPart.SetWeldSprite(oppositeDirection, connDesc.connectionType != PhysicalConnectionType.None);
		
		
		//if we are disconnecting the side (None) or if it's not weldable, make sure that the parts are not connected
		if (connDesc.connectionType == PhysicalConnectionType.None)
		{
			otherConnDesc.connectedPart = null;
			connDesc.connectedPart = null;
			
			if (ParentConstruction == null)
			{
				Debug.LogWarning ("Parent is null!");
			}
			else
			{
				// check that by disconnecting a side, we have not split the construction up
				if (splitOption == SplitOptions.SplitIfNecessary)
				{
					return ParentConstruction.CheckForSplitsOrJoins();
				}
				return new HashSet<Construction> { ParentConstruction };
			}
		}
		if (ParentConstruction != null)
		{
			return new HashSet<Construction> { ParentConstruction };
		}
		return new HashSet<Construction>();
	}
	
	public int GetAuxilaryConnectionTypes(HexMetrics.Direction relativeDirection)
	{
		if (GetConnectedPart(relativeDirection) == null)
		{
			return 0;
		}
		return _connectedParts[(int)relativeDirection].auxConnectionTypes;
	}
	
	public void SetAuxilaryConnections(HexMetrics.Direction relativeDirection, int newConnectionTypes)
	{
		ConnectionDescription connDesc = _connectedParts[(int)relativeDirection];
		if (connDesc.connectedPart == null)
		{
			connDesc.auxConnectionTypes = 0;
			return;
		}
		
		HexMetrics.Direction oppositeDirection = ConnectedsOpposite(relativeDirection);
		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[(int)oppositeDirection];
		
		bool weldableHere = Weldable(relativeDirection);
		bool weldabelThere = connDesc.connectedPart.Weldable(oppositeDirection);
		
		if (weldableHere && weldabelThere)
		{
			connDesc.auxConnectionTypes = newConnectionTypes;
			otherConnDesc.auxConnectionTypes = newConnectionTypes;
		}
		else
		{
			connDesc.auxConnectionTypes = 0;
			otherConnDesc.auxConnectionTypes = 0;
		}
	}
	
	
	public IEnumerable<HexMetrics.Direction> IsWeldableWithRotationFactor (HexMetrics.Direction relativeDirection, GrabbablePart otherPart)
	{
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			if (IsWeldable(relativeDirection, otherPart, iDir))
			{
				yield return iDir;
			}
		}
	}
	public bool IsWeldable (HexMetrics.Direction relativeDirection, GrabbablePart otherPart)
	{
		return IsWeldable(relativeDirection, otherPart, (HexMetrics.Direction)0);
	}
	public bool IsWeldable (HexMetrics.Direction relativeDirection, GrabbablePart otherPart, HexMetrics.Direction rotationFactor)
	{
//		relativeDirection = (HexMetrics.Direction)(((int)relativeDirection+(int)rotationFactor+6)%6);
		
		if (otherPart == null) return false;
		
		HexMetrics.Direction oppositeDirection = OtherPartsOppositeDirection(relativeDirection, otherPart);
		oppositeDirection = (HexMetrics.Direction)(((int)oppositeDirection+(int)rotationFactor+6)%6); // what if we were to rotate the other part?
//		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[(int)oppositeDirection];
		
		bool weldableHere = Weldable(relativeDirection);
		bool weldabelThere = otherPart.Weldable(oppositeDirection);
//		Debug.Log("Checking weldability: "+direction+"("+weldableHere+") <-> "+oppositeDirection+"("+weldabelThere+")");
		
		return weldableHere && weldabelThere;
	}
	
	
	public bool ConnectPartOnGrid(GrabbablePart otherPart, PhysicalConnectionType connectionType)
	{
		if (connectionType == PhysicalConnectionType.None)
		{
//			Debug.Log("Not Connecting: "+this.idNumber+" : "+otherPart.idNumber);
			return false;
		}
		
//		Debug.Log("Connecting: "+this.idNumber+" : "+otherPart.idNumber);
		
//		HexMetrics.Direction directionToOther;
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			IntVector2 relativeLocation = HexMetrics.GetGridOffset(iDir);
			
//			Debug.Log (GetGridLocationFromPosition().ToString() +" : "+otherPart.GetGridLocationFromPosition().ToString() + " + "+relativeLocation.ToString() + " = "+ (otherPart.GetGridLocationFromPosition() + relativeLocation).ToString());
			if (GetGridLocationFromPosition().IsEqualTo(otherPart.GetGridLocationFromPosition() - relativeLocation))
			{
				HexMetrics.Direction relativeDirection = Relative(iDir);
				int r = (int)relativeDirection;
//				Debug.Log ("Found Direction A: "+iDir+", R: "+relativeDirection);
				if (_connectedParts[r] != null && _connectedParts[r].connectedPart != null)
				{
					// if this happens twice in a step, remember that it is actually happening over 2 steps. 
					// It weld at the beginning of each step, i.e. as it comes into the welder and as it leaves
//					Debug.Log("Connecting part "+otherPart.idNumber+" to a direction that is already connected (connected to "+_connectedParts[r].connectedPart.idNumber+")");
					return false;
				}
//				otherPart.gameObject.transform.parent = gameObject.transform;
				
				if (IsWeldable (relativeDirection, otherPart))
				{
					if (ParentConstruction != null)
					{
						ParentConstruction.AddToConstruction(otherPart);
					}
				
					_connectedParts[r].Reset();
					_connectedParts[r].connectedPart = otherPart;
					HexMetrics.Direction oppositeDirection = ConnectedsOpposite(relativeDirection);
					otherPart._connectedParts[(int)oppositeDirection].Reset();
					otherPart._connectedParts[(int)oppositeDirection].connectedPart = this;
					SetPhysicalConnection(relativeDirection, connectionType);
					return true;
				}
				
				
				
			}
		}
		
		return false;
		
	}
	
	public int SimulationRotationDifference(HexMetrics.Direction other)
	{
		return RotationDifference(SimulationOrientation, other);
	}
	
	public static int RotationDifference(HexMetrics.Direction a)
	{
		return RotationDifference(a, HexMetrics.Direction.Up);
	}
	public static int RotationDifference(HexMetrics.Direction a, HexMetrics.Direction b)
	{
		HexMetrics.Direction diff = (HexMetrics.Direction)((int)a-(int)b);
		return (((int)diff+3)%6)-3;
	}
	
	// NOTE we are not using this because if side are not connected then it's connectedPart MUST be null!
//	public void ConnectUnconnectedParts()
//	{
////		List<GrabbablePart> connection1 = new List<GrabbablePart>();
////		List<GrabbablePart> connection2 = new List<GrabbablePart>();
////		List<IntVector2> location1 = new List<IntVector2>();
////		List<IntVector2> location2 = new List<IntVector2>();
////		List<HexMetrics.Direction> connectionDirection = new List<HexMetrics.Direction>();
//		
//		Dictionary<IntVector2, GrabbablePart> partDictionary = new Dictionary<IntVector2, GrabbablePart>(new IntVector2.IntVectorEqualityComparer ());
//		foreach(var locatedPart in GetAllConnectedPartsWithLocation())
//		{
////			GrabbablePart thisPart = locatedPart.part;
//			partDictionary[locatedPart.location] = locatedPart.part;
//		}
//		
//		foreach (IntVector2 partLocation in partDictionary.Keys)
//		{
//			GrabbablePart part = partDictionary[partLocation];
//			for (int i = 0 ; i < 6 ; i++)
//			{
//				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
//				IntVector2 otherLocation = partLocation + HexMetrics.GetGridOffset(part.Absolute(iDir));
//				if (partDictionary.ContainsKey(otherLocation)) // there is a part in this direction
//				{
//					Debug.Log("Found at A:"+Absolute(iDir)+", R:"+iDir +" @ "+partLocation.x+":"+partLocation.y);
//					GrabbablePart otherPart = partDictionary[otherLocation];
//					
//					Debug.Log("Connected already in "+iDir+" -> "+part.GetConnectedPart(iDir));
//					if (part.GetConnectedPart(iDir) == null)
//					{
//						Debug.Log("Connecting "+part+":"+otherPart +" in "+iDir);
//						
//						part._connectedParts[(int)iDir].connectedPart = otherPart;
//					}
//				}
//				else
//				{
//					part.RemoveConnectedPart(iDir);
//				}
//			}
//		}
//		
////		for (int i = 0 ; i < connection1.Count ; i++)
////		{
////			Debug.Log ("Connection "+i+" "+location1[i].x+":"+location1[i].y+" to "+location2[i].x+":"+location2[i].y +" ("+connectionDirection[i]+")");
////		}
//	}
	
#region Connected parts
	public IEnumerable<GrabbablePart> GetAllConnectedParts()
	{
		return GetAllConnectedParts(new HashSet<GrabbablePart>());

	}
	
	private IEnumerable<GrabbablePart> GetAllConnectedParts(HashSet<GrabbablePart> exploredParts)
	{
		if (exploredParts.Contains(this))
		{
			yield break;
		}
		exploredParts.Add(this);
		
		yield return this;
//		Debug.Log ("adding "+thisLocation.x+":"+thisLocation.y);
		
		for (int i = 0 ; i < 6 ; i++)
		{
			GrabbablePart connectedPart = _connectedParts[i].connectedPart;
			if (connectedPart != null && _connectedParts[i].connectionType != PhysicalConnectionType.None)
			{
				HexMetrics.Direction relativeDirection = (HexMetrics.Direction)i;
				HexMetrics.Direction absoluteDirection = AbsoluteDirectionFromRelative(relativeDirection);
//				IntVector2 newLocation = HexMetrics.GetGridOffset(absoluteDirection) + thisLocation;
//				Debug.Log (thisLocation.x+":"+thisLocation.y+" -> "+newLocation.x+":"+newLocation.y+" (A:"+absoluteDirection+", R:"+relativeDirection+")");
				foreach (GrabbablePart fromPart in connectedPart.GetAllConnectedParts(exploredParts))
				{
					yield return fromPart;
				}
			}
		}
	}
	
	
	public IEnumerable<LocatedPart> GetAllConnectedPartsWithLocation()
	{
		return GetAllConnectedPartsWithLocation(IntVector2.zero, new HashSet<GrabbablePart>());
//		foreach (LocatedPart loactedPart in RootPart.GetAllPartWithLocationFromThisPart(IntVector2.zero, new HashSet<GrabbablePart>()))
//		{
//			yield return loactedPart;
//		}
	}
	
	private IEnumerable<LocatedPart> GetAllConnectedPartsWithLocation(IntVector2 thisLocation, HashSet<GrabbablePart> exploredParts)
	{
		if (exploredParts.Contains(this))
		{
			yield break;
		}
		exploredParts.Add(this);
		
		yield return new LocatedPart(this, thisLocation);
//		Debug.Log ("adding "+thisLocation.x+":"+thisLocation.y);
		
		for (int i = 0 ; i < 6 ; i++)
		{
			GrabbablePart connectedPart = _connectedParts[i].connectedPart;
			if (connectedPart != null && _connectedParts[i].connectionType != PhysicalConnectionType.None)
			{
				HexMetrics.Direction relativeDirection = (HexMetrics.Direction)i;
				HexMetrics.Direction absoluteDirection = AbsoluteDirectionFromRelative(relativeDirection);
				IntVector2 newLocation = HexMetrics.GetGridOffset(absoluteDirection) + thisLocation;
//				Debug.Log (thisLocation.x+":"+thisLocation.y+" -> "+newLocation.x+":"+newLocation.y+" (A:"+absoluteDirection+", R:"+relativeDirection+")");
				foreach (LocatedPart loactedPart in connectedPart.GetAllConnectedPartsWithLocation(newLocation, exploredParts))
				{
					yield return loactedPart;
				}
			}
		}
	}
	
#endregion
	
#region Encodign and Decoding
	
	
	public IEnumerable<IEncodable> EncodeWithContext(Dictionary<GrabbablePart, int> partIDs)
	{
		int id = partIDs[this];
		
		yield return (EncodableInt)id;
		yield return (EncodableInt)(int)partType;
		yield return (EncodableInt)(int)SimulationOrientation;
		
//		string code = ""+CharSerializer.ToCode()+CharSerializer.ToCode((int)partType)+CharSerializer.ToCode((int)SimulationOrientation);
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
			GrabbablePart connPart = GetConnectedPart(iDir);
			int connPartID = connPart == null ? 0 : partIDs[connPart];
			int physicalConnType = (int)GetPhysicalConnectionType(iDir);
			int auxilaryConnType = (int)GetAuxilaryConnectionTypes(iDir);
			if (physicalConnType == (int)PhysicalConnectionType.None) // i.e. None
			{
				connPartID = 0;
				auxilaryConnType = 0;
			}
			
			yield return (EncodableInt)connPartID;
			yield return (EncodableInt)(int)physicalConnType;
			yield return (EncodableInt)(int)auxilaryConnType;
//			code += ""+
//					CharSerializer.ToCode(connPartID)+
//					CharSerializer.ToCode((int)physicalConnType)+
//					CharSerializer.ToCode((int)auxilaryConnType);
		}
		
//		return code;
	}
	
	
//	public void Decode(string code, Dictionary<int, GrabbablePart> idParts)
//	{
//		ConstructionElement constructionElement = new ConstructionElement();
//		constructionElement.id          =                       CharSerializer.ToNumber(code[0]);
//		constructionElement.partType    =             (PartType)CharSerializer.ToNumber(code[1]);
//		SimulationOrientation = (HexMetrics.Direction)CharSerializer.ToNumber(code[2]);
//		for (int i = 0 ; i < 6 ; i++)
//		{
//			constructionElement.connectedParts[i]         =                                       CharSerializer.ToNumber(code[3+(i*3)+0]);
//			constructionElement.physicalConnectionType[i] = (GrabbablePart.PhysicalConnectionType)CharSerializer.ToNumber(code[3+(i*3)+1]);
//			constructionElement.auxilaryConnectionType[i] =                                       CharSerializer.ToNumber(code[3+(i*3)+2]);
//		}
//		
//		return constructionElement;
//		return null;
//	}
	


#endregion
	
	void Awake()
	{
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction absoluteIDir = (HexMetrics.Direction)i;
			
			if (Weldable(i))
			{
				_weldSpriteObjects[i] = Instantiate(GameSettings.instance.weldPrefab) as GameObject;//ObjectPoolManager.GetObject(GameSettings.instance.weldPrefab);
				_weldSpriteObjects[i].transform.parent = transform;
				_weldSpriteObjects[i].transform.localPosition = Vector3.zero;
				_weldSpriteObjects[i].transform.localRotation = Quaternion.Euler(0,0,-60*i);
				_weldSpriteObjects[i].transform.localScale = Vector3.zero;
			}
		}
	}
	
	
	#region IPooledObject implementation
	public void OnPoolActivate ()
	{
	}

	public void OnPoolDeactivate ()
	{
		highlighted = false;
		for (int i = 0 ; i < 6 ; i++)
		{
			_connectedParts[i].Reset();
		}
		
	}
	#endregion
	
	void Start ()
	{
		_sphereCollider = gameObject.GetComponentsInChildren<Collider>()[0] as SphereCollider;
	}

	void Update ()
	{
	}
	
//	public override void PlaceAtLocation(IntVector2 location)
//	{
//		Location = location;
//		if (location != null)
//		{
//			hexCell = GridManager.instance.GetHexCell(location);
//			if (hexCell != null)
//			{
//				transform.position = hexCell.transform.position;
//				hexCell.partOnCell = this;
//			}
//		}
//		else
//		{
//			if (hexCell != null)
//			{
//				if (hexCell.partOnCell == this)
//				{
//					hexCell.partOnCell = null;
//				}
//				hexCell = null;
//			}
//		}
//	}

//	public void CheckForFinish ()
//	{
//		hexCell = Location == null ? null : GridManager.instance.GetHexCell(Location);
//		
//		if (hexCell != null && hexCell.finishCell)
//		{
//			// check if it is the right thing
//			bool isCorrectConstruction = true;
//			if (isCorrectConstruction)
//			{
//				PlaceAtLocation(null);
//				
//				finished = true;
//				Destroy(this.gameObject);
//				GameManager.instance.AddCompletedConstruction();
//				return;
//			}
//			GameManager.instance.IncompleteConstruction();
//		}
//	}
	
	
	
	public GrabbablePart CheckForCollisions()
	{
		if (_sphereCollider == null)
		{
			return null;
		}
		Collider [] colliders = Physics.OverlapSphere(_sphereCollider.transform.position + _sphereCollider.center, _sphereCollider.radius, 1<<gameObject.layer);
		
		foreach(Collider c in colliders)
		{
			if (c.attachedRigidbody.GetComponent<GrabbablePart>().IsFinished)
			{
				continue;
			}
			if (c.attachedRigidbody == _sphereCollider.attachedRigidbody)
			{
				continue;
			}
			
			GrabbablePart collidedPart = c.attachedRigidbody.gameObject.GetComponent<GrabbablePart>();
			if (collidedPart != null)
			{
				return collidedPart;
			}
		}
		
		return null;
	}

	#region ITriggerDelegateReceiver implementation
	public void OnTriggerEnter (Collider other)
	{
//		Debug.Log("OnTriggerEnter (Collider "+other+")");
		
		if (other.attachedRigidbody == null)
			return;
		
		GrabbablePart otherPart = other.attachedRigidbody.gameObject.GetComponent<GrabbablePart>();
//		foreach(Component c in other.gameObject.GetComponents<MonoBehaviour>())
//		{
//			Debug.Log (c);
//		}
		if (otherPart != null)
		{
			
			if ((          ParentConstruction == null ||           ParentConstruction.ignoreCollisions) ||
				(otherPart.ParentConstruction == null || otherPart.ParentConstruction.ignoreCollisions))
			{
				return;
			}
		
			LevelManager.instance.PartCollisionOccured(this, otherPart);
		}
	}
	#endregion
}
