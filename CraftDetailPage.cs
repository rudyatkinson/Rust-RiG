using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public struct TotalCost
{
    public int cost;
}

// Detail Page requires Animator
[RequireComponent(typeof(Animator))]
public class CraftDetailPage : MonoBehaviour, IBack
{
    public Animator anim { get; private set; }
    public CraftDetailPopup popup;

    [Header("Prefab Instantiate Transforms")]
    public Transform itemIconTransform;
    public Transform itemTotalCostTransform;
    [Space]
    [Header("Prefabs")]
    public GameObject itemIconPrefab;
    public GameObject itemTotalCostPrefab;
    [Space]
    #region Collections
    public Dictionary<Item, CostInstance> instantiatedCostObjects { get; private set; } = new Dictionary<Item, CostInstance>();
    public List<GameObject> instantiatedIconObjects { get; private set; } = new List<GameObject>();
    public List<ItemActivity> addedItems { get; private set; }
    public Dictionary<Item, CostsEventArgs> costs { get; private set; }
    public List<DetailItemActivity> selectedItems { get; private set; }
    #endregion

    #region CachedColors
    public Color32 selectedItemColor = new Color32(43, 98, 143, 255);
    public Color32 nonSelectedItemColor = new Color32(255, 255, 255, 100);
    #endregion
    
    public Button selectedItemsResetButton;

    private void Awake()
    {
        //_Instance = _Instance ?? this;
        anim = anim ?? GetComponent<Animator>();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        selectedItems = new List<DetailItemActivity>();
        AddAsEvent();
        addedItems = null;
        addedItems = FilterItems();
        CreateItems();
        MergeCosts();
        CreateTotalCosts();
        IsSelectedItemsResettable();
    }

    /// <summary>
    /// Filter added items and return as list collection.
    /// </summary>
    /// <returns></returns>
    List<ItemActivity> FilterItems()
    {
        List<ItemActivity> tempList = new List<ItemActivity>();
        foreach (GameObject go in CraftPage._Instance.instantiatedItems.Values)
        {
            ItemActivity iA = go.GetComponent<ItemActivity>();
            if (iA.itemAddedCount > 0)
                tempList.Add(iA);
        }
        return tempList
            .OrderByDescending(x => x.item.isCraftable)
            .ThenBy(x => x.item.type)
            .ToList();
    }

