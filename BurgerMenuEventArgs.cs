using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurgerMenuEventArgs : EventArgs
{
    public Pager.PageType? newPageType { get; private set; }
    public bool? isOpen { get; private set; }
    public bool? isBackButton { get; private set; }

    public BurgerMenuEventArgs(Pager.PageType? newPageType, 
        bool? isOpen, bool? isBackButton)
    {
        this.newPageType = newPageType;
        this.isOpen = isOpen;
        this.isBackButton = isBackButton;
    }

    public BurgerMenuEventArgs(bool isOpen) : 
        this(null, isOpen, null) { }

    public BurgerMenuEventArgs(Pager.PageType newPageType) : 
        this(newPageType, null, null) { }

    public BurgerMenuEventArgs(bool isOpen, bool isBackButton) :
        this(null, isOpen, isBackButton) { }
}
