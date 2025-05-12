using Newtonsoft.Json;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class SurviveTimeCondition : QuestConditionBase {
	[JsonProperty] public float Seconds { get; private set; }
	[JsonProperty] public float RefreshRate { get; private set; }
	private CancellationTokenSource timeoutToken;
	private bool isCompleted;
	private float startTime;

	public override bool IsCompleted => isCompleted;
	public override float GetProgress() {
		if (isCompleted) return 1f;
		var elapsed = Time.unscaledTime - startTime;
		return Mathf.Clamp01(elapsed / Seconds);
	}

	public SurviveTimeCondition(string taskID, string description, float seconds, float refreshRate)
		: base(taskID, description) {
		this.Seconds = seconds;
		RefreshRate = refreshRate;
	}

	public override void Activate() {
		timeoutToken = new CancellationTokenSource();
		startTime = Time.unscaledTime;
		WaitForCompletion(timeoutToken.Token).Forget();
	}

	public override void Deactivate() {
		timeoutToken?.Cancel();
		timeoutToken?.Dispose();
		timeoutToken = null;
	}

	private async UniTaskVoid WaitForCompletion(CancellationToken token) {
		try {
			float elapsed = 0f;
			while (elapsed < Seconds && !token.IsCancellationRequested) {
				float delay = Mathf.Min(RefreshRate, Seconds - elapsed);
				if (delay <= 0) break;

				await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

				elapsed = Time.unscaledTime - startTime;
				if (elapsed >= Seconds) {
					isCompleted = true;				
				}
				NotifyProgress();
				if (isCompleted) { 
					break; }

			}
		}
		catch (OperationCanceledException) {
			
		}
	}
}