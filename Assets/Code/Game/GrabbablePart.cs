using UnityEngine;
using System.Collections;


public class GrabbablePart : HexCellPlaceable
{
	SphereCollider _sphereCollider;
	
	#region implemented abstract members of HexCellPlaceable
	protected override void PlaceableStart ()
	{
		_sphereCollider = gameObject.GetComponentsInChildren<Collider>()[0] as SphereCollider;
	}

	protected override void PlaceableUpdate ()
	{
	}
	#endregion
	
	public override void PlaceAtLocation(IntVector2 location)
	{
		Location = location;
		if (location != null)
		{
			hexCell = GridManager.instance.GetHexCell(location);
			if (hexCell != null)
			{
				transform.position = hexCell.transform.position;
				hexCell.part = this;
			}
		}
		else
		{
			if (hexCell != null)
			{
				if (hexCell.part == this)
				{
					hexCell.part = null;
				}
				hexCell = null;
			}
		}
	}

	public void CheckForFinish ()
	{
		hexCell = GridManager.instance.GetHexCell(Location);
		
		if (hexCell != null && hexCell.finishCell)
		{
			// check if it is the right thing
			bool isCorrectConstruction = true;
			if (isCorrectConstruction)
			{
				PlaceAtLocation(null);
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
