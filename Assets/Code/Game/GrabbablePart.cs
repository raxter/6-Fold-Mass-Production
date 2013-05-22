using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PartType {None, Standard6Sided}

public class GrabbablePart : MonoBehaviour
{
	SphereCollider _sphereCollider;
	
	public PartType partType;
	
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

	public void RegisterLocationFromPosition ()
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
		
//		Debug.Log ("registering at "+x+":"+oy);
		//find hex cell, register
		GridManager.instance.GetHexCell(new IntVector2(x,y)).RegisterPart(this);
	}
	
	
	public int idNumber = -1;
	
	bool finished = false;
	
	public bool IsFinished { get { return finished; } }
	
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
	
	
	
	public GrabbablePart GetConnectedPart(HexMetrics.Direction inDirection)
	{
		return _connectedParts[(int)inDirection].connectedPart;
	}
	
	public GrabbablePart RemoveConnectedPart(HexMetrics.Direction inDirection)
	{
		GrabbablePart ret = _connectedParts[(int)inDirection].connectedPart;
		_connectedParts[(int)inDirection].connectedPart = null;
		_connectedParts[(int)inDirection].connectionType = PhysicalConnectionType.None;
		_connectedParts[(int)inDirection].auxConnectionTypes = 0;
		
		return ret;
	}
	
