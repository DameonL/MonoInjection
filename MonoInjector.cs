using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonoInjection
{
	public class MonoInjector : MonoBehaviour
	{
		[SerializeField]
		private InjectionMode _injectionMode;

		private HashSet<IUpdate> _subscribersToAdd = new HashSet<IUpdate>();
		private HashSet<IUpdate> _subscribersToRemove = new HashSet<IUpdate>();

		/// <summary>
		/// Subscribers are called every Update, and passed a single float, which is Time.deltaTime.
		/// </summary>
		private HashSet<IUpdate> _subscribers = new HashSet<IUpdate>();

		/// <summary>
		/// Subscribers are called only once, at the next Update.
		/// </summary>
		private Queue<Action> _queuedCallbacks = new Queue<Action>();

		private readonly object _callbackMonitor = new object();

		private void Update()
		{
			if (_injectionMode == InjectionMode.Update)
			{
				ProcessCalls(Time.deltaTime);
			}
		}

		private void FixedUpdate()
		{
			if (_injectionMode == InjectionMode.FixedUpdate)
			{
				ProcessCalls(Time.fixedDeltaTime);
			}
		}

		private void LateUpdate()
		{
			if (_injectionMode == InjectionMode.LateUpdate)
			{
				ProcessCalls(Time.deltaTime);
			}
		}

		private void ProcessCalls(float deltaTime)
		{
			lock (_callbackMonitor)
			{
				while (_queuedCallbacks.Count > 0)
				{
					var action = _queuedCallbacks.Dequeue();
					if (action != null)
						action.Invoke();
				}
			}

			foreach (var subscriber in _subscribersToRemove)
			{
				_subscribers.Remove(subscriber);
			}
			_subscribersToRemove.Clear();

			foreach (var subscriber in _subscribersToAdd)
			{
				_subscribers.Add(subscriber);
			}
			_subscribersToAdd.Clear();

			var enumerator = _subscribers.GetEnumerator();
			while (enumerator.MoveNext() != false)
			{
				enumerator.Current.OnUpdate(deltaTime);
			}
		}

		/// <summary>
		/// Adds an object to the list of subscribers called every update.
		/// </summary>
		/// <param name="subscriber">An object which is to be called every update.</param>
		public void Subscribe(IUpdate subscriber)
		{
			if (_subscribersToRemove.Contains(subscriber))
				_subscribersToRemove.Remove(subscriber);
			if (!_subscribers.Contains(subscriber))
			{
				_subscribersToAdd.Add(subscriber);
			}
		}

		/// <summary>
		/// Removes an object from the list of subscribers called every Update.
		/// </summary>
		/// <param name="subscriber">The object to be removed from the list.</param>
		public void Unsubscribe(IUpdate subscriber)
		{
			if (_subscribersToAdd.Contains(subscriber))
				_subscribersToAdd.Remove(subscriber);
			if (_subscribers.Contains(subscriber))
			{
				_subscribersToRemove.Add(subscriber);
			}
		}

		/// <summary>
		/// Queues a callback to be called at the next Update.
		/// </summary>
		/// <param name="callback">Any delegate that accepts no parameters and returns void.</param>
		public void Inject(Action callback)
		{
			lock (_callbackMonitor)
				_queuedCallbacks.Enqueue(callback);
		}

		public void Reset()
		{
			_subscribersToAdd.Clear();
			_subscribersToRemove.Clear();
			_subscribers.Clear();
			_queuedCallbacks.Clear();
		}

		private void OnDestroy()
		{
			_queuedCallbacks = null;
			_subscribersToAdd = null;
			_subscribersToRemove = null;
		}

		public enum InjectionMode
		{
			Update,
			FixedUpdate,
			LateUpdate
		}
	}
}
