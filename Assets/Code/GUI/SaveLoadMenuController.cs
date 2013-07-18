using UnityEngine;
using System.Collections;

public class SaveLoadMenuController : SingletonBehaviour<SaveLoadMenuController> 
{
	
	
	[SerializeField]
	UIPanel _saveLoadPanel = null;
	
	[SerializeField]
	UIScrollList _saveScrollList = null;
	
	[SerializeField]
	UIListItem _saveListItemPrefab = null;
	
	[SerializeField]
	UITextField _saveLevelTextField = null;
	
	string SaveName { get { return _saveLevelTextField.Text; } }
	
	[SerializeField]
	UIButton _loadButton = null;
	
	[SerializeField]
	UIButton _deleteButton = null;
	
	[SerializeField]
	UIButton _quickSaveButton = null;
	
	string _quickSaveTextAtStart;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		_quickSaveTextAtStart = _quickSaveButton.spriteText.Text;
		_saveLoadPanel.Dismiss();
		
		GridManager.instance.MechanismChangedEvent += UpdateQuickSave;
		
		_saveLevelTextField.AddValueChangedDelegate((obj) => RefreshButtons());
		
		yield return null;
		RefreshButtons();
		
	}
	
	void UpdateQuickSave()
	{
		_quickSaveButton.spriteText.Text = _quickSaveTextAtStart+"*"+"\n"+SaveName;
	}
	
	void ResetQuickSave()
	{
		_quickSaveButton.spriteText.Text = _quickSaveTextAtStart+"\n"+SaveName;
	}
	
	void RefreshButtons()
	{
		bool exists = LevelDataManager.instance.Contains(SaveName);
		
		_loadButton.gameObject.SetActive(exists);
		_deleteButton.gameObject.SetActive(exists);
		
		_quickSaveButton.gameObject.SetActive(LevelDataManager.instance.Contains(GridManager.instance.LoadedLevelName));
		
	}
	
	void RefreshList()
	{
		RefreshButtons();
		
		_saveScrollList.ClearList(true);
		foreach (string levelName in LevelDataManager.instance.SaveList)
		{
			IUIListObject listObject = _saveScrollList.CreateItem(_saveListItemPrefab.gameObject, levelName);
			listObject.SetInputDelegate(HandleHandleEZInputDelegate);
		}
	}

	void HandleHandleEZInputDelegate (ref POINTER_INFO ptr)
	{
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP || ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE)
		{
			_saveLevelTextField.Text = (ptr.targetObj as UIListItem).Text;
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
		GridManager.instance.LoadLevel(SaveName);
		ResetQuickSave();
		RefreshButtons();
	}
	
	void Delete()
	{
		LevelDataManager.instance.Delete(SaveName, SaveType.Level, AutoSaveType.Named);
		RefreshList();
	}
	
	void Save()
	{
		GridManager.instance.SaveCurrentLevelAs(SaveName);
		RefreshList();
		ResetQuickSave();
		
	}
	
	void QuickSave()
	{
		GridManager.instance.SaveCurrentLevelAs(SaveName);
		ResetQuickSave();
	}
	
	#endregion
}
