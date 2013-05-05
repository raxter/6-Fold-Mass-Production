using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ConstructionDefinition : ScriptableObject, System.IComparable<ConstructionDefinition>, System.IComparable<GrabbablePart>
{
	[System.Serializable]
	public class ConstructionElement
	{
		public ConstructionElement(GrabbablePart part)
		{
			orientation = part.orientation;
			
			for (int i = 0 ; i < 6 ; i++)
			{
				GrabbablePart.ConnectionDescription connectionDescription = part.connectedParts[i];
				physicalConnectionType[i] = connectionDescription.connectionType;
				weldedInDirection[i] = null;
			}
		}
		
		public ConstructionElement()
		{
		}
		
		public PartType partType = PartType.None;
		
		public HexMetrics.Direction orientation = HexMetrics.Direction.Up;
		
		public ConstructionElement [] weldedInDirection = new ConstructionElement [6];
		public GrabbablePart.PhysicalConnectionType [] physicalConnectionType = new GrabbablePart.PhysicalConnectionType [6];
	}
	
	private ConstructionDefinition (IEnumerable<ConstructionElement> elements)
	{
		constructionElements = new List<ConstructionElement>(elements);
	}
	
	public List<ConstructionElement> constructionElements = new List<ConstructionElement>();
	
	#region IComparable[ConstructionDefinition] implementation
	public int CompareTo (ConstructionDefinition other)
	{
		Debug.Log (constructionElements.Count+":"+other.constructionElements.Count);
		
		
		return 0;
	}
	#endregion

	#region IComparable[GrabbablePart] implementation
	public int CompareTo (GrabbablePart other)
	{
		return CompareTo(ToConstructionDefinition(other));
	}
	#endregion
	
	public static ConstructionDefinition ToConstructionDefinition(GrabbablePart rootPart)
	{
		Dictionary<GrabbablePart, ConstructionElement> checkedParts = new Dictionary<GrabbablePart, ConstructionElement>();
		
		Queue<GrabbablePart> partQueue = new Queue<GrabbablePart>();
		
		partQueue.Enqueue(rootPart);
		checkedParts[rootPart] = new ConstructionElement(rootPart);
		
		while(partQueue.Count > 0)
		{
			GrabbablePart currentPart = partQueue.Dequeue();
			
			for (int i = 0 ; i < 6 ; i++)
			{
				GrabbablePart.ConnectionDescription connectionDescription = currentPart.connectedParts[i];
				GrabbablePart connectedPart = connectionDescription.connectedPart;
				if (!checkedParts.ContainsKey(connectedPart))
				{
					checkedParts[connectedPart] = new ConstructionElement(connectedPart);
					partQueue.Enqueue(connectedPart);
				}
				checkedParts[currentPart].weldedInDirection[i] = checkedParts[connectedPart];
			}
		}
		
		return new ConstructionDefinition(checkedParts.Values);
	}
	
}
