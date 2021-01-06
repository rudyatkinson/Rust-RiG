//#define DEBUG_BackButtonTrack

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour
{
    #region Instance
    public static BackButton _Instance { get; private set; }
    #endregion

    // Stack includes page information which last opened.
    Stack eventStack;

    private void Awake()
    {
        Screen.fullScreen = false;
        _Instance = _Instance ?? this;
        eventStack = new Stack();
    }

    public void AddEvent(IBack page) => eventStack.Push(page);
    public void DeleteLastEvent()
    {
#if DEBUG_BackButtonTrack
        int size = eventStack.Count;
#endif
        eventStack.Pop();
#if DEBUG_BackButtonTrack
        Debug.Log($"eventStack size at beginning: {size}, eventStack size at the end: {eventStack.Count} as button");
#endif
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
#if DEBUG_BackButtonTrack
            int size = eventStack.Count;
#endif
            // if there are any event in the stack, stack pops last page and 
            // calls Back funtion via IBack interface. Else application quit.
            if (eventStack.Count > 0)
            {
                IBack page = eventStack.Pop() as IBack;
                page.Back();
            }
            else Application.Quit();
#if DEBUG_BackButtonTrack
            Debug.Log($"eventStack size at beginning: {size}, eventStack size at the end: {eventStack.Count} as escape");
#endif
        }
    }
}
