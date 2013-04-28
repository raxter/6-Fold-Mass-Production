using UnityEngine;
using System.Collections;


public class GrabbablePart : HexCellPlaceable
{
	SphereCollider _sphereCollider;
	
	bool finished = false;
	
	public IntVector2 heldOverLocation;
	
	public IntVector2 OverLocation { get { return Location ?? heldOverLocation; } }
	
	public int idNumber = -1;
	
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
	
	ConnectionDescription [] connectedParts = new ConnectionDescription [6];
	
	
	public GrabbablePart RootPart
	{
		get 
		{
			Transform parentTransform = gameObject.transform.parent;
			GameObject parentGO = ( parentTransform == null ? null : parentTransform.gameObject );
			if (parentGO != null)
			{
				GrabbablePart parentPart = parentGO.GetComponent<GrabbablePart>();
				if (parentPart != null)
				{
					return parentPart.RootPart;
				}
			}
			return this;
		}
	}
	
	public bool ConnectPart(GrabbablePart otherPart)
	{
		Debug.Log("Connecting: "+this+" : "+otherPart);
		ConnectionDescription connectionDesc = null;
		bool foundDirection = false;
//		HexMetrics.Direction directionToOther;
		for (int i = 0 ; i < 6 ; i++)
		{
			HexMetrics.Direction dir = (HexMetrics.Direction)i;
			IntVector2 relativeLocation = HexMetrics.GetRelativeLocation(dir);
			
			if (OverLocation == null || relativeLocation == null)
			{
				Debug.LogError ("Arg!");
			}
			IntVector2 offsetLocation = OverLocation + relativeLocation;
			if (offsetLocation.IsEqualTo(otherPart.OverLocation)) // part might not be dropped, must use an over location
			{
				foundDirection = true;
//				directionToOther = (HexMetrics.Direction)i;
				if (connectedParts[i] == null)
				{
					connectedParts[i] = new ConnectionDescription();
				}
				else
				{
					Debug.Log("Connecting part "+otherPart+" to a direction that is already connected");
					return false;
				}
				connectionDesc = connectedParts[i];
				break;
			}
		}
		if (!foundDirection) return false;
		
		otherPart.gameObject.transform.parent = gameObject.transform;
//		connectionDesc.joint = gameObject.AddComponent<FixedJoint>();
//		connectionDesc.joint.connectedBody = otherPart.gameObject.rigidbody;
		connectionDesc.connectionType = PhysicalConnectionType.Weld;
		
		return true;
	}
	
	
	#region implemented abstract members of HexCellPlaceable
	protected override void PlaceableStart ()
	{
		_sphereCollider = gameObject.GetComponentsInChildren<Collider>()[0] as SphereCollider;
	}

	protected override void PlaceableUpdate ()
	{
	}
	#endregion
	
	// TODO place children of root's object rather than just this one!
	public override void PlaceAtLocation(IntVector2 location)
	{
		Location = location;
		if (location != null)
		{
			hexCell = GridManager.instance.GetHexCell(location);
			if (hexCell != null)
			{
				transform.position = hexCell.transform.position;
				hexCell.partOnCell = this;
			}
		}
		else
		{
			if (hexCell != null)
			{
				if (hexCell.partOnCell == this)
				{
					hexCell.partOnCell = null;
				}
				hexCell = null;
			}
		}
	}

	public void CheckForFinish ()
	{
		hexCell = Location == null ? null : GridManager.instance.GetHexCell(Location);
		
		if (hexCell != null && hexCell.finishCell)
		{
			// check if it is the right thing
			bool isCorrectConstruction = true;
			if (isCorrectConstruction)
			{
				PlaceAtLocation(null);
				
				finished = true;
				Destroy(this.gameObject);
				GameManager.instance.AddCompletedConstruction();
				return;
			}
			GameManager.instance.IncompleteConstruction();
		}
	}
	
	
	
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
