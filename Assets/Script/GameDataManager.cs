using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    private GameObject player;
    private float x;
    private float y;
    private float z;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // GIỮ LẠI KHI CHUYỂN SCENE
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if( player != null && (PlayerPrefs.HasKey($"PosX{SceneManager.GetActiveScene().name}") && PlayerPrefs.HasKey($"PosY{SceneManager.GetActiveScene().name}") && PlayerPrefs.HasKey($"PosZ{SceneManager.GetActiveScene().name}")))
        {
            x = PlayerPrefs.GetFloat($"PosX{SceneManager.GetActiveScene().name}");
            y = PlayerPrefs.GetFloat($"PosY{SceneManager.GetActiveScene().name}");
            z = PlayerPrefs.GetFloat($"PosZ{SceneManager.GetActiveScene().name}");
            player.transform.position = new Vector3(x, y, z);
        }
    }

    private void Update()
    {
        if(player != null && Input.GetKeyDown(KeyCode.S))
        {
            PlayerPrefs.SetFloat($"PosX{SceneManager.GetActiveScene().name}", player.transform.position.x);
            PlayerPrefs.SetFloat($"PosY{SceneManager.GetActiveScene().name}", player.transform.position.y);
            PlayerPrefs.SetFloat($"PosZ{SceneManager.GetActiveScene().name}", player.transform.position.z);
        }
    }
}