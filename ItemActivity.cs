using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void GUIActivities();

public class ItemActivity : MonoBehaviour
{
    #region ItemInfoAndSet
    public Item item { get; private set; }

    public void SetItem(Item item)
    {
        this.item = item;
        InitItem();
    }
    #endregion

    event GUIActivities GUIHandler;

    public Text ItemNameText;
    public Image ItemPNGImage;
    public GameObject ItemCountBackground;
    public InputField ItemCountText;
    public GameObject ResetButton;
    public GameObject InfoButton;
    public GameObject ItemCountDecreaseButton;
    public Image ItemWorkbenchLevelImage;
    public Image ItemWorkbenchLevelRomanImage;
    public GameObject CostArea;
    public GameObject CostObjectPrefix;
    public Dictionary<Item, GameObject> InstantiatedCostObjects = new Dictionary<Item, GameObject>();

    public int itemAddedCount { get; private set; }
    public int itemRecycledCount { get; private set; }

    public Dictionary<Item, ItemReferenceEventArgs> references  = new Dictionary<Item, ItemReferenceEventArgs>();
    public int ReferenceNeed()
    {
        if (references.Count <= 0) return 0;
        //Debug.Log($"item {item}, references {references.Count}");
        int result = 0;
        foreach (ItemReferenceEventArgs e in references.Values)
            result += e.need;

        return result;
    }
    public int TotalNeed { get { return itemAddedCount + ReferenceNeed(); } }

    private void Awake()
    {
        GUIHandler += DecreaseButtonActivity;
        GUIHandler += CountBackgroundActivity;
        GUIHandler += ResetButtonActivity;
        //GUIHandler += InfoButtonActivity;
    }

    private void InitItem()
    {
        ItemNameText.text = item.inGameName;
        ItemPNGImage.sprite = item.image;
        ItemCountText.text = itemAddedCount.ToString();

        if (item.workbench > 0) ItemWorkbenchLevelImage.sprite = Database._Instance.workbenchSprites[(int)item.workbench - 1];
        else ItemWorkbenchLevelImage.gameObject.SetActive(false);
            
        DeleteCostObjects();
    }

    public void Add()
    {
        switch(Pager._Instance.CurrentPage)
        {
            case Pager.PageType.Craft:
                ItemCountText.text = ((itemAddedCount += item.craftSize) + ReferenceNeed()).ToString();
                PushReference();
                CalculateCost();
                CraftPage._Instance.IsCalculateAllUsable();
                CraftPage._Instance.ItemAdded(item, itemAddedCount);
                break;
            case Pager.PageType.Recycle:
                ItemCountText.text = (++itemRecycledCount).ToString();
                CalculateRecycle();
                RecyclePage._Instance.IsCalculateAllUsable();
                RecyclePage._Instance.ItemAdded(item, itemRecycledCount);
                break;
        }
        GUIHandler();
    }

    public void Decrease()
    {
        switch(Pager._Instance.CurrentPage)
        {
            case Pager.PageType.Craft:
                if (itemAddedCount > 0) ItemCountText.text = ((itemAddedCount -= item.craftSize) + ReferenceNeed()).ToString();
                else ItemCountText.text = ReferenceNeed().ToString();
                if (TotalNeed > 0)
                {
                    PushReference();
                    CalculateCost();
                }
                else
                {
                    DeleteCostObjects();
                    DeleteReference(item);
                }
                CraftPage._Instance.IsCalculateAllUsable();
                CraftPage._Instance.ItemAdded(item, itemAddedCount);
                break;
            case Pager.PageType.Recycle:
                if (itemRecycledCount > 0)
                {
                    ItemCountText.text = (--itemRecycledCount).ToString();
                    CalculateRecycle();
                }
                RecyclePage._Instance.IsCalculateAllUsable();
                RecyclePage._Instance.ItemAdded(item, itemRecycledCount);
                if (itemRecycledCount <= 0) DeleteCostObjects();
                break;
        }

        GUIHandler();
    }

    void CalculateCost()
    {            
        int count = 0;
        foreach(Item item in item.requiredItems)
        {
            if(!InstantiatedCostObjects.ContainsKey(item))
            {
                GameObject go = Instantiate(CostObjectPrefix, CostArea.transform);
                if (item.isCraftable) go.transform.GetChild(0).gameObject.SetActive(true);
                go.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = item.image;
                go.transform.GetChild(2).gameObject.GetComponent<Text>().text =
                    (this.item.requiredItemCount[count] * (TotalNeed / this.item.craftSize)).ToString();
                InstantiatedCostObjects.Add(item, go);
            }
            else
            {
                InstantiatedCostObjects[item].transform.GetChild(2).gameObject.GetComponent<Text>().text = 
                    (this.item.requiredItemCount[count] * (TotalNeed / this.item.craftSize)).ToString();
            }
            count++;
        }
    }

    void CalculateRecycle()
    {
        int count = 0;
        foreach(Item item in item.recycledItem)
        {
            if(!InstantiatedCostObjects.ContainsKey(item))
            {
                GameObject go = Instantiate(CostObjectPrefix, CostArea.transform);
                if (item.recycledItem.Length > 0) go.transform.GetChild(0).gameObject.SetActive(true);
                go.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = item.image;
                go.transform.GetChild(2).gameObject.GetComponent<Text>().text =
                    (this.item.recycledItemCount[count] * itemRecycledCount).ToString();
                InstantiatedCostObjects.Add(item, go);
            }
            else
            {
                InstantiatedCostObjects[item].transform.GetChild(2).gameObject.GetComponent<Text>().text =
                    (this.item.recycledItemCount[count] * itemRecycledCount).ToString();
            }
            count++;
        }
    }

