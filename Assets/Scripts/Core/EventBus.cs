using UnityEngine;

using System;
using System.Collections.Generic;

public static class EventBus {
	private static readonly Dictionary<Type, Delegate> _handlers = new();

	public static void Subscribe<T>(Action<T> listener) {
		var type = typeof(T);
		if (_handlers.TryGetValue(type, out var del)) {
			_handlers[type] = Delegate.Combine(del, listener);
		}
		else {
			_handlers[type] = listener;
		}
	}

	public static void Unsubscribe<T>(Action<T> listener) {
		var type = typeof(T);
		if (_handlers.TryGetValue(type, out var del)) {
			var newDel = Delegate.Remove(del, listener);
			if (newDel == null) {
				_handlers.Remove(type);
			}
			else {
				_handlers[type] = newDel;
			}
		}
	}

	public static void Publish<T>(T evt) {
		var type = typeof(T);
		if (_handlers.TryGetValue(type, out var del)) {
			((Action<T>)del)?.Invoke(evt);
		}
	}

	public static void ClearAll() {
		_handlers.Clear();
	}
}