//	public void ConnectPartAndPlace(GrabbablePart otherPart, HexMetrics.Direction direction)
//	{
//		ConnectPartAndPlace(otherPart, direction, true);
//	}
	public void ConnectPartAndPlace(GrabbablePart otherPart, HexMetrics.Direction direction)
	{
		ConnectionDescription connDesc = _connectedParts[(int)direction];
		ConnectionDescription otherConnDesc = otherPart._connectedParts[((int)direction+3)%6];
		
		connDesc.connectedPart = otherPart;
		otherConnDesc.connectedPart = this;
		
//		if (!placeOtherPart)
//		{
//			return;
//		}
		
		connDesc.connectedPart.transform.parent = transform;
		
		connDesc.connectedPart.transform.position = transform.position + (Vector3)GameSettings.instance.hexCellPrefab.GetDirection(direction);
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
				HexMetrics.Direction iDir = (HexMetrics.Direction)((i+(int)SimulationOrientation)%6);
				IntVector2 otherLocation = partLocation + HexMetrics.GetRelativeLocation(iDir);
				if (partDictionary.ContainsKey(otherLocation)) // there is a part in this direction
				{
					GrabbablePart otherPart = partDictionary[otherLocation];
					
					Debug.Log("Connected already in "+iDir+" -> "+part.GetConnectedPart(iDir));
					if (part.GetConnectedPart(iDir) == null)
					{
						Debug.Log("Connecting "+part+":"+otherPart +" in "+iDir);
						
//						connection1.Add(part);
//						connection2.Add(otherPart);
//						location1.Add(partLocation);
//						location2.Add(otherLocation);
//						connectionDirection.Add(iDir);
						part._connectedParts[(int)iDir].connectedPart = otherPart;
//						ConnectionDescription otherConnDesc = otherPart._connectedParts[((int)iDir+3)%6];
		
//						connDesc.connectedPart = otherPart;
//						otherConnDesc.connectedPart = part;
						//part.ConnectPartAndPlace(otherPart, iDir, false);
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
	
	public void SetAbsoluteOrientation (HexMetrics.Direction orientation)
	{
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
		
		children.ForEach((obj) => obj.parent = transform);
		
		for (int i = 0 ; i < 6 ; i++)
		{
			newParts[i] = _connectedParts[(i+directionChange)%6];
		}
		for (int i = 0 ; i < 6 ; i++)
		{
			_connectedParts[i] = newParts[i];
		}
	}
	
	
	public PhysicalConnectionType GetConnectionType(HexMetrics.Direction inDirection)
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
		if (_connectedParts[(int)direction].connectedPart == null)
		{
			connDesc.connectionType = newConnectionType;
			return;
		}
		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[((int)direction+3)%6];
		
		connDesc.connectionType = newConnectionType;
		otherConnDesc.connectionType = newConnectionType;
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
		if (_connectedParts[(int)direction].connectedPart == null)
		{
			_connectedParts[(int)direction].auxConnectionTypes = 0;
			return;
		}
		ConnectionDescription connDesc = _connectedParts[(int)direction];
		ConnectionDescription otherConnDesc = connDesc.connectedPart._connectedParts[((int)direction+3)%6];
		
		connDesc.auxConnectionTypes = newConnectionTypes;
		otherConnDesc.auxConnectionTypes = newConnectionTypes;
	}
	
	public HexMetrics.Direction GetAbsoluteDirection(HexMetrics.Direction direction)
	{
		return (HexMetrics.Direction)( ((int)direction + 6 - (int)SimulationOrientation) % 6);
	}
	
//	public HexMetrics.Direction orientation { get { return _orientation; } }
//	HexMetrics.Direction _orientation;
	
	
	public bool ConnectPartOnGrid(GrabbablePart otherPart)
	{
		Debug.Log("Connecting: "+this.idNumber+" : "+otherPart.idNumber);
//		ConnectionDescription connectionDesc = null;
//		ConnectionDescription otherConnectionDesc = null;
		
//		HexMetrics.Direction directionToOther;
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction dir = (HexMetrics.Direction)i;
			IntVector2 relativeLocation = HexMetrics.GetRelativeLocation(dir);
			
//			if (OverLocation == null || relativeLocation == null)
//			{
//				Debug.LogError ("Arg!");
//			}
//			IntVector2 offsetLocation = OverLocation + relativeLocation;
//			if (offsetLocation.IsEqualTo(otherPart.OverLocation)) // part might not be dropped, must use an over location
			{
//				directionToOther = (HexMetrics.Direction)i;
				if (_connectedParts[i] != null && _connectedParts[i].connectedPart != null)
				{
					Debug.Log("Connecting part "+otherPart.idNumber+" to a direction that is already connected");
					return false;
				}
				otherPart.gameObject.transform.parent = gameObject.transform;
				
				_connectedParts[i] = new ConnectionDescription();
				_connectedParts[i].connectedPart = otherPart;
				_connectedParts[i].connectionType = PhysicalConnectionType.Weld;
				
				otherPart._connectedParts[(i+3)%6] = new ConnectionDescription();
				otherPart._connectedParts[(i+3)%6].connectedPart = this;
				otherPart._connectedParts[(i+3)%6].connectionType = PhysicalConnectionType.Weld;
				
				return true;
			}
		}
		
		return false;
		
	}
	
	IEnumerable<GrabbablePart> GetAllChildConnectedParts(HashSet<GrabbablePart> exploredParts)
	{
//		HashSet<GrabbablePart> exploredParts = new HashSet<GrabbablePart>();
//		
//		Queue<GrabbablePart> queue = new Queue<GrabbablePart>();
//		queue.Enqueue(this);
//		
//		while (queue.Count > 0)
//		{
//			GrabbablePart qPart = queue.Dequeue();
//			
//			yield return qPart;
//			
//			exploredParts.Add(qPart);
//			
//			for(int i = 0 ; i < 6 ; i++)
//			{
//				HexMetrics.Direction iDir = (HexMetrics.Direction)i;
//				
//				GrabbablePart partInDirection = qPart.GetConnectedPart(iDir);
//				
//				if (!exploredParts.Contains(partInDirection))
//				{
//					queue.Enqueue(qPart);
//				}
//			}
//		}
		
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
	
	public HexMetrics.Direction SimulationOrientation
	{
		get 
		{
			int directionInt = (int)((-transform.rotation.eulerAngles.z-1)/60);
			directionInt = (directionInt + 6) % 6;
			return (HexMetrics.Direction)directionInt;
		}
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
	
	
//	public IntVector2 RootLocation
//	{
//		get 
//		{
//			GrabbablePart parentPart = ParentPart;
//			if (parentPart == null)
//			{
//				return this.OverLocation;
//			}
//			else
//			{
//				for (int i = 0 ; i < 6 ; i++)
//				{
//					ConnectionDescription connectionDesc = connectedParts[i];
//					if (connectionDesc != null && connectionDesc.connectedPart == parentPart)
//					{
//						HexMetrics.Direction thisOrientation = SimulationOrientation;
//						
//						HexMetrics.Direction combinedDirection = (HexMetrics.Direction)(((int)thisOrientation + i)%6);
//						
//						return HexMetrics.GetRelativeLocation(combinedDirection) + parentPart.RootLocation;
//					}
//				}
//				return null;
//			}
//		}
//	}
	
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
		
		GrabbablePart parentPart = ParentPart;
		
		for (int i = 0 ; i < 6 ; i++)
		{
			GrabbablePart connectedPart = _connectedParts[i].connectedPart;
			if (connectedPart != null && connectedPart != parentPart)
			{
				HexMetrics.Direction absoluteDirection = GetAbsoluteDirection((HexMetrics.Direction)i);
				
				foreach (LocatedPart loactedPart in connectedPart.GetAllPartWithLocationFromThisPart(HexMetrics.GetRelativeLocation(absoluteDirection) + thisLocation, exploredParts))
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
