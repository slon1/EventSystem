using System;
using UnityEngine;
using UnityEngine.UI;

public class TaskUIPrefab : MonoBehaviour {
	[SerializeField] private Image taskIcon; 
	[SerializeField] private Text taskText; 
	[SerializeField] private Image progressBar; 

	private IQuestCondition _task; 

	public void Initialize(IQuestCondition task, Sprite iconSprite) {
		_task = task;
		taskText.text = task.Description;
		taskIcon.sprite = iconSprite;
		progressBar.fillAmount = task.GetProgress();

		progressBar.type = Image.Type.Filled;
		progressBar.fillMethod = Image.FillMethod.Horizontal;
		progressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
	}

	public void UpdateProgress() {
		if (_task == null) return;

		progressBar.fillAmount = _task.GetProgress();
		taskText.text = _task.IsCompleted
			? $"{_task.Description} (Completed)"
			: $"{_task.Description}: {Mathf.RoundToInt(_task.GetProgress() * 100)}%";
	}

	internal Sprite GetSprite() {
		return taskIcon.sprite;
	}


	private void OnDestroy() {
		_task = null;
	}
}