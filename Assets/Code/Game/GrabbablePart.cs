using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PartType {None, Standard6Sided, Standard3Sided}

public enum Symmetry {None, TwoWay, ThreeWay, SixWay}

public class GrabbablePart : MonoBehaviour
{
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
			List<Transform> children = new List<Transform>();
			for (int i = 0 ; i < 6 ; i++)
			{
				GrabbablePart childPart = _connectedParts[i].connectedPart;
				if (childPart != null && childPart.transform != transform.parent && childPart.transform.parent == transform) 
				{
					children.Add(childPart.transform);
				}
			}
			
			children.ForEach((obj) => obj.parent = null);
			int directionChange = ((int)orientation - (int)SimulationOrientation + 6)%6;
			transform.rotation = Quaternion.Euler(0, 0, (int)orientation * -60);
			
			ConnectionDescription [] newParts = new ConnectionDescription [6];
			PhysicalConnectionType [] physicalConnections = new PhysicalConnectionType [6];
			int [] auxilaryConnections = new int [6];
			
			children.ForEach((obj) => obj.parent = transform);
			
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
	
	public Grabber heldAndMovingGrabber { get; set; }
	
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
		
		if ( transform.parent != null && ret != null && ret.transform == transform.parent )
		{
			transform.parent = null;
		}
		_connectedParts[(int)absoluteDirection].connectedPart = null;
		_connectedParts[(int)absoluteDirection].connectionType = PhysicalConnectionType.None;
		_connectedParts[(int)absoluteDirection].auxConnectionTypes = 0;
		
		return ret;
	}
	
	public void ConnectPartAndPlaceAtRelativeDirection(GrabbablePart otherPart, HexMetrics.Direction relativeDirection)
	{
		HexMetrics.Direction absoluteDirection = Absolute(relativeDirection);
		ConnectionDescription connDesc = _connectedParts[(int)relativeDirection];
		
		connDesc.connectedPart = otherPart; // connect the part
		
		ConnectionDescription otherConnDesc = otherPart._connectedParts[(int)ConnectedsOpposite(relativeDirection)]; // get the other parts opposite side that is connected to this
		
		otherConnDesc.connectedPart = this; // connect that side
		
//		if (!placeOtherPart)
//		{
//			return;
//		}
		
		connDesc.connectedPart.transform.parent = transform;
		
		connDesc.connectedPart.transform.position = transform.position + (Vector3)GameSettings.instance.hexCellPrefab.GetDirection(absoluteDirection);
//		connDesc.connectedPart.transform.localRotation = Quaternion.Euler(0, 0, 0);//( (int)simulationDirection + (int)newOrientation ) * -60);
		
		
		// we have to connect parts that are adjacent
		
		ConnectUnconnectedParts();
	}
	
	
	private void ConnectUnconnectedParts()
	{
//		List<GrabbablePart> connection1 = new List<GrabbablePart>();
//		List<GrabbablePart> connection2 = new List<GrabbablePart>();
//		List<IntVector2> location1 = new List<IntVector2>();
//		List<IntVector2> location2 = new List<IntVector2>();
//		List<HexMetrics.Direction> connectionDirection = new List<HexMetrics.Direction>();
		
		Dictionary<IntVector2, GrabbablePart> partDictionary = new Dictionary<IntVector2, GrabbablePart>(new IntVector2.IntVectorEqualityComparer ());
		foreach(var locatedPart in GetAllPartsWithLocation())
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
			
			Debug.Log (GetGridLocationFromPosition().ToString() +" : "+otherPart.GetGridLocationFromPosition().ToString() + " + "+relativeLocation.ToString() + " = "+ (otherPart.GetGridLocationFromPosition() + relativeLocation).ToString());
			if (GetGridLocationFromPosition().IsEqualTo(otherPart.GetGridLocationFromPosition() - relativeLocation))
			{
				HexMetrics.Direction relativeDirection = Relative(iDir);
				int r = (int)relativeDirection;
				Debug.Log ("Found Direction A: "+iDir+", R: "+relativeDirection);
				if (_connectedParts[r] != null && _connectedParts[r].connectedPart != null)
				{
					Debug.Log("Connecting part "+otherPart.idNumber+" to a direction that is already connected");
					return false;
				}
				otherPart.gameObject.transform.parent = gameObject.transform;
				
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
	
	//=========================================================================================
	#region Construction tree traveral and access
	
	IEnumerable<GrabbablePart> GetAllChildConnectedParts(HashSet<GrabbablePart> exploredParts)
	{
		
		if (exploredParts.Contains(this))
		{
			yield break;
		}
		exploredParts.Add(this);
		
		yield return this;
		// else, this is the root
		foreach (ConnectionDescription connectionDesc in _connectedParts)
		{
			if (connectionDesc != null && 
				connectionDesc.connectedPart != null && 
				connectionDesc.connectedPart != ParentPart)
			{
				
				foreach (GrabbablePart part in connectionDesc.connectedPart.GetAllChildConnectedParts(exploredParts))
				{
					yield return part;
				}
			}
		}
	}
	
		
	public IEnumerable<GrabbablePart> GetAllConnectedPartsFromRoot()
	{
		return RootPart.GetAllChildConnectedParts(new HashSet<GrabbablePart>());
	}
	
	
	public GrabbablePart ParentPart
	{
		get 
		{
			Transform parentTransform = gameObject.transform.parent;
			GameObject parentGO = ( parentTransform == null ? null : parentTransform.gameObject );
			GrabbablePart parentPart = ( parentGO == null ? null : parentGO.GetComponent<GrabbablePart>() );
			
			if (parentPart != null)
			{
				return parentPart;
			}
			return null;
		}
	}
	
	public GrabbablePart RootPart
	{
		get 
		{
			GrabbablePart parentPart = ParentPart;
			if (parentPart == null)
			{
				return this;
			}
			else
			{
				return parentPart.RootPart;
			}
		}
	}
	
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
	
	public IEnumerable<LocatedPart> GetAllPartsWithLocation()
	{
		foreach (LocatedPart loactedPart in RootPart.GetAllPartWithLocationFromThisPart(IntVector2.zero, new HashSet<GrabbablePart>()))
		{
			yield return loactedPart;
		}
	}
	
	private IEnumerable<LocatedPart> GetAllPartWithLocationFromThisPart(IntVector2 thisLocation, HashSet<GrabbablePart> exploredParts)
	{
		if (exploredParts.Contains(this))
		{
			yield break;
		}
		exploredParts.Add(this);
		
		yield return new LocatedPart(this, thisLocation);
//		Debug.Log ("adding "+thisLocation.x+":"+thisLocation.y);
		
		GrabbablePart parentPart = ParentPart;
		
		for (int i = 0 ; i < 6 ; i++)
		{
			GrabbablePart connectedPart = _connectedParts[i].connectedPart;
			if (connectedPart != null && connectedPart != parentPart)
			{
				HexMetrics.Direction relativeDirection = (HexMetrics.Direction)i;
				HexMetrics.Direction absoluteDirection = AbsoluteDirectionFromRelative(relativeDirection);
				IntVector2 newLocation = HexMetrics.GetGridOffset(absoluteDirection) + thisLocation;
//				Debug.Log (thisLocation.x+":"+thisLocation.y+" -> "+newLocation.x+":"+newLocation.y+" (A:"+absoluteDirection+", R:"+relativeDirection+")");
				foreach (LocatedPart loactedPart in connectedPart.GetAllPartWithLocationFromThisPart(newLocation, exploredParts))
				{
					yield return loactedPart;
				}
			}
		}
	}
	
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
