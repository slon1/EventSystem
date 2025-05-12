using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour {
	[SerializeField] private TextAsset questJson;
	private IUIManager uiManager;
	private IEnemyController enemyController;

	private TaskLoader taskLoader;
	private TaskManager taskManager;
	public UnityEvent OnTaskCompleted;	
		
	
	
	private void Start() {
		taskLoader = new TaskLoader(questJson.text);
		var tasks = taskLoader.LoadAllQuests();
		taskManager = new TaskManager(tasks);
		enemyController = Installer.GetService<IEnemyController>();
		uiManager = Installer.GetService<IUIManager>();
		uiManager.Init(tasks);
		taskManager.OnProgress += TaskManager_OnProgress;

		taskManager.StartTasks(3);
		enemyController.Init();
		enemyController.SpawnEnemies();
	}

	private void TaskManager_OnProgress(IQuestCondition task) {
		uiManager.UpdateTaskUI(task);
		if (task.IsCompleted) {
			OnTaskCompleted?.Invoke();
		}
	}


	private void OnDestroy() {
		taskManager.CompleteAllTasks();
		taskManager.OnProgress -= TaskManager_OnProgress;
		enemyController.Cleanup();
	}
	
}
