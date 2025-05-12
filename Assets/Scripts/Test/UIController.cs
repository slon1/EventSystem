using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

interface IUIController {
	 void UpdateTaskUI(IQuestCondition task);
	 void Init(List<IQuestCondition> quests);
	void Clear();
}
public class UIController : MonoBehaviour, IUIController {
	[SerializeField] private GameObject taskUIPrefab; 
	[SerializeField] private Transform root; 
	[SerializeField] private Sprite defaultTaskIcon; 
	[SerializeField] private List<TaskIconMapping> taskIcons; 

	private readonly Dictionary<string, TaskUIPrefab> taskUIElements = new();

	[System.Serializable]
	private struct TaskIconMapping {
		public string TaskID;
		public Sprite Icon;
	}
	

	public void Init(List<IQuestCondition> quests) {
		quests.ForEach(quest => CreateTaskUI(quest));
	}

	
	private void CreateTaskUI(IQuestCondition task) {
		var taskUIObject = Instantiate(taskUIPrefab, root);
		var taskUI =  taskUIObject.GetComponent<TaskUIPrefab>();

		
		Sprite icon = defaultTaskIcon;
		foreach (var mapping in taskIcons) {
			if (mapping.TaskID == task.TaskID) {
				icon = mapping.Icon;
				break;
			}
		}

		taskUI.Initialize(task, icon);
		taskUIElements[task.TaskID] = taskUI;
	}

	public void UpdateTaskUI(IQuestCondition task) {
		if (taskUIElements.TryGetValue(task.TaskID, out var taskUI)) {
			taskUI.UpdateProgress();
		}
		if (task.IsCompleted) {
			RemoveTaskUI(task);
		}
	}

	private void RemoveTaskUI(IQuestCondition task) {
		if (taskUIElements.TryGetValue(task.TaskID, out var taskUI)) {
			Destroy(taskUI.gameObject);
			taskUIElements.Remove(task.TaskID);
		}
	}
	public void Clear() {
		foreach (var taskUI in taskUIElements.Values) {
			if (taskUI != null) {
				Destroy(taskUI.gameObject);
			}
		}
		taskUIElements.Clear();
	}
}