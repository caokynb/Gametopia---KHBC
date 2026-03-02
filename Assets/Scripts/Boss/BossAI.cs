using UnityEngine;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, SlamAttack, SpinRam, ThrowRock, Slipped }

    [Header("Trạng thái hiện tại")]
    public BossState currentState;

    [Header("Chỉ số Sinh Tồn (Health)")]
    public float maxHealth = 15f; // Đổi thành máu thực tế của Boss
    public float currentHealth;

    [Header("Cài đặt Tốc độ (Scaling)")]
    public float maxActionCooldown = 3.0f;
    public float minActionCooldown = 0.5f;
    public float slipDuration = 5f;

    [Header("Attack 1: Đập Đất (Slam)")]
    public float runSpeed = 6f;
    public float slamRange = 2.5f;
    public float slamExplosionRadius = 3f; // Bán kính vụ nổ phá tre
    public Transform slamCenter; // Kéo object rỗng nằm ở chân/chùy của boss vào đây

    [Header("Attack 2: Húc (Spin Ram)")]
    public float ramSpeed = 15f;
    public float ramDuration = 1.2f;
    public GameObject ramHitbox;
    [Range(0f, 1f)] public float slipChance = 0.3f; // 30% ngã sau khi húc

    [Header("Attack 3: Ném Đá (Throw Rock)")]
    public GameObject rockPrefab;
    public Transform throwPoint;
    public Vector2 throwForce = new Vector2(10f, 5f); // Lực ném (X, Y)

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        currentHealth = maxHealth;

        // Đảm bảo hitbox húc bị tắt lúc mới vào game
        if (ramHitbox != null) ramHitbox.SetActive(false);

        StartCoroutine(BossBehaviorLoop());
    }

    private float GetDynamicCooldown()
    {
        float healthPercent = currentHealth / maxHealth;
        return Mathf.Lerp(minActionCooldown, maxActionCooldown, healthPercent);
    }

    private IEnumerator BossBehaviorLoop()
    {
        while (true)
        {
            switch (currentState)
            {
                case BossState.Idle:
                    float currentCooldown = GetDynamicCooldown();
                    yield return new WaitForSeconds(currentCooldown);
                    ChooseNextAttack();
                    break;
                case BossState.SlamAttack:
                    yield return StartCoroutine(ExecuteSlamAttack());
                    break;
                case BossState.SpinRam:
                    yield return StartCoroutine(ExecuteSpinRam());
                    break;
                case BossState.ThrowRock:
                    yield return StartCoroutine(ExecuteThrowRock());
                    break;
                case BossState.Slipped:
                    yield return StartCoroutine(HandleSlippedState());
                    break;
            }
            yield return null;
        }
    }

    private void ChooseNextAttack()
    {
        int randomAttack = Random.Range(1, 4);
        if (randomAttack == 1) currentState = BossState.SlamAttack;
        else if (randomAttack == 2) currentState = BossState.SpinRam;
        else if (randomAttack == 3) currentState = BossState.ThrowRock;
    }

    // --- ATTACK 1: CHẠY VÀ ĐẬP ĐẤT (CÓ VỤ NỔ) ---
    private IEnumerator ExecuteSlamAttack()
    {
        Debug.Log("<color=red>BOSS:</color> Chạy tới đập đất!");

        while (Mathf.Abs(player.position.x - transform.position.x) > slamRange)
        {
            FlipTowardsPlayer();
            int direction = player.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * runSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.6f); // Wind-up

        Debug.Log("ĐẬP!");

        // 1. Quét tìm và PHÁ HỦY TOÀN BỘ TRE trong bán kính vụ nổ
        Collider2D[] destroyedBamboo = Physics2D.OverlapCircleAll(slamCenter.position, slamExplosionRadius, LayerMask.GetMask("Bamboo"));
        foreach (Collider2D bamboo in destroyedBamboo)
        {
            Destroy(bamboo.gameObject);
        }

        // 2. Quét tìm và GÂY SÁT THƯƠNG CHO PLAYER nếu đứng trong bán kính
        Collider2D hitPlayer = Physics2D.OverlapCircle(slamCenter.position, slamExplosionRadius, LayerMask.GetMask("Player"));
        if (hitPlayer != null)
        {
            PlayerMovement pMovement = hitPlayer.GetComponent<PlayerMovement>();
            if (pMovement != null)
            {
                pMovement.TakeDamage(1); // Gọi hàm chớp đỏ và trừ máu!
            }
        }

        yield return new WaitForSeconds(1f); // Hồi chiêu
        currentState = BossState.Idle;
    }

    // --- ATTACK 2: XOAY CHÙY VÀ HÚC TỚI ---
    private IEnumerator ExecuteSpinRam()
    {
        Debug.Log("<color=orange>BOSS:</color> Xoay chùy húc tới!");
        FlipTowardsPlayer();

        yield return new WaitForSeconds(0.8f); // Gồng chiêu

        if (ramHitbox != null) ramHitbox.SetActive(true);
        int direction = transform.localScale.x > 0 ? 1 : -1;

        rb.linearVelocity = new Vector2(direction * ramSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(ramDuration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (ramHitbox != null) ramHitbox.SetActive(false);

        if (Random.value <= slipChance)
        {
            Debug.Log("Quá đà! Boss mất thăng bằng ngã nhào!");
            currentState = BossState.Slipped;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            currentState = BossState.Idle;
        }
    }

    // --- ATTACK 3: NÉM ĐÁ ---
    private IEnumerator ExecuteThrowRock()
    {
        Debug.Log("<color=yellow>BOSS:</color> Ném đá!");
        FlipTowardsPlayer();

        yield return new WaitForSeconds(0.5f);

        if (rockPrefab != null && throwPoint != null)
        {
            GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
            Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();

            if (rockRb != null)
            {
                int direction = transform.localScale.x > 0 ? 1 : -1;
                rockRb.linearVelocity = new Vector2(throwForce.x * direction, throwForce.y);
            }
        }

        yield return new WaitForSeconds(1.5f);
        currentState = BossState.Idle;
    }

    // --- ĐIỂM YẾU: TRƯỢT NGÃ ---
    private IEnumerator HandleSlippedState()
    {
        Debug.Log("<color=green>BOSS ĐÃ NGÃ!</color> Tới chém nó đi!");
        yield return new WaitForSeconds(slipDuration);

        Debug.Log("Boss đứng dậy!");
        currentState = BossState.Idle;
    }

    // --- HỆ THỐNG NHẬN SÁT THƯƠNG TỪ PLAYER ---
    public void TakeDamage(Vector2 playerPos)
    {
        // Theo GDD của bạn, Boss chỉ ăn đòn khi đang bị ngã (Slipped)
        if (currentState == BossState.Slipped)
        {
            currentHealth -= 1;
            Debug.Log($"Boss bị chém! Máu còn: {currentHealth}");

            if (spriteRenderer != null) StartCoroutine(FlashRed());

            if (currentHealth <= 0)
            {
                Debug.Log("BOSS BỊ HẠ GỤC!");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Boss đang đứng vững, da trâu chém không thủng!");
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }

    private void FlipTowardsPlayer()
    {
        if (player.position.x > transform.position.x && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (player.position.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // Vẽ vòng tròn đỏ trong Editor để căn chỉnh bán kính vụ nổ dễ dàng
    private void OnDrawGizmosSelected()
    {
        if (slamCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slamCenter.position, slamExplosionRadius);
        }
    }
}