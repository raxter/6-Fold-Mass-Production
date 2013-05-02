using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GrabbablePart : MonoBehaviour
{
	SphereCollider _sphereCollider;
	
	
	
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
		
//		Debug.Log ("registering at "+x+":"+y);
		//find hex cell, register
		GridManager.instance.GetHexCell(new IntVector2(x,y)).RegisterPart(this);
	}
	
//	public IntVector2 _heldOverLocation;
//	public IntVector2 heldOverLocation
//	{
//		get
//		{
//			return _heldOverLocation;
//		}
//		set
//		{
//			if (value == null)
//			{
//				Debug.Log ("part "+idNumber+" no longer held over "+_heldOverLocation.x +":"+ _heldOverLocation.y);
//			}
//			_heldOverLocation = value;
//			if (_heldOverLocation != null)
//			{
//				Debug.Log ("part "+idNumber+" held over "+_heldOverLocation.x +":"+ _heldOverLocation.y);
//			}
//		}
//	}
//	
//	public IntVector2 OverLocation { get { return Location ?? heldOverLocation; } }
	
//	public IntVector2 OverLocation 
//	{ 
//		get 
//		{ 
//			return 
//		} 
//	}
	
	public int idNumber = -1;
	
	bool finished = false;
	
	public bool IsFinished { get { return finished; } }
	
	public enum PhysicalConnectionType {None = 0, Weld = 1, Magentic = 2};
	public enum AuxilaryConnectionType {None = 0, Electric = 1, Belt = 2};
	// weld is a normal connection, will physically keep the parts together
	// magnet is a connection with string magnets, will physically keep the parts together
	// wired means it is electrically connected (though not physically, just wired connections alone won't keep them together)
	// belt means that a rotary part is connected (engine, wheel, gun, anything operated by an engine)
	
	[System.Serializable]
	public class ConnectionDescription
	{
		public GrabbablePart connectedPart;
		
		
		public PhysicalConnectionType connectionType = GrabbablePart.PhysicalConnectionType.None;
	}
	
	protected ConnectionDescription [] connectedParts = new ConnectionDescription [6];
	
	
	
	public bool ConnectPart(GrabbablePart otherPart)
	{
		Debug.Log("Connecting: "+this+" : "+otherPart);
		ConnectionDescription connectionDesc = null;
		ConnectionDescription otherConnectionDesc = null;
		
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
				if (connectedParts[i] != null)
				{
					Debug.Log("Connecting part "+otherPart+" to a direction that is already connected");
					return false;
				}
				otherPart.gameObject.transform.parent = gameObject.transform;
				
				connectedParts[i] = new ConnectionDescription();
				connectedParts[i].connectedPart = otherPart;
				connectedParts[i].connectionType = PhysicalConnectionType.Weld;
				
				otherPart.connectedParts[(i+3)%6] = new ConnectionDescription();
				otherPart.connectedParts[(i+3)%6].connectedPart = this;
				otherPart.connectedParts[(i+3)%6].connectionType = PhysicalConnectionType.Weld;
				
				return true;
			}
		}
		
		return false;
		
	}
	
	IEnumerable<GrabbablePart> GetAllChildConnectedParts()
	{
		yield return this;
		// else, this is the root
		foreach (ConnectionDescription connectionDesc in connectedParts)
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
