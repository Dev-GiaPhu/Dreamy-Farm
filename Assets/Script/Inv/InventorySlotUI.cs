using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ItemType
{
    Null,
    Sword,
    Axe,
    Pickaxe,
    Shovel,
    CanWater,
    FishingRod,
}
public class InventorySlotUI : MonoBehaviour
{
    public ItemType type;
    public Image icon;
    public TMP_Text amountText;

    private void Awake()
    {
        // Tự tìm Image trong con
        if (icon == null)
            icon = GetComponentInChildren<Image>();
            icon.color = new Color(1f, 1f, 1f, 0f);

        // Tự tìm TextMeshPro trong con
        if (amountText == null)
            amountText = GetComponentInChildren<TMP_Text>();
    }

    public void SetItem(ItemData data, string nameitem, int count)
    {
        if (icon == null || amountText == null)
        {
            Debug.LogError("MISSING UI REFERENCES IN: " + name);
            return;
        }

        string typeit = nameitem.ToString();
        Debug.Log("Nhat item1: " + nameitem);
        Debug.Log("Nhat item2: " + typeit);
        switch (typeit)
        {
            case "Sword":
                type = ItemType.Sword; break;
            case "Axe":
                type = ItemType.Axe; break;
            case "Pickaxe":
                type = ItemType.Pickaxe; break;
            case "Shovel":
                type = ItemType.Shovel; break;
            case "CanWater":
                type = ItemType.CanWater; break;
            case "FishingRod":
                type = ItemType.FishingRod; break;
            default:
                type = ItemType.Null; break;
        }
        icon.sprite = data.icon;
        icon.enabled = true;
        icon.color = new Color(1f, 1f, 1f, 1f);

        amountText.text = (count > 1) ? "x" + count : "";
    }

    public void ClearSlot()
    {
        if (icon == null || amountText == null)
        {
            Debug.LogError("MISSING UI REFERENCES IN: " + name);
            return;
        }

        icon.sprite = null;
        icon.color = new Color(1f, 1f, 1f, 0f);
        icon.enabled = false;
        amountText.text = "";
    }
}
