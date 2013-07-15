using UnityEngine;
using System.Collections;

public class SaveLoadMenuController : SingletonBehaviour<SaveLoadMenuController> 
{
	
	
	[SerializeField]
	UIPanel _saveLoadPanel;
	
	[SerializeField]
	UIScrollList _saveScrollList;
	
	[SerializeField]
	UIListItem _saveListItemPrefab;
	
	[SerializeField]
	SpriteText _saveLevelName;
	
	// Use this for initialization
	void Start () 
	{
		_saveLoadPanel.Dismiss();
	}
	
	void RefreshList()
	{
		_saveScrollList.ClearList(false);
		foreach (string levelName in LevelDataManager.instance.SaveList)
		{
			_saveScrollList.CreateItem(_saveListItemPrefab.gameObject, levelName);
		}
	}
	
	#region EZ GUI
	
	void BringIn()
	{
		RefreshList();
		_saveLoadPanel.BringIn();
	}
	
	
	void Dismiss()
	{
		_saveLoadPanel.Dismiss();
	}
	
	void Load()
	{
		GridManager.instance.LoadLevel(_saveLevelName.Text);
	}
	
	void Save()
	{
		GridManager.instance.SaveCurrentLevelAs(_saveLevelName.Text);
		RefreshList();
	}
	
	#endregion
}
