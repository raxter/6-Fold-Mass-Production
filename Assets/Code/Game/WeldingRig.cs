using UnityEngine;
using System.Collections;

public class WeldingRig : Mechanism 
{
	#region implemented abstract members of Mechanism
	public override MechanismType MechanismType {
		get 
		{
			return MechanismType.WeldingRig;
		}
	}

	protected override void MechanismUpdate ()
	{
		
	}
	#endregion
}
