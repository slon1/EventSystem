using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DIContainer: IDisposable {
	private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

	public void Dispose() {
		services.Clear();
	}

	public void Register<T>(T instance) => services[typeof(T)] = instance;
	public T Resolve<T>() => (T)services[typeof(T)];
}
