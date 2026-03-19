using UnityEngine;
using System.Collections;

public class TrapManager : MonoBehaviour
{
    [Header("Danh sách Lư Hương (Bát)")]
    public BambooBowl bowl1;
    public BambooBowl bowl2;
    public BambooBowl bowl3;

    [Header("Cài đặt chướng ngại vật")]
    public GameObject objectToDestroy;
    public float fadeDuration = 1.5f;

    [Header("Âm thanh (SFX)")]
    public AudioClip solveAllSound; // Tiếng nhạc hào quang khi giải xong cả 3

    private bool trapTriggered = false;

    void Update()
    {
        if (trapTriggered) return;
        CheckResult();
    }

    void CheckResult()
    {
        if (bowl1.isSolved && bowl2.isSolved && bowl3.isSolved)
        {
            TriggerTrap();
        }
    }

    void TriggerTrap()
    {
        trapTriggered = true;

        // --- PHÁT NHẠC THẮNG LỢI ---
        if (solveAllSound != null) AudioSource.PlayClipAtPoint(solveAllSound, transform.position);

        if (objectToDestroy != null)
        {
            StartCoroutine(FadeAndDestroy(objectToDestroy, fadeDuration));
        }
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();
        Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;

        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) startColors[i] = renderers[i].color;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            for (int i = 0; i < renderers.Length; i++)
            {
                Color newColor = startColors[i];
                newColor.a = alpha;
                renderers[i].color = newColor;
            }
            yield return null;
        }
        Destroy(target);
    }
}