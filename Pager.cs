using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pager : MonoBehaviour
{
    #region Instance
    public static Pager _Instance { get; private set; }
    #endregion

    #region PageType
    public enum PageType
    {
        Craft,
        Recycle,
        Build,
        Raid,
        Update,
        Info
    }
    public PageType CurrentPage { get; private set; } = PageType.Craft;
    #endregion

    #region PageObjects
    [SerializeField]
    private Page[] Pages;
    #endregion

    //public event ChangePageHandler ChangePage;

    private void Awake()
    {
        // Define _Instance reference for singleton.
        _Instance = _Instance ?? this;

        BurgerMenu._Instance.BurgerMenuActivation += CloseOrOpenCurrentPage;
    }

    void CloseOrOpenCurrentPage(BurgerMenuEventArgs e)
    {
        if(e.newPageType.HasValue) CurrentPage = e.newPageType.Value;

        int pageNumber = (int)CurrentPage;
        if (e.isOpen.HasValue && e.isOpen.Value)
        {
            IBack b = Pages[pageNumber] as IBack;
            b.Back();
            if(e.isBackButton.HasValue && !e.isBackButton.Value)
                BackButton._Instance.DeleteLastEvent();
                
        }
        else Pages[pageNumber].gameObject.SetActive(true);
    }
}
