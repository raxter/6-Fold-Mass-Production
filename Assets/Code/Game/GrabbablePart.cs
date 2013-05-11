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
	
	public void ConnectPartAndPlace(GrabbablePart otherPart, HexMetrics.Direction direction, HexMetrics.Direction newOrientation)
	{
		ConnectionDescription connDesc = _connectedParts[(int)direction];
		ConnectionDescription otherConnDesc = otherPart._connectedParts[((int)newOrientation+3)%6];
		
		connDesc.connectedPart = otherPart;
		otherConnDesc.connectedPart = this;
		
		connDesc.connectedPart.transform.parent = transform;
		HexMetrics.Direction simulationDirection = (HexMetrics.Direction)(( (int)SimulationOrientation + (int)direction ) % 6);
		
		connDesc.connectedPart.transform.position = transform.position + (Vector3)GameSettings.instance.hexCellPrefab.GetDirection(simulationDirection);
		connDesc.connectedPart.transform.localRotation = Quaternion.Euler(0, 0, ( (int)simulationDirection + (int)newOrientation ) * -60);
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
	
	IEnumerable<GrabbablePart> GetAllChildConnectedParts()
	{
		yield return this;
		// else, this is the root
		foreach (ConnectionDescription connectionDesc in _connectedParts)
		{
			if (connectionDesc != null && 
				connectionDesc.connectedPart != null && 
				connectionDesc.connectedPart != ParentPart)
			{
				
				foreach (GrabbablePart part in connectionDesc.connectedPart.GetAllChildConnectedParts())
				{
					yield return part;
				}
			}
		}
	}
		
	public IEnumerable<GrabbablePart> GetAllConnectedPartsFromRoot()
	{
		return RootPart.GetAllChildConnectedParts();
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
	
//	public delegate void PartAtLocation(GrabbablePart part, IntVector2 location);
//	
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
//				HexMetrics.Direction thisOrientation = SimulationOrientation;
//				
//				HexMetrics.Direction combinedDirection = (HexMetrics.Direction)(((int)thisOrientation + i)%6);
//				
//				WalkChildrenWithLocation(partAtLocationFunction, HexMetrics.GetRelativeLocation(combinedDirection) + thisLocation);
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
