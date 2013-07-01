using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GrabberManager : SingletonBehaviour<GrabberManager>
{
	Dictionary<Grabber, GrabbablePart> grabbedPart = new Dictionary<Grabber, GrabbablePart>();
	Dictionary<Construction, HashSet<Grabber>> grabbedBy = new Dictionary<Construction, HashSet<Grabber>>();
	
	void Start()
	{
		GameManager.instance.GameStateChangedEvent += OnGameStateChanged;
	}
	
	public override void OnDestroy()
	{
		if (GameManager.hasInstance)
		{
			GameManager.instance.GameStateChangedEvent -= OnGameStateChanged;
		}
		base.OnDestroy();
	}
	
	void OnGameStateChanged()
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
			grabbedPart.Clear();
			grabbedBy.Clear();
		}
	}

	public void TransferConstruction (GrabbablePart part, Construction toConstruction)
	{
		
		Grabber grabber = GrabberManager.instance.GetGrabberHolding(part);
		if (grabber != null)
		{
//			Debug.Log ("Grabber "+grabber.name +" is holding "+part.name);
//			Debug.Log ("TransferConstruction---");
			GrabberManager.instance.RegisterDrop(grabber, part);
		}
		part.transform.parent = toConstruction.transform;
		if (grabber != null)
		{
//			Debug.Log ("TransferConstruction---");
			GrabberManager.instance.RegisterGrab(grabber, part);
		}
	}

	
	public Grabber GetGrabberHolding(GrabbablePart part)
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
			if (part.ParentConstruction == null || !grabbedBy.ContainsKey(part.ParentConstruction))
			{
				return null;
			}
				
			foreach(Grabber grabber in grabbedBy[part.ParentConstruction])
			{
				if (grabbedPart[grabber] == part)
				{
					return grabber;
				}
			}
		}
		return null;
	}
	
	public GrabbablePart GetPartHeldBy(Grabber grabber)
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
			return grabbedPart.ContainsKey(grabber) ? grabbedPart[grabber] : null;
		}
		return null;
	}
		
	public int GetGrabbersHoldingCount(Construction construction)
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
			if (grabbedBy.ContainsKey(construction))
			{
				return grabbedBy[construction].Count;
			}
		}
		return 0;
		
	}
	public IEnumerable<Grabber> GetAllGrabbersHolding(Construction construction)
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
//			Debug.Log ("Getting All Grabbers");
			if (grabbedBy.ContainsKey(construction))
			{
//				Debug.Log ("grabbedBy.Contains "+construction, construction);
				foreach (Grabber grabber in grabbedBy[construction])
				{
//					Debug.Log ("grabbedBy -> "+grabber, grabber);
					yield return grabber;
				}
			}
		}
		yield break;
	}
	
	void DumpInfo()
	{
		/*
		Debug.Log("grabbedPart\n"+string.Join("\n", new List<Grabber>(grabbedPart.Keys).ConvertAll((input) => input.name+" < "+grabbedPart[input].name+" ("+grabbedPart[input].ParentConstruction.name+")").ToArray()) +
		
		"\ngrabbedBy\n"+string.Join("\n", new List<Construction>(grabbedBy.Keys).ConvertAll((input) => 
		{
			return string.Join ("\n ", new List<Grabber>(grabbedBy[input]).ConvertAll((grabber) => grabber.name).ToArray()) + 
				" < "+input.name;
		}).ToArray()));
		*/
//		Debug.Log("-------------");
	}
	
	public void RegisterGrab(Grabber grabber, GrabbablePart part)
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
			Debug.LogWarning("Grab "+grabber.name+":"+part.name+" ("+part.ParentConstruction.name+")");
			grabbedPart[grabber] = part;
			
			if (!grabbedBy.ContainsKey(part.ParentConstruction))
			{
				grabbedBy[part.ParentConstruction] = new HashSet<Grabber>();
			}
			grabbedBy[part.ParentConstruction].Add(grabber);
			DumpInfo();
		}
	}
	public void RegisterDrop(Grabber grabber, GrabbablePart part)
	{
		if (GameManager.instance.gameState == GameManager.State.Simulation)
		{
			Debug.LogWarning("Drop "+grabber.name+":"+part.name+" ("+part.ParentConstruction.name+")\n" +
				"grabbedPart.Remove("+grabber.name+")\n" +
				"grabbedBy["+part.ParentConstruction.name+"].Remove("+grabber.name+"); ");
			grabbedPart.Remove(grabber);
			grabbedBy[part.ParentConstruction].Remove(grabber);
			if (grabbedBy[part.ParentConstruction].Count == 0)
			{
				grabbedBy.Remove(part.ParentConstruction);
			}
			DumpInfo();
		}
	}
}