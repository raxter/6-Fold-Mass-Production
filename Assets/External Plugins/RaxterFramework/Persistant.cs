using UnityEngine;
using System.Collections;

public class Persistant : MonoBehaviour 
{
	
	private bool isFirst = false;
	
	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		
		gameObject.name += "(Persistant)";
	}
}
