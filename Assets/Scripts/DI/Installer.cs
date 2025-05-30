using System;
using System.Collections;
using UnityEngine;
public class Installer : MonoBehaviour {
	
	private static DIContainer container;
	
	
	
	void Awake() {		
		container = new DIContainer();
		
		container.Register<IUIController>(GetComponent<UIController>());
		container.Register<IEnemyController>(GetComponent<EnemyController>());
		container.Register(GetComponent<EnemyMover>());

	}
	
	
	public static T GetService<T>() => container.Resolve<T>();
	private void OnDestroy() {
		container.Dispose();
	}
}
