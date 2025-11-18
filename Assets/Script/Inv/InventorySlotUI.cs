using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
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

    public void SetItem(ItemData data, int count)
    {
        if (icon == null || amountText == null)
        {
            Debug.LogError("MISSING UI REFERENCES IN: " + name);
            return;
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
