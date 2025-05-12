using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.Jobs;

public class EnemyMover : MonoBehaviour {
	[SerializeField] private float moveInterval = 0.1f; 
	[SerializeField] private float moveRange = 50f; 
	[SerializeField] private float noiseScale = 0.005f; 
	[SerializeField] private float waveSpeed = 0.5f; 

	private HashSet<EnemyView> enemies;
	private NativeArray<Vector2> initialPositions;
	private NativeArray<float> offsets;
	private TransformAccessArray transformAccessArray;
	private CancellationTokenSource animationCts;
	private bool isInitialized;

	public void Init(HashSet<EnemyView> enemies) {
		if (enemies == null || enemies.Count == 0) {
			Debug.LogWarning("EnemyMover initialized with empty or null enemy set.");
			return;
		}

		
		StopAnimation();

		this.enemies = enemies;
		InitializeJobs();
		isInitialized = true;

		
		animationCts = new CancellationTokenSource();
		StartAnimation(animationCts.Token).Forget();
	}

	public void Reinitialize(HashSet<EnemyView> enemies) {
		
		StopAnimation();

		this.enemies = enemies;
		InitializeJobs();
		isInitialized = enemies != null && enemies.Count > 0;

		
		if (isInitialized) {
			animationCts = new CancellationTokenSource();
			StartAnimation(animationCts.Token).Forget();
		}
	}

	private void OnDestroy() {
		StopAnimation();
		if (transformAccessArray.isCreated)
			transformAccessArray.Dispose();
		if (initialPositions.IsCreated)
			initialPositions.Dispose();
		if (offsets.IsCreated)
			offsets.Dispose();
	}

	private void StopAnimation() {
		if (animationCts != null) {
			animationCts.Cancel();
			animationCts.Dispose();
			animationCts = null;
		}
	}

	private async UniTaskVoid StartAnimation(CancellationToken cancellationToken) {
		try {
			while (!cancellationToken.IsCancellationRequested) {
				if (isInitialized && enemies.Count > 0) {
					await UpdatePositions(cancellationToken);
				}
				await UniTask.Delay(TimeSpan.FromSeconds(moveInterval), cancellationToken: cancellationToken);
			}
		}
		catch (OperationCanceledException) {
		
		}
		catch (Exception ex) {
			Debug.LogError($"Error in EnemyMover animation: {ex.Message}", this);
		}
	}

	private async UniTask UpdatePositions(CancellationToken cancellationToken) {
		if (enemies.Count == 0 || !transformAccessArray.isCreated) return;

		
		var job = new MoveEnemiesJob {
			InitialPositions = initialPositions,
			Offsets = offsets,
			NoiseScale = noiseScale,
			MoveRange = moveRange,
			Time = Time.time * waveSpeed
		};

		
		JobHandle handle = job.Schedule(transformAccessArray);

		
		await UniTask.WaitUntil(() => handle.IsCompleted, cancellationToken: cancellationToken);

		
		handle.Complete();
	}

	private void InitializeJobs() {
		
		if (transformAccessArray.isCreated)
			transformAccessArray.Dispose();
		if (initialPositions.IsCreated)
			initialPositions.Dispose();
		if (offsets.IsCreated)
			offsets.Dispose();

		
		initialPositions = new NativeArray<Vector2>(enemies.Count, Allocator.Persistent);
		offsets = new NativeArray<float>(enemies.Count, Allocator.Persistent);
		transformAccessArray = new TransformAccessArray(enemies.Count);

		int index = 0;
		foreach (var enemy in enemies) {
			if (enemy != null) {
				var rectTransform = enemy.GetComponent<RectTransform>();
				if (rectTransform != null) {
					initialPositions[index] = rectTransform.anchoredPosition;
		
					offsets[index] = UnityEngine.Random.Range(0f, 100f);
					transformAccessArray.Add(enemy.transform);
					index++;
				}
			}
		}
	}

	private struct MoveEnemiesJob : IJobParallelForTransform {
		[ReadOnly] public NativeArray<Vector2> InitialPositions;
		[ReadOnly] public NativeArray<float> Offsets;
		public float NoiseScale;
		public float MoveRange;
		public float Time;

		public void Execute(int index, TransformAccess transform) {
			if (!transform.isValid) return;

			Vector2 initialPosition = InitialPositions[index];

		
			float xInput = initialPosition.x * NoiseScale + Time;
			float yInput = initialPosition.y * NoiseScale + Time;
			float offset = Offsets[index];

			float xNoise = Mathf.PerlinNoise(xInput + offset, yInput) * 2f - 1f;
			float yNoise = Mathf.PerlinNoise(xInput, yInput + offset) * 2f - 1f;

			Vector2 offsetPosition = new Vector2(xNoise * MoveRange, yNoise * MoveRange);
			transform.localPosition = initialPosition + offsetPosition;
		}
	}
}