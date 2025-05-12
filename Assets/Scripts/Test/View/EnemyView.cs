using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour, IPointerClickHandler {
	private Image image;
	private EnemyType enemyType;
	public EnemyType EnemyType=>enemyType;
	public event Action<EnemyView> Killed;
	private RectTransform rect;
	public RectTransform RectTransform=>rect;
	public void Init(EnemyType enemyType) {
		rect = GetComponent<RectTransform>();
		image = GetComponent<Image>();
		if (image == null) {
			Debug.LogError($"Image component missing on {gameObject.name}", this);
			return;
		}
		this.enemyType = enemyType;
		switch (enemyType) {
			case EnemyType.A:
				image.color = Color.blue;
				break;
			case EnemyType.B:
				image.color = Color.yellow;
				break;
			case EnemyType.C:
				image.color = Color.green;
				break;
			default:
				break;
		}
	}

	public void OnPointerClick(PointerEventData eventData) {
		Killed?.Invoke(this);
	}
}