    /// <summary>
    /// All added items will be instantiated as icon with itemActivity.
    /// </summary>
    void CreateItems()
    {
        foreach(ItemActivity iA in addedItems)
        {
            if (iA.itemAddedCount <= 0) continue;

            // Item icon created and itemIcon attributes set.
            GameObject go = Instantiate(itemIconPrefab, itemIconTransform);
            go.transform.GetChild(0).GetComponent<Image>().sprite = iA.item.image;
            go.transform.GetChild(1).GetComponent<Text>().text = iA.itemAddedCount.ToString();
            // DetailItemActivity component added to gameObject and given as activity.
            DetailItemActivity dIA = go.AddComponent<DetailItemActivity>();
            dIA.itemActivity = iA;
            // SelectedItemChanged function given as event of button component.
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => SelectedItemChanged(dIA));
            // instantiated object added to collection for fastest reset functionality.
            instantiatedIconObjects.Add(go);
        }
    }

    /// <summary>
    /// Costs of added items will be merged in costs collection.
    /// </summary>
    void MergeCosts()
    {
        costs = new Dictionary<Item, CostsEventArgs>();
        foreach(ItemActivity iA in addedItems)
        {
            if (iA.item.type == ItemTypes.Components) continue;

            int index = 0;
            foreach(Item need in iA.item.requiredItems)
            {
                int value = iA.item.requiredItemCount[index] * (iA.itemAddedCount / iA.item.craftSize);

                if (!costs.ContainsKey(need)) costs.Add(need, new CostsEventArgs(value));
                else costs[need].value += value;

                index++;
            }
        }
    }

    /// <summary>
    /// All merged costs which in costs collection will be instantiated via CreateCostInstance.
    /// </summary>
    void CreateTotalCosts()
    {
        foreach(KeyValuePair<Item, CostsEventArgs> item in costs) 
            CreateCostInstance(item.Key, item.Value);
    }

    /// <summary>
    /// Function instantiates given item as cost instance.
    /// </summary>
    /// <param name="item">This item will be instantiated.</param>
    /// <param name="cost">CostsEventArgs includes base value and current values of item.</param>
    private void CreateCostInstance(Item item, CostsEventArgs cost) 
    {
        // Creates an instance for given item and Image and text properties of
        // instance takes the parameters of given item.
        GameObject go = Instantiate(itemTotalCostPrefab, itemTotalCostTransform);
        go.transform.GetChild(1).GetComponent<Image>().sprite = item.image;
        go.transform.GetChild(2).GetComponent<Text>().text = item.inGameName;
        go.transform.GetChild(3).GetComponent<Text>().text = cost.value.ToString();
        // if given item craftable then algorithm adds exclusive components to
        // instance and give them their functions.
        if (item.isCraftable)
        {
            go.transform.GetChild(0).gameObject.SetActive(true);
            Button button = go.AddComponent<Button>();
            button.onClick.AddListener(() =>
            {
                // exclusive detail popup will be open when click on item.
                popup.OpenPopup(item, 
                    new TotalCost { cost = cost.value});
            });
            go.transform.GetChild(4).gameObject.SetActive(true);
            Toggle toggle = go.transform.GetChild(4).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener((isBool) =>
            {
                // if toggle is on than required items of current item will be added to
                // other requirements.
                int index = 0;
                foreach(Item i in item.requiredItems)
                {
                    // calculates count of requirement
                    int subValue = item.requiredItemCount[index] * (instantiatedCostObjects[item].currentValue / item.craftSize);
                    // if dictionary already has this item then modify count.
                    if (instantiatedCostObjects.ContainsKey(i))
                    {
                        if (isBool) ModifyCostInstance(i, subValue);
                        else ModifyCostInstance(i, -subValue);
                    }
                    // otherwise algorithm creates a different instance of this item.
                    else CreateCostInstance(i, new CostsEventArgs(subValue));
                    // after all toggle function isAdded will be boolean for next iteration.
                    cost.isAdded = isBool;
                    index++;
                }
            });
        }
        instantiatedCostObjects.Add(item, new CostInstance(go, cost.value));
    }

    /// <summary>
    /// Function modifies cost value if an item added before.
    /// </summary>
    /// <param name="item">The Item you want to modify</param>
    /// <param name="value">The value modified</param>
    void ModifyCostInstance(Item item, int value)
    {
        CostInstance instance = instantiatedCostObjects[item];
        Text costText = instance.instanceObject.transform.GetChild(3).GetComponent<Text>();

        instance.currentValue += value;
        if (instance.currentValue <= 0)
        {
            Destroy(instantiatedCostObjects[item].instanceObject);
            instantiatedCostObjects.Remove(item);
        }
        else costText.text = instance.currentValue.ToString();

        if (costs.ContainsKey(item) && 
            costs[item].isAdded)
        {
            RefreshAddedItem(item, value);
        }

        if(item.isCraftable)
        {
            Button button = instance.instanceObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                // exclusive detail popup will be open when click on item.
                popup.OpenPopup(item, 
                    new TotalCost { cost = instance.currentValue});
            });
        }
            
    }

    /// <summary>
    /// Required items of given item will be added as cost.
    /// </summary>
    /// <param name="item">The item which has requirements.</param>
    /// <param name="value">The value current cost of item.</param>
    void RefreshAddedItem(Item item, int value)
    {
        int index = 0;
        foreach (Item i in item.requiredItems)
        {

            int subValue = item.requiredItemCount[index] * (value / item.craftSize);
            if (instantiatedCostObjects.ContainsKey(i))
                ModifyCostInstance(i, subValue);
            else CreateCostInstance(i, new CostsEventArgs(subValue));
            index++;
        }
    }
       
    private void OnDisable()
    {
        foreach (GameObject go in instantiatedIconObjects)
            Destroy(go);
        instantiatedIconObjects.Clear();
        ClearCost();
    }

    void ClearCost()
    {
        foreach (CostInstance instance in instantiatedCostObjects.Values)
            Destroy(instance.instanceObject);
        instantiatedCostObjects.Clear();
    }

    /// <summary>
    /// Recalculates costs of selected or nonselected item.
    /// </summary>
    /// <param name="dIA">Selected item's DetailItemActivity component.</param>
    public void SelectedItemChanged(DetailItemActivity dIA)
    {
        if (selectedItems.Contains(dIA))
        {
            // if selectedItems already has this item, list deletes item from itself and
            selectedItems.Remove(dIA);
            dIA.GetComponent<Image>().color = nonSelectedItemColor;
            ClearCost();
            // if there are no any item in the list then resets all of calculations.
            if (selectedItems.Count <= 0) MergeCosts();
            // otherwise, calculates the selected items.
            else CreateCostForSingleItem(selectedItems.ToArray());
            CreateTotalCosts();
            IsSelectedItemsResettable();
            return;
        }
        // if item is not added to the list before, then add and set properties as selectedItem.
        selectedItems.Add(dIA);
        selectedItems.Last().GetComponent<Image>().color = selectedItemColor;
        // calculates and shows the craft costs to players.
        ClearCost();
        CreateCostForSingleItem(selectedItems.ToArray());
        CreateTotalCosts();
        IsSelectedItemsResettable();
    }

    /// <summary>
    /// Filter and instantiates costs if there are any selected items.
    /// </summary>
    /// <param name="dIA"></param>
    private void CreateCostForSingleItem(params DetailItemActivity[] dIA)
    {
        costs = new Dictionary<Item, CostsEventArgs>();
        foreach (DetailItemActivity activities in dIA)
        {
            int index = 0;
            foreach (Item need in activities.itemActivity.item.requiredItems)
            {
                Item item = activities.itemActivity.item;
                int value = item.requiredItemCount[index] * (activities.itemActivity.itemAddedCount / item.craftSize);

                if (!costs.ContainsKey(need)) costs.Add(need, new CostsEventArgs(value));
                else costs[need].value += value;

                index++;
            }
        }
    }

    /// <summary>
    /// if there are any selected item in the list then resets all of selected items
    /// and set their color properties as nonSelectedItem.
    /// </summary>
    public void ResetSelectedItems()
    {
        foreach(DetailItemActivity dIA in selectedItems)
            dIA.GetComponent<Image>().color = nonSelectedItemColor;
        if (selectedItems.Count <= 0) return;

        selectedItems.Clear();
        ClearCost();
        MergeCosts();
        CreateTotalCosts();
        IsSelectedItemsResettable();
    }

    public void IsSelectedItemsResettable() => selectedItemsResetButton.gameObject.SetActive(selectedItems.Count > 0);

    public void AddAsEvent() => BackButton._Instance.AddEvent(this);
    public void Back() => anim.SetTrigger("Close");
    public void BackButtonCalled()
    {
        Back();
        BackButton._Instance.DeleteLastEvent();
    }

    public void Deactive() => gameObject.SetActive(false);
}
