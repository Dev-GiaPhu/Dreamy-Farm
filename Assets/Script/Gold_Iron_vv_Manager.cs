using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using UnityEngine.SceneManagement; // Thêm thư viện

public class Gold_Iron_vv_Manager : MonoBehaviour
{
    public static Gold_Iron_vv_Manager Instance;
    
    // Khai báo biến
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
    private const float AUTOSAVE_INTERVAL = 5f; // Đặt hằng số cho khoảng thời gian tự động lưu

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Đăng ký sự kiện Tải Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Khởi tạo và tải dữ liệu cho scene đầu tiên
        LoadResources(); 
        
        // Bắt đầu chu trình AutoSave
        if (Saving == false)
        {
            Saving = true;
            StartCoroutine(AutoSave());
        }
    }

    void Update()
    {
        // Chỉ cập nhật UI (Đảm bảo TextMeshProUGUI đã được tìm thấy)
        if (EmeralText != null) EmeralText.text = Emeral.ToString();
        if (GoldText != null) GoldText.text = Gold.ToString();
        if (IronText != null) IronText.text = Iron.ToString();
        if (WoodText != null) WoodText.text = Wood.ToString();
    }

    /// <summary>
    /// HÀM MỚI: Được gọi bởi HouseTrigger để CHUYỂN SCENE
    /// </summary>
    public void LoadAndChangeScene(string sceneName)
    {
        // BƯỚC 1: LƯU TÀI NGUYÊN (NẾU CẦN)
        SaveResources(); 
        
        // BƯỚC 2: GỌI HÀM CHUYỂN SCENE AN TOÀN TRONG DATAMANAGER
        // Giả sử GameDataManager có hàm này để quản lý Save/Load Position của Player
        if (GameDataManager.Instance != null)
        {
             GameDataManager.Instance.ChangeSceneAndSave(sceneName);
        }
        else
        {
            Debug.LogError("GameDataManager.Instance is NULL! Cannot change scene safely.");
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// HÀM QUAN TRỌNG: Xử lý sau khi Scene tải xong
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            // BƯỚC 3: TÌM LẠI UI VÀ TẢI DỮ LIỆU TÀI NGUYÊN MỚI
            // Cần tìm lại UI trong Scene mới
            EmeralText = GameObject.FindWithTag("Number Emerral")?.GetComponent<TextMeshProUGUI>();
            GoldText = GameObject.FindWithTag("Number Gold")?.GetComponent<TextMeshProUGUI>();
            IronText = GameObject.FindWithTag("Number Iron")?.GetComponent<TextMeshProUGUI>();
            WoodText = GameObject.FindWithTag("Number Wood")?.GetComponent<TextMeshProUGUI>();
            
            LoadResources();
            Debug.Log($"[Gold_Iron_vv_Manager] Đã tìm và cập nhật UI cho Scene: {scene.name}");
        }
    }

    // Hàm Save/Load Tài nguyên (Tách biệt khỏi Logic Chuyển Scene)
    public void SaveResources()
    {
        PlayerPrefs.SetInt("Emeral", Emeral);
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("Iron", Iron);
        PlayerPrefs.SetInt("Wood", Wood);
        PlayerPrefs.Save(); 
        Debug.Log("[Gold_Iron_vv_Manager] Đã LƯU tài nguyên.");
    }

    public void LoadResources()
    {
        if(!PlayerPrefs.HasKey("Emeral") || !PlayerPrefs.HasKey("Gold") || !PlayerPrefs.HasKey("Iron") || !PlayerPrefs.HasKey("Wood"))
        {
            Emeral = 0;
            Gold = 0;
            Iron = 0;
            Wood = 0;
            Debug.Log("[Gold_Iron_vv_Manager] Khởi tạo tài nguyên = 0.");
        }
        else
        {
            Emeral = PlayerPrefs.GetInt("Emeral");
            Gold = PlayerPrefs.GetInt("Gold");
            Iron = PlayerPrefs.GetInt("Iron");
            Wood = PlayerPrefs.GetInt("Wood");
            Debug.Log("[Gold_Iron_vv_Manager] Đã tải tài nguyên thành công.");
        }
    }

    public IEnumerator AutoSave()
    {
        while (true) 
        {
            yield return new WaitForSeconds(AUTOSAVE_INTERVAL); 
            SaveResources(); // Tự động lưu tài nguyên
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        SaveResources(); // Lưu lần cuối
    }
}