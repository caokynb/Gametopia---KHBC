using UnityEngine;
using System.Collections;

public class BambooBowl : MonoBehaviour
{
    public int currentBamboo = 0;
    public LayerMask bambooLayer;

    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & bambooLayer.value) != 0)
        {
            currentBamboo++;
            Destroy(collision.gameObject);
        }
    }

    public void ResetBowl()
    {
        currentBamboo = 0;
        if (sr != null) StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        sr.color = originalColor;
    }
}