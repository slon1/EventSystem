using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Pool;
public enum EnemyType { A, B, C }
public class EnemyKilledEvent {
	public EnemyType EnemyType; 	
}

[JsonObject(MemberSerialization.OptIn)]
public class KillEnemiesCondition : QuestConditionBase {
	[JsonProperty] public EnemyType? EnemyType { get; private set; }
	[JsonProperty] public int Count { get; private set; }
	private int _current;
	

	public override bool IsCompleted => _current >= Count;
	public override float GetProgress() => (float)_current / Count;

	public KillEnemiesCondition(string taskID, string description, EnemyType? enemyType, int count)
		: base(taskID, description) {
		if (count <= 0) {
			Debug.LogError($"Invalid count for task {taskID}: {count}. Setting to 1.");
			count = 1;
		}
		EnemyType = enemyType;
		Count = count;
	
	}

	public override void Activate() {
		EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
	}

	public override void Deactivate() {
		EventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
	
	}

	private void OnEnemyKilled(EnemyKilledEvent evt) {
		if (!EnemyType.HasValue || evt.EnemyType == EnemyType) {
			_current++;
			NotifyProgress();	

		}
	
	}


}