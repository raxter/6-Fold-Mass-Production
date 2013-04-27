using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ConstructionDefinition : ScriptableObject, System.IComparable<ConstructionDefinition>
{
	[System.Serializable]
	public class ConstructionElement
	{
		public MechanismType mechanism = MechanismType.None;
		
		public ConstructionElement [] weldedInDirection = new ConstructionElement [6];
	}
	
	public List<ConstructionElement> constructionElements = new List<ConstructionElement>();
	
	#region IComparable[ConstructionDefinition] implementation
	public int CompareTo (ConstructionDefinition other)
	{
		throw new System.NotImplementedException ();
	}
	#endregion
}
