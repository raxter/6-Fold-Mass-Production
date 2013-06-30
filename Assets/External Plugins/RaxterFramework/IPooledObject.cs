using UnityEngine;
using System.Collections;

// this interface is used by the ObjectPool to initialise and deinitialise objects, it is only a helper interface and is not required by the ObjectPool
public interface IPooledObject
{
	void OnPoolActivate();
	
	
	void OnPoolDeactivate();
}
