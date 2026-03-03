using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    public enum BossState { Passive, Idle, SlamAttack, SpinRam, ThrowRock, Slipped }

    // THÊM: Biến cài đặt vùng kích hoạt boss
    [Header("Cài đặt Kích Hoạt (Activation)")]
    public float detectionRadius = 10f; // Khoảng cách Anh Khoai cần đứng để boss thức dậy
    [SerializeField] private Color detectionGizmoColor = new Color(0f, 1f, 1f, 0.1f); // Màu xanh dương nhạt cho Gizmo
    private bool playerDetected = false; // Cờ đánh dấu boss đã thức dậy chưa

    [Header("Trạng thái hiện tại")]
    public BossState currentState = BossState.Idle;

    [Header("Chỉ số Sinh Tồn")]
    public float maxHealth = 15f;
    public float currentHealth;

    [Header("Hình ảnh Boss (Sprites)")]
    public Sprite idleSprite;
    public Sprite slamSprite;
    public Sprite ramSprite;
    public Sprite throwSprite;
    public Sprite slippedSprite;

    [Header("Cài đặt Attack 1 (Slam)")]
    public float runSpeed = 6f;
    public float slamRange = 2.5f;
    public float slamExplosionRadius = 3f;
    public Transform slamCenter;

    [Header("Cài đặt Attack 2 (Spin Ram)")]
    public float ramSpeed = 15f;
    public float ramDuration = 1.2f;
    [Range(0f, 1f)] public float slipChance = 0.3f;
    public Vector2 ramHitboxSize = new Vector2(2.5f, 2f);
    public Transform ramCenter;

    [Header("Cài đặt Attack 3 (Throw Rock)")]
    public GameObject rockPrefab;
    public Transform throwPoint;
    public float throwPower = 15f;
    public float minThrowAngle = 30f;
    public float maxThrowAngle = 70f;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr; // <--- Thêm SpriteRenderer
    private Color originalColor; // Thêm dòng này

    private bool isExploding = false;

    void Start()
    {
        currentHealth = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // ĐÃ XÓA: Dòng StartCoroutine(BossBehaviorLoop());
        // Boss bây giờ đứng Passive chờ đợi.
    }

    // THÊM: Hàm Update để kiểm tra khoảng cách đến player
    void Update()
    {
        // Nếu player chưa bị phát hiện, liên tục kiểm tra khoảng cách
        if (!playerDetected && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Nếu player đi vào vùng kích hoạt
            if (distanceToPlayer <= detectionRadius)
            {
                ActivateBoss();
            }
        }
    }

    // THÊM: Hàm tiện ích để thức dậy boss
    void ActivateBoss()
    {
        if (playerDetected) return; // Bảo mật: không thức dậy 2 lần

        playerDetected = true;
        Debug.Log("<color=purple>KÍCH HOẠT BOSS!</color> 'The Destroyer' đã thức giấc.");

        // Chuyển sang Idle và bắt đầu Behavior Loop
        currentState = BossState.Idle;
        StartCoroutine(BossBehaviorLoop());
    }

    // Hàm tiện ích để đổi hình ảnh nhanh gọn
    private void ChangeSprite(Sprite newSprite)
    {
        if (sr != null && newSprite != null)
        {
            sr.sprite = newSprite;
        }
    }

    private IEnumerator BossBehaviorLoop()
    {
        while (true)
        {
            switch (currentState)
            {
                case BossState.Idle:
                    ChangeSprite(idleSprite); // Đổi hình Đứng im
                    Debug.Log("<color=white>BOSS: Đang nghỉ ngơi...</color>");
                    yield return new WaitForSeconds(2f);
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
                    ChangeSprite(slippedSprite); // Đổi hình Ngã sấp mặt
                    Debug.Log("<color=green>BOSS: Trượt ngã!</color>");
                    yield return new WaitForSeconds(2f);
                    currentState = BossState.Idle;
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

    // ==========================================
    // ATTACK 1: ĐẬP ĐẤT (SLAM)
    // ==========================================
    private IEnumerator ExecuteSlamAttack()
    {
        ChangeSprite(slamSprite); // Đổi hình Giơ chùy đập đất
        Debug.Log("<color=red>BOSS:</color> Chạy tới đập đất!");

        while (Mathf.Abs(player.position.x - transform.position.x) > slamRange)
        {
            FlipTowardsPlayer();
            int direction = player.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * runSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.6f);

        Debug.Log("ĐẬP!");
        isExploding = true;

        if (slamCenter != null)
        {
            Collider2D[] destroyedBamboo = Physics2D.OverlapCircleAll(slamCenter.position, slamExplosionRadius, LayerMask.GetMask("Bamboo"));
            foreach (Collider2D bamboo in destroyedBamboo)
            {
                Destroy(bamboo.gameObject);
            }

            float distanceToPlayer = Vector2.Distance(slamCenter.position, player.position);

            if (distanceToPlayer <= slamExplosionRadius)
            {
                PlayerMovement pMovement = player.GetComponent<PlayerMovement>();
                if (pMovement != null)
                {
                    pMovement.TakeDamage(1);
                    if (pMovement.stats.healthPoint <= 0) pMovement.stats.currentBambooCount = 0;
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        isExploding = false;

        yield return new WaitForSeconds(0.8f);
        currentState = BossState.Idle;
    }

    // ==========================================
    // ATTACK 2: HÚC (SPIN RAM)
    // ==========================================
    private IEnumerator ExecuteSpinRam()
    {
        ChangeSprite(ramSprite); // Đổi hình Cắm đầu húc
        Debug.Log("<color=orange>BOSS:</color> Xoay chùy húc tới!");
        FlipTowardsPlayer();

        yield return new WaitForSeconds(0.8f);

        int direction = player.position.x > transform.position.x ? 1 : -1;

        float timer = 0f;
        bool hasHitPlayer = false;

        while (timer < ramDuration)
        {
            rb.linearVelocity = new Vector2(direction * ramSpeed, rb.linearVelocity.y);

            Vector2 boxCenter = ramCenter != null ? (Vector2)ramCenter.position : (Vector2)transform.position + new Vector2(0, 1f);

            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(boxCenter, ramHitboxSize, 0f);
            foreach (Collider2D obj in hitObjects)
            {
                if (((1 << obj.gameObject.layer) & LayerMask.GetMask("Bamboo")) != 0)
                {
                    Destroy(obj.gameObject);
                }
                else if (obj.CompareTag("Player") && !hasHitPlayer)
                {
                    PlayerMovement pMovement = obj.GetComponent<PlayerMovement>();
                    if (pMovement != null)
                    {
                        pMovement.TakeDamage(1);
                        hasHitPlayer = true;
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (Random.value <= slipChance)
        {
            Debug.Log("<color=green>QUÁ ĐÀ! Boss mất thăng bằng ngã nhào!</color>");
            currentState = BossState.Slipped;
        }
        else
        {
            yield return new WaitForSeconds(4f);
            currentState = BossState.Idle;
        }
    }

    // ==========================================
    // ATTACK 3: NÉM 3 CỤC ĐÁ GÓC NGẪU NHIÊN
    // ==========================================
    private IEnumerator ExecuteThrowRock()
    {
        ChangeSprite(throwSprite);
        Debug.Log("<color=yellow>BOSS:</color> Ném 3 cục đá liên tiếp!");
        FlipTowardsPlayer();

        yield return new WaitForSeconds(0.5f); // Gồng chiêu

        if (rockPrefab != null && throwPoint != null)
        {
            // Vòng lặp ném 3 cục đá
            for (int i = 0; i < 3; i++)
            {
                GameObject rock = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();

                if (rockRb != null)
                {
                    // Chọn một góc ném ngẫu nhiên cho MỖI cục đá
                    float randomAngle = Random.Range(minThrowAngle, maxThrowAngle);
                    float radianAngle = randomAngle * Mathf.Deg2Rad;

                    float xForce = Mathf.Cos(radianAngle) * throwPower;
                    float yForce = Mathf.Sin(radianAngle) * throwPower;

                    int direction = player.position.x > transform.position.x ? 1 : -1;

                    rockRb.linearVelocity = new Vector2(xForce * direction, yForce);
                }

                // Chờ 1 giây trước khi ném cục tiếp theo
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            Debug.LogWarning("Chưa gắn Rock Prefab hoặc Throw Point vào BossAI!");
        }

        yield return new WaitForSeconds(1.0f); // Đứng yên hồi chiêu sau khi ném xong cả 3 cục
        currentState = BossState.Idle;
    }

    // ==========================================
    // HỆ THỐNG NHẬN SÁT THƯƠNG TỪ PLAYER
    // ==========================================
    public void TakeDamage(Vector2 playerPos)
    {
        if (currentState == BossState.Slipped)
        {
            currentHealth -= 1;
            Debug.Log($"<color=cyan>Boss bị chém!</color> Máu còn: {currentHealth}");

            // GỌI HIỆU ỨNG CHỚP ĐỎ TẠI ĐÂY!
            StartCoroutine(FlashRedEffect());

            if (currentHealth <= 0)
            {
                Debug.Log("<color=green>BOSS BỊ HẠ GỤC!</color>");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Boss đang đứng vững, da trâu chém không thủng!");
        }
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
    private IEnumerator FlashRedEffect()
    {
        if (sr != null)
        {
            sr.color = Color.yellow;
            yield return new WaitForSeconds(0.5f); // Nháy đỏ trong 0.15s
            sr.color = originalColor;
        }
    }

    private void OnDrawGizmos()
    {
        if (slamCenter == null) return;
        if (isExploding)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(slamCenter.position, slamExplosionRadius);
        }
        // THÊM: Vẽ vòng tròn kích hoạt màu xanh dương
        Gizmos.color = detectionGizmoColor;
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (slamCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(slamCenter.position, slamExplosionRadius);
        }

        Gizmos.color = Color.yellow;
        Vector2 boxPos = ramCenter != null ? (Vector2)ramCenter.position : (Vector2)transform.position + new Vector2(0, 1f);
        Gizmos.DrawWireCube(boxPos, ramHitboxSize);
    }
}