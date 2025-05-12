using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour {
	[SerializeField] private TextAsset questJson;
	private IUIController uiManager;
	private IEnemyController enemyController;

	private TaskLoader taskLoader;
	private TaskManager taskManager;
	public UnityEvent OnTaskCompleted;	
		
	
	
	private void Start() {

		taskLoader = new TaskLoader(questJson.text);
		var tasks = taskLoader.LoadAllQuests();
		taskManager = new TaskManager(tasks);
		taskManager.OnProgress += TaskManager_OnProgress;
		
		uiManager = Installer.GetService<IUIController>();
		uiManager.Init(tasks);
		
		enemyController = Installer.GetService<IEnemyController>();
		enemyController.Init();
		enemyController.SpawnEnemies();

		taskManager.StartTasks(3);
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
		uiManager.Clear();
	}
	
}
