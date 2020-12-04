using System;
using System.Collections.Generic;

public class ConcurrentList<T>
{
    private readonly object lockObj = new object();
    List<T> list = new List<T>();
    public void Add(T item)
    {
        lock (lockObj)
        {
            list.Add(item);
        }
    }

    public void Remove(T item)
    {
        lock (lockObj)
        {
            list.Add(item);
        }
    }
    public void ForEach(Action<T> action)
    {
        lock (lockObj)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }
    }

}