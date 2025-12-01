using UnityEngine;

public class metalCrusher : MonoBehaviour
{
    [SerializeField] private float upwardForce = 500f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Crusher"))
        {
            rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
        }
    }
}