    void PushReference()
    {
        if(!item.isCraftable) return;
        if(item.requiredItems.Length > 0)
        {
            int index = 0;
            foreach(Item item in item.requiredItems)
            {
                if (item.isCraftable)
                {
                    
                    ItemActivity target = CraftPage._Instance.instantiatedItems[item].GetComponent<ItemActivity>();
                    target.AddReference(new ItemReferenceEventArgs(this.item, this.item.requiredItemCount[index] * ((itemAddedCount + ReferenceNeed())) / this.item.craftSize));
                    //Debug.Log($"target: {target.item.inGameName}, {item.inGameName}, {this.item.requiredItemCount[index]}");
                }
                index++;
            }
        }
    }

    public void AddReference(ItemReferenceEventArgs e)
    {
        if (!item.isCraftable) return;

        if(!references.ContainsKey(e.mainItem)) references.Add(e.mainItem, e);
        else references[e.mainItem] = e;

        if(item.requiredItems.Length > 0)
        {
            int index = 0;
            foreach(Item item in item.requiredItems)
            {
                if (item.isCraftable)
                {
                    //Debug.Log($"this{this.item}, count{this.item.requiredItemCount[index]}");
                    ItemReferenceEventArgs newE = 
                        new ItemReferenceEventArgs(this.item, this.item.requiredItemCount[index] * (itemAddedCount + ReferenceNeed()));
                    CraftPage._Instance.instantiatedItems[item].GetComponent<ItemActivity>().AddReference(newE);
                }
                index++;
            } 
        }
        //Debug.Log($"main {e.mainItem}, need {e.need}, item {item}");
        ItemCountText.text = (itemAddedCount  + ReferenceNeed()).ToString();
        CalculateCost();
        GUIHandler();
    }
    
    public void DeleteReference(Item mainItem)
    {
        if (!item.isCraftable) return;
        
        if (references.Count > 0 && references.ContainsKey(mainItem)) references.Remove(mainItem);

        if(ReferenceNeed() <= 0)
        {
            if (item.requiredItems.Length > 0)
            {
                foreach (Item item in item.requiredItems)
                {
                    if (!item.isCraftable) continue;
                    CraftPage._Instance.instantiatedItems[item].GetComponent<ItemActivity>().DeleteReference(this.item);
                }
            }
        }
        else
        {
            PushReference();
        }
        ItemCountText.text = (itemAddedCount + ReferenceNeed()).ToString();
        GUIHandler();
        CalculateCost();
        if (itemAddedCount <= 0 && ReferenceNeed() <= 0) DeleteCostObjects();
    }

    void DeleteCostObjects()
    {
        foreach(GameObject go in InstantiatedCostObjects.Values)
        {
            Destroy(go);
        }
        InstantiatedCostObjects = new Dictionary<Item, GameObject>();
    }

    public void ResetCounter()
    {
        itemAddedCount = 0;
        itemRecycledCount = 0;
        ItemCountText.text = (itemAddedCount + ReferenceNeed()).ToString();

        DeleteReference(item);
        GUIHandler();

        if (itemAddedCount > 0 || ReferenceNeed() > 0) CalculateCost();
        else DeleteCostObjects();

        switch(Pager._Instance.CurrentPage)
        {
            case Pager.PageType.Craft:
                CraftPage._Instance.IsCalculateAllUsable();
                CraftPage._Instance.ItemAdded(item, itemAddedCount);
                break;
            case Pager.PageType.Recycle:
                RecyclePage._Instance.IsCalculateAllUsable();
                RecyclePage._Instance.ItemAdded(item, itemRecycledCount);
                break;
        }
    }

    public void ItemCountChanged()
    {
        int input;
        int.TryParse(ItemCountText.text, out input);
        if (ItemCountText.text.Length > 0 &&
            input >= 0)
        {
            switch(Pager._Instance.CurrentPage)
            {
                case Pager.PageType.Craft:
                    itemAddedCount = input - ReferenceNeed();
                    ItemCountText.text = (itemAddedCount + ReferenceNeed()).ToString();
                    CalculateCost();
                    PushReference();
                    CraftPage._Instance.IsCalculateAllUsable();
                    CraftPage._Instance.ItemAdded(item, itemAddedCount);
                    break;
                case Pager.PageType.Recycle:
                    itemRecycledCount = input;
                    ItemCountText.text = itemRecycledCount.ToString();
                    CalculateRecycle();
                    RecyclePage._Instance.IsCalculateAllUsable();
                    RecyclePage._Instance.ItemAdded(item, itemRecycledCount);
                    break;
            }
            GUIHandler();
        }
        else
        {
            itemAddedCount = 0;
            DeleteCostObjects();
            DeleteReference(item);
            GUIHandler();
            return;
        }
    }

    void DecreaseButtonActivity()
    {
        if(itemAddedCount > 0 || itemRecycledCount > 0) ItemCountDecreaseButton.SetActive(true);
        else ItemCountDecreaseButton.SetActive(false);
    }

    void CountBackgroundActivity()
    {
        if(ReferenceNeed() + itemAddedCount > 0 || itemRecycledCount > 0) ItemCountBackground.SetActive(true);
        else ItemCountBackground.SetActive(false);
    }

    void ResetButtonActivity()
    {
        if(itemAddedCount > 1 || itemRecycledCount > 1) ResetButton.SetActive(true);
        else ResetButton.SetActive(false);
    }
}
