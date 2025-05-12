using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;
interface IEnemyController {
	void Init();
	void SpawnEnemies();
	void Cleanup();
}
public class EnemyController : MonoBehaviour, IEnemyController {
	[SerializeField] private GameObject enemyPrefab;
	[SerializeField] private Transform canvas;
	[SerializeField] private int spawnCount = 50; 
	[SerializeField] private Vector2 spawnAreaSize = new Vector2(1920, 1080); 
	[SerializeField] private float moveInterval = 0.1f; 
	[SerializeField] private float moveRange = 75f; 
	[SerializeField] private float moveSpeed = 1f; 

	private ObjectPool<EnemyView> enemyPool;
	private readonly HashSet<EnemyView> activeEnemies = new();
	private readonly Dictionary<EnemyView, Vector2> initialPositions = new();
	private CancellationTokenSource moveCancellationToken;

	private void Awake() {
		
		enemyPool = new ObjectPool<EnemyView>(
			createFunc: CreateEnemy,
			actionOnGet: OnGetEnemy,
			actionOnRelease: OnReleaseEnemy,
			actionOnDestroy: OnDestroyEnemy,
			collectionCheck: true,
			defaultCapacity: spawnCount,
			maxSize: spawnCount * 2
		);
	}

	public void Init() {		
		moveCancellationToken = new CancellationTokenSource();
		MoveAll(moveCancellationToken.Token).Forget();
	}

	private void OnDestroy() {
		
		moveCancellationToken?.Cancel();
		moveCancellationToken?.Dispose();
		moveCancellationToken = null;

		
		foreach (var enemy in activeEnemies) {
			if (enemy != null) {
				enemy.Killed -= OnEnemyKilled;
			}
		}
		enemyPool.Dispose();
	}

	public void SpawnEnemies() {
		for (int i = 0; i < spawnCount; i++) {
			var enemy = enemyPool.Get();
			if (enemy != null) {
				activeEnemies.Add(enemy);
			}
		}
	}

	private EnemyView CreateEnemy() {
		var enemyObject = Instantiate(enemyPrefab, canvas);
		var enemyView = enemyObject.GetComponent<EnemyView>();
		if (enemyView == null) {
			Debug.LogError($"EnemyView component missing on {enemyObject.name}", enemyObject);
			Destroy(enemyObject);
			return null;
		}
		enemyView.Killed += OnEnemyKilled;
		return enemyView;
	}

	private void OnGetEnemy(EnemyView enemy) {
		if (enemy == null) return;

		enemy.gameObject.SetActive(true);

		
		var enemyType = (EnemyType)UnityEngine.Random.Range(0, 3); // A, B, C
		enemy.Init(enemyType);

		
		var rectTransform = enemy.GetComponent<RectTransform>();
		if (rectTransform != null) {
			float x = UnityEngine.Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
			float y = UnityEngine.Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
			var initialPosition = new Vector2(x, y);
			rectTransform.anchoredPosition = initialPosition;

			
			initialPositions[enemy] = initialPosition;
		}
		else {
			Debug.LogError($"RectTransform missing on {enemy.gameObject.name}", enemy);
		}
	}

	private void OnReleaseEnemy(EnemyView enemy) {
		if (enemy != null) {
			enemy.gameObject.SetActive(false);
			activeEnemies.Remove(enemy);
			initialPositions.Remove(enemy);
		}
	}

	private void OnDestroyEnemy(EnemyView enemy) {
		if (enemy != null) {
			enemy.Killed -= OnEnemyKilled;
			initialPositions.Remove(enemy);
			Destroy(enemy.gameObject);
		}
	}
	public void Cleanup() {
		var enemiesToCleanup = activeEnemies.ToList();

		foreach (var enemy in enemiesToCleanup) {
			if (enemy != null) {
				
				enemyPool.Release(enemy); 
			}
		}

		activeEnemies.Clear();
	}


	private void OnEnemyKilled(EnemyView enemy) {
		
		EventBus.Publish(new EnemyKilledEvent { EnemyType = enemy.EnemyType });
				
		if (enemy != null && activeEnemies.Contains(enemy)) {
			enemyPool.Release(enemy);
		}
	}

	private async UniTaskVoid MoveAll(CancellationToken cancellationToken) {
		while (!cancellationToken.IsCancellationRequested) {
			foreach (var enemy in activeEnemies) {
				if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

				
				if (!initialPositions.TryGetValue(enemy, out var initialPosition))
					continue;

				float offset = enemy.GetInstanceID() * 0.1f;
				float time = Time.time * moveSpeed;

				float xNoise = Mathf.PerlinNoise(time + offset, 0f) * 2f - 1f; // [-1, 1]
				float yNoise = Mathf.PerlinNoise(0f, time + offset) * 2f - 1f; // [-1, 1]

				Vector2 offsetPosition = new Vector2(xNoise * moveRange, yNoise * moveRange);
				var rectTransform = enemy.RectTransform;
				if (rectTransform != null) {
					rectTransform.anchoredPosition = initialPosition + offsetPosition;
				}
			}
			
			await UniTask.Delay(TimeSpan.FromSeconds(moveInterval), cancellationToken: cancellationToken);
		}
	}
}