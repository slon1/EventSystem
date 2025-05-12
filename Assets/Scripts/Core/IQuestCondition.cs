using System;

public interface IQuestCondition {
	string TaskID { get; }
	string Description { get; }
	bool IsCompleted { get; }
	float GetProgress();
	void Activate();
	void Deactivate();
	event Action<IQuestCondition> OnProgress;
}