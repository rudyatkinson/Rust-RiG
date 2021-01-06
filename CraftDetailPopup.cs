using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class CraftDetailPopup : MonoBehaviour, IBack
{
    public List<Item> item { get; private set; }
    public List<int> value { get; private set; }

    public Image itemImage;
    public Text itemName;

    public GameObject costObjectPrefab;
    public Transform costObjectsParent;
    public Dictionary<Item, CostInstance> instantiatedObjects = new Dictionary<Item, CostInstance>();

    public bool isOpen { get; private set; }

    public Animator anim { get; private set; }

    private void Awake()
    {
        anim = anim ?? GetComponent<Animator>();
        isOpen = false;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        AddAsEvent();
        anim.SetTrigger("FadeIn");
        instantiatedObjects = new Dictionary<Item, CostInstance>();
        ShowLastItem();
    }

    private void ShowLastItem()
    {
        Item itemIndex = this.item[this.item.Count - 1];
        itemImage.sprite = itemIndex.image;
        itemName.text = $"<Color=orange><size=108>" +
            $"{value[value.Count - 1]}" +
            $"\n</size></color>{itemIndex.inGameName}";

        int index = 0;
        if(Pager._Instance.CurrentPage == Pager.PageType.Craft)
        {
            foreach (Item item in itemIndex.requiredItems)
            {
                int value = (itemIndex.requiredItemCount[index] * this.value[this.value.Count - 1]) / itemIndex.craftSize;
                CreateCostInstance(item, value);
                index++;
            }
        }
        else
        {
            foreach (Item item in itemIndex.recycledItem)
            {
                int value = (itemIndex.recycledItemCount[index] * this.value[this.value.Count - 1]);
                CreateCostInstance(item, value);
                index++;
            }
        }
        
    }

    void CreateCostInstance(Item item, int value)
    {
        Item itemIndex = this.item[this.item.Count - 1];
        if (!instantiatedObjects.ContainsKey(item)) instantiatedObjects.Add(item, new CostInstance());
        
        GameObject go = Instantiate(costObjectPrefab, costObjectsParent);

        go.transform.GetChild(1).GetComponent<Image>().sprite = item.image;
        go.transform.GetChild(2).GetComponent<Text>().text = item.inGameName;
        go.transform.GetChild(3).GetComponent<Text>().text = value.ToString();

        bool isContinue;
        if (Pager._Instance.CurrentPage == Pager.PageType.Craft) isContinue = item.isCraftable;
        else isContinue = item.recycledItem.Length > 0;

        if (isContinue)
        {
            go.transform.GetChild(0).gameObject.SetActive(true);
            Button button = go.AddComponent<Button>();
            button.onClick.AddListener(() =>
            {
                NextItem(item, value);
            });
            go.transform.GetChild(4).gameObject.SetActive(true);
            Toggle toggle = go.transform.GetChild(4).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener((isBool) =>
            {
                int index = 0;
                if (Pager._Instance.CurrentPage == Pager.PageType.Craft)
                {
                    foreach (Item i in item.requiredItems)
                    {
                        int subValue = item.requiredItemCount[index] * (value / item.craftSize);
                        if (instantiatedObjects.ContainsKey(i))
                        {
                            if (isBool) ModifyCostInstance(i, subValue);
                            else ModifyCostInstance(i, -subValue);
                        }
                        else CreateCostInstance(i, subValue);
                        instantiatedObjects[item].isAdded = isBool;
                        index++;
                    }
                }
                else
                {
                    foreach (Item i in item.recycledItem)
                    {
                        int subValue = item.recycledItemCount[index] * value;
                        if (instantiatedObjects.ContainsKey(i))
                        {
                            if (isBool) ModifyCostInstance(i, subValue);
                            else ModifyCostInstance(i, -subValue);
                        }
                        else CreateCostInstance(i, subValue);
                        instantiatedObjects[item].isAdded = isBool;
                        index++;
                    }
                }
            });
        }
        instantiatedObjects[item] = new CostInstance
        {
            instanceObject = go,
            rawValue = value,
            currentValue = value,
            isAdded = false
        };
    }

    void ModifyCostInstance(Item item, int value)
    {
        CostInstance instance = instantiatedObjects[item];
        Text costText = instance.instanceObject.transform.GetChild(3).GetComponent<Text>();

        instance.currentValue += value;
        if (instance.currentValue <= 0)
        {
            Destroy(instantiatedObjects[item].instanceObject);
            instantiatedObjects.Remove(item);
        }
        else costText.text = instance.currentValue.ToString();

        if (instantiatedObjects.ContainsKey(item) &&
            instantiatedObjects[item].isAdded) RefreshAddedItem(item, value);
    }

    void RefreshAddedItem(Item item, int value)
    {
        int index = 0;
        foreach (Item i in item.requiredItems)
        {
            int subValue = item.requiredItemCount[index] * (value / item.craftSize);

            if (instantiatedObjects.ContainsKey(i)) ModifyCostInstance(i, subValue);
            else CreateCostInstance(i, item.requiredItemCount[index]);

            index++;
        }
    }

    public void DeleteInstantiatedObjects()
    {
        foreach (CostInstance instance in instantiatedObjects.Values)
            Destroy(instance.instanceObject);
        instantiatedObjects = new Dictionary<Item, CostInstance>();
    }

    public void OpenPopup(Item item, TotalCost totalCost)
    {
        this.item = new List<Item>();
        this.value = new List<int>();
        this.item.Add(item);
        this.value.Add(totalCost.cost);
        isOpen = true;
        gameObject.SetActive(true);
    }

    void NextItem(Item item, int value)
    {
        AddAsEvent();
        this.item.Add(item);
        this.value.Add(value);
        DeleteInstantiatedObjects();
        ShowLastItem();
    }

    void BackItem()
    {
        item.RemoveAt(item.Count - 1);
        value.RemoveAt(value.Count - 1);
        DeleteInstantiatedObjects();
        ShowLastItem();
    }

    public void AddAsEvent() => BackButton._Instance.AddEvent(this);

    public void Back()
    {
        if (item.Count > 1) BackItem();
        else anim.SetTrigger("FadeOut");
    }

    public void BackButtonCalled()
    {
        Back();
        BackButton._Instance.DeleteLastEvent();
    }

    public void DisablePopup()
    {
        DeleteInstantiatedObjects();
        isOpen = false;
        gameObject.SetActive(false);
    }
}
