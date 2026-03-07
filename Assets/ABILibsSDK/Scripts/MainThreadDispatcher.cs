using System;
using System.Collections.Generic;
using UnityEngine;

namespace ABILibsSDK
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly Queue<Action> _actionQueue = new Queue<Action>();
        private static readonly object _lock = new object();

        public static void Enqueue(Action action)
        {
            if (action == null) return;

            lock (_lock)
            {
                _actionQueue.Enqueue(action);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Update()
        {
            lock (_lock)
            {
                while (_actionQueue.Count > 0)
                {
                    var action = _actionQueue.Dequeue();
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
