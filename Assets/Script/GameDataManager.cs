using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// GameDataManager: Quản lý Player (vị trí) và Scene Management an toàn.
/// Đảm bảo LoadPos luôn được gọi sau khi Scene mới đã tải xong.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;
    
    // Biến cache cho Player (Gameobject Player không có DontDestroyOnLoad)
    private GameObject player; 
    
    // Coroutine lưu vị trí
    private Coroutine SavingPos;
    private const float AUTOSAVE_POS_INTERVAL = 3f;

    // Biến cache để lưu tạm vị trí khi tải
    private float x, y, z; 
    
    // Thuộc tính để tham chiếu đến tên Scene hiện tại trong GameDataManager
    // (Không cần thiết nếu dùng SceneManager.GetActiveScene().name)
    // private string sceneName = ""; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Đăng ký sự kiện TẢI SCENE ngay lập tức
            SceneManager.sceneLoaded += OnSceneLoaded; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 1. Tìm PlayerController lần đầu
        player = GameObject.Find("Player");
        
        // 2. Tải vị trí cho Scene khởi đầu
        LoadPos();
        
        // 3. Bắt đầu Coroutine lưu vị trí tự động
        StartSavePos();
    }
    
    // Thay vì dùng Update để tìm Player, nên dùng OnSceneLoaded hoặc tìm trong LoadPos
    void Update()
    {
        // Không cần thiết
    }

    /// <summary>
    /// BẮT ĐẦU Coroutine lưu vị trí Player
    /// </summary>
    public void StartSavePos()
    {
        // Ngăn chặn Coroutine được gọi nhiều lần
        if(SavingPos == null)
        {
            SavingPos = StartCoroutine(SavePos());
        }
    }
    
    /// <summary>
    /// DỪNG Coroutine lưu vị trí Player
    /// </summary>
    public void StopSavingPos()
    {
        if(SavingPos != null)
        {
            StopCoroutine(SavingPos);
            SavingPos = null;
        }
    }

    /// <summary>
    /// Coroutine lưu vị trí Player tự động.
    /// </summary>
    IEnumerator SavePos()
    {
        while (true)
        {
            // Delay (Đảm bảo delay 3 giây trước khi thực hiện Save)
            yield return new WaitForSeconds(AUTOSAVE_POS_INTERVAL);
            
            if(player == null)
            {
                // Thử tìm Player nếu nó chưa được gán
                player = GameObject.Find("Player");
            }

            if(player != null)
            {
                // Lưu vị trí
                string currentSceneName = SceneManager.GetActiveScene().name;
                PlayerPrefs.SetFloat($"PosX{currentSceneName}", player.transform.position.x);
                PlayerPrefs.SetFloat($"PosY{currentSceneName}", player.transform.position.y);
                PlayerPrefs.SetFloat($"PosZ{currentSceneName}", player.transform.position.z);
                PlayerPrefs.Save();
                
                Debug.Log("Đã lưu vị trí người chơi! | Scene: " + currentSceneName + "| X: " + player.transform.position.x + " Y: " + player.transform.position.y + " Z: " + player.transform.position.z);
            }
        }
    }
    
    /// <summary>
    /// Hàm gọi từ Gold_Iron_vv_Manager để bắt đầu chuyển Scene an toàn
    /// </summary>
    public void ChangeSceneAndSave(string sceneName)
    {
        // BƯỚC 1: LƯU VỊ TRÍ CŨ NGAY LẬP TỨC TRƯỚC KHI RỜI ĐI
        SavePosImmediately();
        
        // BƯỚC 2: TẢI SCENE MỚI
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        Debug.Log($"[GameDataManager] Đang chuyển sang scene: {sceneName}");
    }

    /// <summary>
    /// Thực hiện lưu vị trí ngay lập tức (dùng khi chuyển scene)
    /// </summary>
    private void SavePosImmediately()
    {
        if (player != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetFloat($"PosX{currentSceneName}", player.transform.position.x);
            PlayerPrefs.SetFloat($"PosY{currentSceneName}", player.transform.position.y);
            PlayerPrefs.SetFloat($"PosZ{currentSceneName}", player.transform.position.z);
            PlayerPrefs.Save();
            Debug.Log($"[GameDataManager] Đã LƯU vị trí ngay lập tức cho Scene: {currentSceneName}.");
        }
        else
        {
            Debug.LogWarning("[GameDataManager] Không tìm thấy Player để lưu vị trí ngay lập tức.");
        }
    }
    
    /// <summary>
    /// HÀM ĐƯỢC KÍCH HOẠT TỰ ĐỘNG KHI MỘT SCENE MỚI ĐÃ TẢI XONG (Fix lỗi timing)
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            Debug.Log($"[GameDataManager] Scene '{scene.name}' đã tải xong. Bắt đầu TẢI vị trí đã lưu cho Scene mới.");
            
            // Dừng Coroutine SavePos hiện tại (nếu đang chạy) và khởi động lại
            StopSavingPos();
            
            // TẢI VỊ TRÍ MỚI
            LoadPos(); 
            
            // KHỞI ĐỘNG LẠI AUTOSAVE cho scene mới
            StartSavePos();
        }
    }

    /// <summary>
    /// Tải vị trí Player (Luôn đọc tên Scene hiện tại)
    /// </summary>
    public void LoadPos()
    {
        // BƯỚC 1: Tìm Player (Quan trọng: Player luôn xuất hiện sau khi Scene tải xong)
        player = GameObject.Find("Player");
        
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (player == null)
        {
            Debug.LogError($"[LoadPos] Không tìm thấy đối tượng Player trong Scene: {currentSceneName}.");
            return;
        }

        // BƯỚC 2: Kiểm tra dữ liệu và tải
        bool hasData = PlayerPrefs.HasKey($"PosX{currentSceneName}") && 
                       PlayerPrefs.HasKey($"PosY{currentSceneName}") && 
                       PlayerPrefs.HasKey($"PosZ{currentSceneName}");

        if (hasData)
        {
            x = PlayerPrefs.GetFloat($"PosX{currentSceneName}");
            y = PlayerPrefs.GetFloat($"PosY{currentSceneName}");
            z = PlayerPrefs.GetFloat($"PosZ{currentSceneName}");
            
            player.transform.position = new Vector3(x, y, z);
            
            Debug.Log("Đã tải vị trí người chơi! | Scene: "+ currentSceneName + "| X: " + x + " Y: " + y + " Z: " + z);
        }
        else
        {
             Debug.Log($"Chưa có dữ liệu vị trí người chơi cho Scene {currentSceneName}. Giữ nguyên vị trí mặc định.");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopSavingPos(); // Đảm bảo Coroutine dừng khi đối tượng bị hủy
    }

    private void OnApplicationQuit()
    {
        SavePosImmediately(); // Lưu vị trí lần cuối
    }
}