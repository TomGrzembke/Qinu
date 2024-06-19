using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InkEvents : MonoBehaviour
{
    #region Inspector
    [SerializeField] List<InkEvent> inkEvents;
    #endregion

    #region Unity Event Functions

    #endregion

    void OnEnable()
    {
        DialogueController.InkEvent += TryInvokeEvent;
    }

    void OnDisable()
    {
        DialogueController.InkEvent -= TryInvokeEvent;
    }

    void TryInvokeEvent(string eventName)
    {
        foreach (InkEvent inkEvent in inkEvents)
        {
            if (inkEvent.name == eventName)
            {
                inkEvent.onEvent.Invoke();
                return;
            }
        }
    }
    public void InvokeAllEvents()
    {
        foreach (InkEvent inkEvent in inkEvents)
            inkEvent.onEvent.Invoke();
    }
}

[Serializable]
public struct InkEvent
{
    public string name;

    public UnityEvent onEvent;
}
