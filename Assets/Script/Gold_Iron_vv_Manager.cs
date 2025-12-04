using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Gold_Iron_vv_Manager : MonoBehaviour
{
    public static Gold_Iron_vv_Manager Instance;
    

    [Header("Number Items Manager")]
    public int Emeral;
    public int Gold;
    public int Iron;
    public int Wood;

    [Header("UI Show")]
    public TextMeshProUGUI EmeralText;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI IronText;
    public TextMeshProUGUI WoodText;

    [Header("Save")]
    private bool Saving = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Load();
    }
    void Update()
    {
        EmeralText.text = Emeral.ToString();
        GoldText.text = Gold.ToString();
        IronText.text = Iron.ToString();
        WoodText.text = Wood.ToString();

        if(Saving == false)
        {
            Saving = true;
            StartCoroutine(AutoSave());
        }
    }

    public IEnumerator AutoSave()
    {
        Saving = true;
        PlayerPrefs.SetInt("Emeral", Emeral);
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("Iron", Iron);
        PlayerPrefs.SetInt("Wood", Wood);
        yield return new WaitForSeconds(5f);
        Saving = false;
        Debug.Log($"Save Done: Emeral: {Emeral} Gold: {Gold} Iron: {Iron} Wood: {Wood}");
    }

    public void Load()
    {
        if(!PlayerPrefs.HasKey("Emeral") || !PlayerPrefs.HasKey("Gold") || !PlayerPrefs.HasKey("Iron") || !PlayerPrefs.HasKey("Wood"))
        {
            Emeral = 0;
            Gold = 0;
            Iron = 0;
            Wood = 0;
        }
        else
        {
            Emeral = PlayerPrefs.GetInt("Emeral");
            Gold = PlayerPrefs.GetInt("Gold");
            Iron = PlayerPrefs.GetInt("Iron");
            Wood = PlayerPrefs.GetInt("Wood");
        }
    }
}
