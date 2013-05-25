using UnityEngine;
using System.Collections;

public class DraggableInstruction : MonoBehaviour 
{
	
	public UIButton button;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	bool _isClone = false;
	
	public bool isClone { get { return _isClone; } }
	
	public DraggableInstruction GetClone()
	{
		DraggableInstruction clone = Instantiate(this) as DraggableInstruction;
		System.Array.ForEach<SpriteRoot>(clone.GetComponentsInChildren<SpriteRoot>(),(obj) => obj.isClone = true);
		clone.transform.parent = transform.parent;
		clone.transform.localPosition = Vector3.zero;
		clone._isClone = true;
		
		return clone;
	}
	
	
}
