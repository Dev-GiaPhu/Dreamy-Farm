using UnityEngine;
using TMPro;

public class HouseTriggerTMP : MonoBehaviour
{
    [Header("Animator & UI")]
    public Animator animator;
    public string sceneName = "";
    public TextMeshProUGUI interactText;  // Dùng TMP

    private bool Open = false;

    private void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player va cham!");
            Open = true;
            if (animator != null)
                animator.SetBool("Open", true);
            if (interactText != null)
                interactText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Open = false;
            if (animator != null)
                animator.SetBool("Open", false);
            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Open && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player vào nhà!");

            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
