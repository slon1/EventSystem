using Newtonsoft.Json;
using System;

public abstract class QuestConditionBase : IQuestCondition {
	[JsonProperty] public string TaskID { get; private set; }
	[JsonProperty] public string Description { get; private set; }

	public event Action<IQuestCondition> OnProgress;

	
	protected QuestConditionBase(string taskID, string description) {
		TaskID = taskID;
		Description = description;
	}

	public abstract bool IsCompleted { get; }
	public abstract float GetProgress();
	public abstract void Activate();
	public abstract void Deactivate();

	protected void NotifyProgress() {
		OnProgress?.Invoke(this);
	}
}