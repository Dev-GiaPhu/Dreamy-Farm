using UnityEngine;

public class attackNhim : MonoBehaviour
{
    public HedgehogController hedgehogController => GetComponentInParent<HedgehogController>();
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hedgehogController.Die) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>()?.Hit(hedgehogController.attackDamage);
            Debug.Log(hedgehogController.hedgehogName + " đã tấn công người chơi!");
        }
    }
}
