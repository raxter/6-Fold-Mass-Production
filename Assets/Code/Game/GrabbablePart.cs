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

public class GrabbablePart : MonoBehaviour
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
		GridManager.instance.GetHexCell(GetGridLocationFromPosition()).RegisterPart(this);
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
	
	public HexMetrics.Direction SimulationOrientation
	{
		get 
		{
			float epsilon = 1;
			int directionInt = (int)((-transform.rotation.eulerAngles.z-epsilon)/60);
			directionInt = (directionInt + 6) % 6;
			return (HexMetrics.Direction)directionInt;
		}
		set
		{
			HexMetrics.Direction orientation = value;
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
	//				PhysicalConnectionType oldConnection = _connectedParts[i].connectionType;
	//				int oldAuxConnection = _connectedParts[i].auxConnectionTypes;
	//				Debug.Log(((HexMetrics.Direction)i)+":"+oldConnection);
				_connectedParts[i] = newParts[i];
				
				if (_connectedParts[i].connectedPart == null)
				{
					physicalConnections[i] = PhysicalConnectionType.None;
					auxilaryConnections[i] = 0;
				}
	//				
				SetPhysicalConnection((HexMetrics.Direction)i, physicalConnections[i]);
				SetAuxilaryConnections((HexMetrics.Direction)i, auxilaryConnections[i]);
				
			}
		}
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
		
		return ReverseDirection( connectedPart.RelativeDirectionFromAbsolute(AbsoluteDirectionFromRelative(relativeDirection)));
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
	}
	
	
	[SerializeField]
	ConnectionDescription [] _connectedParts = new ConnectionDescription [6];
	
	#endregion
	
	#region Connection Access
	public GrabbablePart GetConnectedPart(HexMetrics.Direction absoluteDirection)
	{
		return _connectedParts[(int)absoluteDirection].connectedPart;
	}
	
	public GrabbablePart RemoveConnectedPart(HexMetrics.Direction absoluteDirection)
	{
		GrabbablePart ret = _connectedParts[(int)absoluteDirection].connectedPart;
		
//		if ( transform.parent != null && ret != null && ret.transform == transform.parent )
//		{
//			transform.parent = null;
//		}
		_connectedParts[(int)absoluteDirection].connectedPart = null;
		_connectedParts[(int)absoluteDirection].connectionType = PhysicalConnectionType.None;
		_connectedParts[(int)absoluteDirection].auxConnectionTypes = 0;
		
		return ret;
	}
	
	
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
			Debug.Log("Adding to construction "+ParentConstruction.name);
			ParentConstruction.AddToConstruction(otherPart);
		}
		
	}
	
	
	
	#endregion
	
	
	public PhysicalConnectionType GetPhysicalConnectionType(HexMetrics.Direction inDirection)
	{
		if (GetConnectedPart(inDirection) == null)
		{
			return PhysicalConnectionType.None;
		}
		return _connectedParts[(int)inDirection].connectionType;
	}
	
	public void SetPhysicalConnection(HexMetrics.Direction direction, PhysicalConnectionType newConnectionType)
	{
		ConnectionDescription connDesc = _connectedParts[(int)direction];
		if (connDesc.connectedPart == null)
		{
			connDesc.connectionType = PhysicalConnectionType.None;
			if (ParentConstruction != null)
			{
				ParentConstruction.CheckForSplit();
			}
			return;
		}
		HexMetrics.Direction oppositeDirection = ConnectedsOpposite(direction);
		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[(int)oppositeDirection];
		
		bool weldableHere = Weldable(direction);
		bool weldabelThere = connDesc.connectedPart.Weldable(oppositeDirection);
//		Debug.Log("Checking weldability: "+direction+"("+weldableHere+") <-> "+oppositeDirection+"("+weldabelThere+")");
		if (weldableHere && weldabelThere)
		{
			connDesc.connectionType = newConnectionType;
			otherConnDesc.connectionType = newConnectionType;
		}
		else
		{
			connDesc.connectionType = PhysicalConnectionType.None;
			otherConnDesc.connectionType = PhysicalConnectionType.None;
		}
		
		//if we are disconnecting the side (None) or if it's not weldable, make sure that the parts are not connected
		if (connDesc.connectionType == PhysicalConnectionType.None)
		{
			otherConnDesc.connectedPart = null;
			connDesc.connectedPart = null;
			
			// check that by disconnecting a side, we have not split the construction up
			ParentConstruction.CheckForSplit();
		}
	}
	
	public int GetAuxilaryConnectionTypes(HexMetrics.Direction direction)
	{
		if (GetConnectedPart(direction) == null)
		{
			return 0;
		}
		return _connectedParts[(int)direction].auxConnectionTypes;
	}
	
	public void SetAuxilaryConnections(HexMetrics.Direction direction, int newConnectionTypes)
	{
		ConnectionDescription connDesc = _connectedParts[(int)direction];
		if (connDesc.connectedPart == null)
		{
			connDesc.auxConnectionTypes = 0;
			return;
		}
		HexMetrics.Direction oppositeDirection = ConnectedsOpposite(direction);
		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[(int)oppositeDirection];
		
		bool weldableHere = Weldable(direction);
		bool weldabelThere = connDesc.connectedPart.Weldable(oppositeDirection);
//		Debug.Log("Checking weldability: "+direction+"("+weldableHere+") <-> "+oppositeDirection+"("+weldabelThere+")");
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
	
	
	public bool ConnectPartOnGrid(GrabbablePart otherPart, PhysicalConnectionType connectionType)
	{
		if (connectionType == PhysicalConnectionType.None)
		{
			Debug.Log("Not Connecting: "+this.idNumber+" : "+otherPart.idNumber);
			return false;
		}
		
		Debug.Log("Connecting: "+this.idNumber+" : "+otherPart.idNumber);
		
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
				Debug.Log ("Found Direction A: "+iDir+", R: "+relativeDirection);
				if (_connectedParts[r] != null && _connectedParts[r].connectedPart != null)
				{
					// if this happens twice in a step, remember that it is actually happening over 2 steps. 
					// It weld at the beginning of each step, i.e. as it comes into the welder and as it leaves
					Debug.Log("Connecting part "+otherPart.idNumber+" to a direction that is already connected (connected to "+_connectedParts[r].connectedPart.idNumber+")");
					return false;
				}
//				otherPart.gameObject.transform.parent = gameObject.transform;
				
				if (ParentConstruction != null)
				{
					ParentConstruction.AddToConstruction(otherPart);
				}
				
				_connectedParts[r] = new ConnectionDescription();
				_connectedParts[r].connectedPart = otherPart;
				HexMetrics.Direction oppositeDirection = ConnectedsOpposite(relativeDirection);
				otherPart._connectedParts[(int)oppositeDirection] = new ConnectionDescription();
				otherPart._connectedParts[(int)oppositeDirection].connectedPart = this;
				SetPhysicalConnection(relativeDirection, connectionType);
				
				return true;
			}
		}
		
		return false;
		
	}
	
	
	public void ConnectUnconnectedParts()
	{
//		List<GrabbablePart> connection1 = new List<GrabbablePart>();
//		List<GrabbablePart> connection2 = new List<GrabbablePart>();
//		List<IntVector2> location1 = new List<IntVector2>();
//		List<IntVector2> location2 = new List<IntVector2>();
//		List<HexMetrics.Direction> connectionDirection = new List<HexMetrics.Direction>();
		
		Dictionary<IntVector2, GrabbablePart> partDictionary = new Dictionary<IntVector2, GrabbablePart>(new IntVector2.IntVectorEqualityComparer ());
		foreach(var locatedPart in GetAllConnectedPartsWithLocation())
		{
//			GrabbablePart thisPart = locatedPart.part;
			partDictionary[locatedPart.location] = locatedPart.part;
		}
		
		foreach (IntVector2 partLocation in partDictionary.Keys)
		{
			GrabbablePart part = partDictionary[partLocation];
			for (int i = 0 ; i < 6 ; i++)
			{
				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
				IntVector2 otherLocation = partLocation + HexMetrics.GetGridOffset(part.Absolute(iDir));
				if (partDictionary.ContainsKey(otherLocation)) // there is a part in this direction
				{
					Debug.Log("Found at A:"+Absolute(iDir)+", R:"+iDir +" @ "+partLocation.x+":"+partLocation.y);
					GrabbablePart otherPart = partDictionary[otherLocation];
					
					Debug.Log("Connected already in "+iDir+" -> "+part.GetConnectedPart(iDir));
					if (part.GetConnectedPart(iDir) == null)
					{
						Debug.Log("Connecting "+part+":"+otherPart +" in "+iDir);
						
						part._connectedParts[(int)iDir].connectedPart = otherPart;
					}
				}
				else
				{
					part.RemoveConnectedPart(iDir);
				}
			}
		}
		
//		for (int i = 0 ; i < connection1.Count ; i++)
//		{
//			Debug.Log ("Connection "+i+" "+location1[i].x+":"+location1[i].y+" to "+location2[i].x+":"+location2[i].y +" ("+connectionDirection[i]+")");
//		}
	}
	
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
	
	public string Encode(Dictionary<GrabbablePart, int> partIDs)
	{
		
//			Debug.Log("Defining "+element.id);
//		HexMetrics.Direction orientation = part.SimulationOrientation;
//		
//		int [] connectedParts = new int [6];
//		GrabbablePart.PhysicalConnectionType [] physicalConnectionType = new GrabbablePart.PhysicalConnectionType [6];
//		int [] auxilaryConnectionType = new int [6];
//		
//		for(int i = 0 ; i < 6 ; i++)
//		{
//			HexMetrics.Direction iDir = (HexMetrics.Direction)i;
//			GrabbablePart connPart = part.GetConnectedPart(iDir);
//			element.connectedParts[i] = connPart == null ? 0 : partIDs[connPart];
//			element.physicalConnectionType[i] = part.GetPhysicalConnectionType(iDir);
//			element.auxilaryConnectionType[i] = part.GetAuxilaryConnectionTypes(iDir);
//			
//			if (element.physicalConnectionType[i] == GrabbablePart.PhysicalConnectionType.None)
//			{
//				element.connectedParts[i] = 0;
//				element.auxilaryConnectionType[i] = 0;
//			}
//		}
//		
		
		int id = partIDs[this];
		
		string code = ""+CharSerializer.ToCode(id)+CharSerializer.ToCode((int)partType)+CharSerializer.ToCode((int)SimulationOrientation);
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
			
			code += ""+
					CharSerializer.ToCode(connPartID)+
					CharSerializer.ToCode((int)physicalConnType)+
					CharSerializer.ToCode((int)auxilaryConnType);
		}
		
		return code;
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

	void Start ()
	{
		
		highlighted = false;
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
			GameManager.instance.PartCollisionOccured(this, otherPart);
		}
	}
	#endregion
}
