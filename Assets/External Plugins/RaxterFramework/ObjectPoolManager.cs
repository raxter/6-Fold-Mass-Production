using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectPoolManager : SingletonBehaviour<ObjectPoolManager> 
{
	
	Dictionary<GameObject, ObjectPool> prefabPools = new Dictionary<GameObject, ObjectPool>();
	Dictionary<GameObject, GameObject> objectToPrefab = new Dictionary<GameObject, GameObject>();
	
	static int id = 0;
	
	Dictionary<GameObject,int> objectIds = new Dictionary<GameObject, int>();
	
	public class ObjectPool
	{
		GameObject prefab;
		
		public ObjectPool (GameObject __prefab)
		{
			prefab = __prefab;
			
			if (prefab == null)
			{
				Debug.LogError("Creating object pool with null prefab!");
			}
		}
		
		public HashSet<GameObject> active = new HashSet<GameObject>();
		public HashSet<GameObject> inactive = new HashSet<GameObject>();
		
		public GameObject ActivateObject()
		{
			GameObject toActivate;
			if (inactive.Count == 0)
			{
				toActivate = GameObject.Instantiate(prefab) as GameObject;
				instance.objectIds.Add(toActivate, id);
				id += 1;
			}
			else
			{
				toActivate = inactive.First();
				inactive.Remove(toActivate);
			}
			
			instance.objectToPrefab[toActivate] = prefab;
			active.Add (toActivate);
			toActivate.SetActive(true);
			return toActivate;
		}
		
		public bool DeactivateObject(GameObject toDeactivate)
		{
			if (!active.Contains(toDeactivate))
			{
				Debug.LogError("Deactivating an object in a pool that does not contain it! Doing nothing");
				return false;
			}
			if (inactive.Contains(toDeactivate))
			{
				Debug.LogWarning("Deactivating an object that is already inactive");
				return false;
			}
			
			inactive.Add(toDeactivate);
			active.Remove(toDeactivate);
			toDeactivate.SetActive(false);
			
			if (!ObjectPoolManager.instance.objectToPrefab.ContainsKey(toDeactivate))
			{
				Debug.LogError("Deactivated object is not in the object pool reverse/prefab lookup dictionary");
				
			}
			ObjectPoolManager.instance.objectToPrefab.Remove(toDeactivate);
			return true;
		}
	}
	
	public static bool DestroyObject<T>(T prefabComponent) where T : MonoBehaviour
	{
		if (prefabComponent == null)
		{
			return false;
		}
		if(DestroyObject(prefabComponent.gameObject))
		{
			if (prefabComponent is IPooledObject)
			{
				(prefabComponent as IPooledObject).OnPoolDeactivate();
			}
			return true;
		}
		return false;
	}
	
	public static bool DestroyObject(GameObject toDestroy)
	{
		
#if UNITY_EDITOR
		if (!EditorApplication.isPlaying)
		{
			Debug.LogWarning("Game is not playing, destroying immediately!");
			GameObject.DestroyImmediate(toDestroy);
			return true;
		}
#endif
		return instance.DestroyObjectInternal(toDestroy);
	}
	
	bool DestroyObjectInternal(GameObject toDestroy)
	{
		if (toDestroy == null)
		{
			Debug.LogWarning("trying to remove a null object from the pool! Doing nothing");
			return false;
		}
		
		
		if (!objectToPrefab.ContainsKey(toDestroy))
		{
			Debug.LogError("Trying to remove an object that was not pooled! Doing nothing", toDestroy);
			return false;
		}
		
		GameObject prefab = objectToPrefab[toDestroy];
		
		if(prefabPools[prefab].DeactivateObject(toDestroy))
		{
			toDestroy.name = prefab.name + "(Pooled - Inactive "+objectIds[toDestroy]+")";
			toDestroy.transform.parent = this.transform;
			return true;
		}
		return false;
	}
	
	public static T GetObject<T>(T prefabComponent) where T : MonoBehaviour
	{
		if (prefabComponent == null)
		{
			return null;
		}
		
		return GetObject<T>(prefabComponent.gameObject);
	}
	
	public static T GetObject<T>(GameObject prefab) where T : MonoBehaviour
	{
		T activated = GetObject(prefab).GetComponent<T>();
		if (activated != null)
		{
			if (activated is IPooledObject)
			{
				(activated as IPooledObject).OnPoolActivate();
			}
		}
		
		return activated;
	}
	
	public static GameObject GetObject(GameObject prefab)
	{
		if (prefab == null)
		{
			Debug.LogWarning("Requesting a null object from the ObjectPool");
			return null;
		}
		
#if UNITY_EDITOR
		if (!EditorApplication.isPlaying)
		{
			Debug.LogWarning("Game is not playing, instantiating prefab!");
			return PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		}
#endif
		if (!instance.prefabPools.ContainsKey(prefab))
		{
			instance.prefabPools[prefab] = new ObjectPool(prefab);
		}
		
		GameObject toReturn = instance.prefabPools[prefab].ActivateObject();
		toReturn.name = prefab.name + "(Pool Clone "+instance.objectIds[toReturn]+")";
		toReturn.transform.parent = null;
		toReturn.transform.position = Vector3.zero;
		return toReturn;
	}
	
	
}
