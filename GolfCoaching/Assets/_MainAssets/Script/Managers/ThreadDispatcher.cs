using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadDispatcher : MonoBehaviourSingleton<ThreadDispatcher>
{
    private Queue<Action> _jobs = new Queue<Action>();

    public void Enqueue(Action action)
    {
        lock (_jobs)
        {
            _jobs.Enqueue(action);
        }
    }

    void Update()
    {
        // �� ������ ���� �����忡�� ��� ���� �۾� ����
        lock (_jobs)
        {
            while (_jobs.Count > 0)
            {
                var job = _jobs.Dequeue();
                job.Invoke();
            }
        }
    }
}
