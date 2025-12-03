using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Gold_Iron_vv_Manager : MonoBehaviour
{
    [Header("Number Items Manager")]
    public int Emeral = 0;
    public int Gold = 0;
    public int Iron = 0;
    public int Wood = 0;

    [Header("UI Show")]
    public TextMeshProUGUI EmeralText;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI IronText;
    public TextMeshProUGUI WoodText;

    void Start()
    {
        EmeralText.text = "...Loading";
        GoldText.text = "...Loading";
        IronText.text = "...Loading";
        WoodText.text = "...Loading";

    }
    void Update()
    {
        EmeralText.text = Emeral.ToString();
        GoldText.text = Gold.ToString();
        IronText.text = Iron.ToString();
        WoodText.text = Wood.ToString();
    }
}
