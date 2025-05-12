using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskManager {
	private readonly List<IQuestCondition> allTasks;
	private readonly HashSet<IQuestCondition> activeTasks = new();
	public event Action<IQuestCondition> OnProgress;
	public TaskManager(List<IQuestCondition> allTasks) {
		this.allTasks = allTasks;
	}

	public void StartTasks(int count) {
		int tasksToStart = Math.Min(count, allTasks.Count - activeTasks.Count);
		int started = 0;

		foreach (var task in allTasks) {
			if (started >= tasksToStart)
				break;

			if (!activeTasks.Contains(task)) {
				task.OnProgress += OnTaskProgress;
				task.Activate();
				activeTasks.Add(task);
				started++;
			}
		}
	}

	private void OnTaskProgress(IQuestCondition task) {
		if (task.IsCompleted) {
			task.OnProgress -= OnTaskProgress;
			task.Deactivate();
			activeTasks.Remove(task);
			UnityEngine.Debug.Log($"{task.TaskID} Completed!");
		}		
		OnProgress?.Invoke(task);
		UnityEngine.Debug.Log($"{task.TaskID} Progress: {task.GetProgress():P0}");
	}

	public void CompleteAllTasks() {
		foreach (var task in activeTasks.ToList()) {
			task.Deactivate();
			task.OnProgress -= OnTaskProgress;
		}
		activeTasks.Clear();
	}
}