using UnityEngine;
using Unity.RemoteConfig;
using UnityEngine.UI;


#region Delegations
// Define delegations here
public delegate void BurgerMenuHandler(BurgerMenuEventArgs e);
public delegate void ChangePageHandler(Pager.PageType pageType);
#endregion

public class BurgerMenu : MonoBehaviour
{
    #region Instance
    public static BurgerMenu _Instance { get; private set; }
    #endregion

    #region Objects
    public GameObject _Menu { get; private set; }
    #endregion

    #region Events
    public event BurgerMenuHandler BurgerMenuActivation;
    #endregion

    #region Components
    public Animator Animator { get; private set; }
    #endregion

    private void Awake()
    {
        _Instance = _Instance ?? this;
        _Menu = GameObject.FindGameObjectWithTag("BurgerMenu");
        Animator = _Menu.GetComponent<Animator>();

        BurgerMenuActivation += MenuActivation;
    }

    void MenuActivation(BurgerMenuEventArgs e)
    {
        if(e.isOpen.HasValue)
        {
            if (e.isOpen.Value) _Menu.SetActive(e.isOpen.Value);
            Animator.SetTrigger("Open");
        }
        else Animator.SetTrigger("Close");
    }

    public void ChangePageButton(int pageTypeIndex)
    {
        Pager.PageType page = (Pager.PageType)pageTypeIndex;
        BurgerMenuActivation(new BurgerMenuEventArgs(page));
    }
}
