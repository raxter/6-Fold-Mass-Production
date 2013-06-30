using UnityEngine;
using System.Collections;

public class ConstructionManager : SingletonBehaviour<ConstructionManager>
{
//	Construction GetConstruction()
//	{
//		Construction construction = ObjectPoolManager.GetObject(GameSettings.instance.constructionPrefab);
//		
//		if (GameManager.instance.gameState == GameManager.State.Simulation)
//		{
//			GrabberManager.instance.RegisterConstructionCreated(construction);
//		}
//		
//		return construction;
//	}
//	
//	void DestroyConstruction(Construction construction)
//	{
//		if (GameManager.instance.gameState == GameManager.State.Simulation)
//		{
//			GrabberManager.instance.RegisterConstructionDestroyed(construction);
//		}
//	}
}
