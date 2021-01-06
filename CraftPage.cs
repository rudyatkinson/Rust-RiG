using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CraftPage : Page, IPageContent, IBack
{
    public static CraftPage _Instance { get; private set; }
    public override Animator Animator { get; set; }
    public override IPageContent IPage { get; set; }

    [SerializeField] private GameObject prefixItem;
    [SerializeField] private GameObject prefixItemParent;

    public Dictionary<Item, GameObject> instantiatedItems { get; private set; } = new Dictionary<Item, GameObject>();
    public List<Item> addedItems { get; private set; } = new List<Item>();

    public InputField searchBar;
    public Button calculateAllButton;
    public Button resetButton;

    public Color32 usableButtonColor = new Color32(206, 65, 43, 255);
    public Color32 nonUsableButtonColor = new Color32(206, 65, 43, 100);

    [SerializeField] private NavigationPager navigation;

    private void Awake()
    {
        _Instance = _Instance ?? this;
        IPage = IPage ?? this;
        Animator = Animator ?? GetComponent<Animator>();
        InitPage();

        gameObject.SetActive(false);
    }

    public override void OnEnable()
    {
        AddAsEvent();
        base.OnEnable();
        IsCalculateAllUsable();
    }

    public void InitPage()
    {
        foreach(Item item in Database._Instance.orderedItems)
        {
            if (!item.isCraftable) continue;

            GameObject tempItem = Instantiate(prefixItem, prefixItemParent.transform);
            tempItem.GetComponent<ItemActivity>().SetItem(item);
            instantiatedItems.Add(item, tempItem);
        }
    }

    public void FilterItems(NavigationPageEventArgs e)
    {
        if(e.currentNavigationType == ItemTypes.All) EnableAllItems();
        else
        {
            foreach (GameObject go in instantiatedItems.Values)
            {
                if (go.GetComponent<ItemActivity>().item.type == navigation.CurrentNavigation)
                {
                    go.SetActive(true);
                }
                else
                    go.SetActive(false);
            }
        }
    }

    public void Search()
    {
        string search = searchBar.text.ToLower();
        foreach(GameObject go in instantiatedItems.Values)
        {
            Item item = go.GetComponent<ItemActivity>().item;
            ItemTypes currentNavigation = navigation.CurrentNavigation;

            if (currentNavigation != ItemTypes.All &&
                item.type != currentNavigation)
            {
                go.SetActive(false);
                continue;
            }

            if(item.inGameName.ToLower().Contains(search))
                go.SetActive(true);
            else 
                go.SetActive(false);
        }
    }

    public void CleanSearch()
    {
        searchBar.text = "";
    }

    public void Reset()
    {
        GameObject[] gameObjects = new GameObject[addedItems.Count];
        int index = 0;

        foreach(var i in addedItems)
            gameObjects[index++] = instantiatedItems[i];

        foreach(GameObject go in gameObjects)
        {
            bool activeSelf = go.activeSelf;
            if (!activeSelf) go.SetActive(true);
            ItemActivity item = go.GetComponent<ItemActivity>();
            item.ResetCounter();
            go.SetActive(activeSelf);
        }
    }

    void DisableAllItems()
    {
        foreach (GameObject go in instantiatedItems.Values) go.SetActive(false);
    }
    void EnableAllItems()
    {
        foreach (GameObject go in instantiatedItems.Values) go.SetActive(true);
    }

    public void AddAsEvent() => BackButton._Instance.AddEvent(this);

    public void Back()
    {
        BurgerMenu._Instance.BackButtonCalled();
        Animator.SetTrigger("Close");
    }

    public override void DisablePage()
    {
        base.DisablePage();
    }

    public void IsCalculateAllUsable()
    {
        foreach(GameObject item in instantiatedItems.Values)
        {
            if (item.GetComponent<ItemActivity>().TotalNeed > 0)
            {
                calculateAllButton.interactable = true;
                resetButton.interactable = true;
                calculateAllButton.gameObject.transform.GetChild(0).GetComponent<Text>().color = usableButtonColor;
                resetButton.gameObject.transform.GetChild(0).GetComponent<Text>().color = usableButtonColor;
                return;
            }
        }
        calculateAllButton.interactable = false;
        resetButton.interactable = false;
        calculateAllButton.gameObject.transform.GetChild(0).GetComponent<Text>().color = nonUsableButtonColor;
        resetButton.gameObject.transform.GetChild(0).GetComponent<Text>().color = nonUsableButtonColor;
    }

    public void ItemAdded(Item item, int count)
    {
        if (!addedItems.Contains(item) &&
            count > 0)
            addedItems.Add(item);
        else if (addedItems.Contains(item) &&
            count <= 0)
            addedItems.Remove(item);
    }
}
