using System.Collections.Generic;
using UnityEngine;

public abstract class Page : MonoBehaviour
{
    public abstract IPageContent IPage { get; set; }
    public abstract Animator Animator { get; set; }
    public virtual void OnEnable() => Animator.SetTrigger("Open");
    public virtual void Close() => Animator.SetTrigger("Close");
    public virtual void DisablePage() => gameObject.SetActive(false);
}
