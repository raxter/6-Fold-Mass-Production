using UnityEngine;
using System.Collections;

public class GrabberInstructions : MonoBehaviour 
{
	[SerializeField]
	EmptyInstructionSlot emptyInstructionPrefab;
	
	
	EmptyInstructionSlot [] emptyInstructions;
	
	
	void Start ()
	{
		int numberOfInstructions = 12;
		
		emptyInstructions = new EmptyInstructionSlot[numberOfInstructions];
		
		for (int i = 0 ; i < numberOfInstructions ; i++)
		{
			emptyInstructions[i] = Instantiate(emptyInstructionPrefab) as EmptyInstructionSlot;
			
			emptyInstructions[i].gameObject.name = "Instruction "+i;
			emptyInstructions[i].transform.parent = transform;
			emptyInstructions[i].transform.localPosition = Vector3.zero + (Vector3.down*60f*i);
			
		}
	}
}
