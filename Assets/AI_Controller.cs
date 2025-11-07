using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using System.Collections;

public class AI_Controller : MonoBehaviour
{
    [Header("Saðlýk Ayarlarý")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isDead = false;

    // --- BÝLEÞENLER ---
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private RigBuilder rigBuilder;

    [Header("Hedef Ayarlarý")]
    public Transform playerTransform;
    public Transform aimTarget;

    public enum AIState { Patrol, Chase, Attack, Dead }
    [Header("Anlýk Durum")]
    public AIState currentState;

    [Header("Combat (Savaþ) Ayarlarý")]
    public float npcDamage = 10f;
    public float chaseSpeed = 3.5f;
    public float firingRange = 15f;
    public float sightRange = 20f;
    public float attackCooldown = 1.0f;
    private float attackTimer = 0f;
    // YENÝ: Görüþ kontrolü için hangi katmanlarýn engel sayýlacaðý
    public LayerMask obstacleMask;

    [Header("Efekt Ayarlarý")]
    public Transform weaponMuzzle;
    public ParticleSystem muzzleFlash;
    public GameObject bulletImpactPrefab;
    public GameObject bulletTracerPrefab;
    public float muzzleVelocity = 100f;

    [Header("Rigging (IK) Ayarlarý")]
    public Rig leftHandIK;
    public Rig aimIK;
    public float rigWeightSpeed = 3.0f;

    [Header("Patrol Ayarlarý")]
    public float patrolSpeed = 1.5f;
    public float waitTime = 3f;
    private float waitCounter = 0f;
    private Vector3 spawnPosition;

    [Header("Ses Ayarlarý")]
    public AudioClip[] footstepSounds;

    private readonly string ANIM_STATE_PARAM = "AnimState";

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rigBuilder = GetComponent<RigBuilder>();
        spawnPosition = transform.position;

        if (weaponMuzzle == null)
        {
            GameObject tempMuzzle = new GameObject("TempMuzzle");
            tempMuzzle.transform.SetParent(this.transform);
            tempMuzzle.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
            weaponMuzzle = tempMuzzle.transform;
        }

        // Eðer obstacleMask ayarlanmamýþsa, varsayýlan olarak her þeyi engel kabul et (Player hariç)
        if (obstacleMask == 0)
        {
            obstacleMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Ignore Raycast"));
        }

        if (playerTransform == null || aimTarget == null || animator == null || agent == null)
        {
            enabled = false;
            return;
        }

        currentState = AIState.Patrol;
        agent.speed = patrolSpeed;
        PickNewDestination();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0f) Die();
        else if (currentState == AIState.Patrol) currentState = AIState.Chase;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        currentState = AIState.Dead;

        // --- YÖNETÝCÝYE HABER VER ---
        if (GameManager.instance != null)
        {
            GameManager.instance.DusmanOldu();
        }
        // -----------------------------

        if (agent != null) { agent.isStopped = true; agent.enabled = false; }
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (rigBuilder != null) rigBuilder.enabled = false;
        if (leftHandIK != null) leftHandIK.weight = 0f;
        if (aimIK != null) aimIK.weight = 0f;
        if (animator != null) animator.SetInteger(ANIM_STATE_PARAM, 4);
        Destroy(gameObject, 3.0f);
    }

    void Update()
    {
        if (isDead) return;
        Vector3 playerPos2D = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        float dist = Vector3.Distance(transform.position, playerPos2D);
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case AIState.Patrol: PatrolLogic(dist); break;
            case AIState.Chase: ChaseLogic(dist); break;
            case AIState.Attack: AttackLogic(dist); break;
        }
        UpdateAnimationState();
        UpdateRigWeights();
    }

    // --- YENÝ: Görüþ Hattý Kontrolü ---
    private bool CanSeePlayer()
    {
        Vector3 eyePos = transform.position + Vector3.up * 1.6f; // NPC'nin göz hizasý
        Vector3 targetPos = aimTarget.position; // Oyuncunun hedef noktasý (göðsü/baþý)
        Vector3 dirToPlayer = (targetPos - eyePos).normalized;
        float distToPlayer = Vector3.Distance(eyePos, targetPos);

        // Oyuncuya doðru bir ýþýn yolla. Eðer menzil içinde bir engele çarpmazsa, görüyoruz demektir.
        if (!Physics.Raycast(eyePos, dirToPlayer, distToPlayer, obstacleMask))
        {
            return true; // Engel yok, görüyorum
        }
        return false; // Engel var
    }
    // ------------------------------------

    private void AttackLogic(float dist)
    {
        // 1. Eðer oyuncu menzilden çýktýysa KOVALA
        if (dist > firingRange * 1.1f)
        {
            currentState = AIState.Chase;
            agent.isStopped = false;
            agent.updateRotation = true;
            return;
        }

        // 2. Eðer menzildeyiz ama oyuncuyu GÖREMÝYORSAK (arada duvar varsa), KOVALAMAYA DEVAM ET
        if (!CanSeePlayer())
        {
            // Saldýrý modundan çýkmadan, oyuncunun son bilinen konumuna doðru ilerle
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(playerTransform.position);
            return;
        }

        // 3. Menzildeyiz VE görüyoruz -> SALDIR
        agent.isStopped = true; // Dur ve ateþ et
        agent.updateRotation = false; // NavMesh'in döndürmesini kapat, biz elle döndüreceðiz

        // Oyuncuya dön
        Vector3 dir = (aimTarget.position - transform.position).normalized;
        dir.y = 0; // Sadece yatayda dön
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        if (attackTimer <= 0f)
        {
            FireWeapon();
            attackTimer = attackCooldown;
        }
    }

    private void FireWeapon()
    {
        if (muzzleFlash != null) muzzleFlash.Play();

        // Hafif rastgelelik ekleyelim ki %100 isabetli robot gibi olmasýn
        Vector3 randomSpread = Random.insideUnitSphere * 0.2f;
        Vector3 shootDirection = (aimTarget.position - weaponMuzzle.position + randomSpread).normalized;

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(weaponMuzzle.position, shootDirection, out hit, firingRange))
        {
            targetPoint = hit.point;
            if (bulletImpactPrefab != null)
            {
                GameObject impact = Instantiate(bulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                impact.transform.SetParent(hit.transform);
                Destroy(impact, 5f);
            }

            PlayerHealth playerHealth = hit.transform.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = hit.transform.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(npcDamage);
        }
        else
        {
            targetPoint = weaponMuzzle.position + shootDirection * 100f;
        }

        if (bulletTracerPrefab != null)
        {
            StartCoroutine(SpawnTracer(targetPoint));
        }
    }

    IEnumerator SpawnTracer(Vector3 targetPoint)
    {
        GameObject tracer = Instantiate(bulletTracerPrefab, weaponMuzzle.position, Quaternion.identity);
        tracer.transform.LookAt(targetPoint);
        float maxLifetime = 3.0f;
        float lifetime = 0f;
        while (Vector3.Distance(tracer.transform.position, targetPoint) > 1.0f && lifetime < maxLifetime)
        {
            tracer.transform.position = Vector3.MoveTowards(tracer.transform.position, targetPoint, muzzleVelocity * Time.deltaTime);
            lifetime += Time.deltaTime;
            yield return null;
        }
        Destroy(tracer);
    }

    private void UpdateAnimationState() { if (animator == null || isDead) return; int targetState = 0; if (currentState == AIState.Attack && agent.isStopped) targetState = 3; else if (agent.velocity.magnitude > 0.1f) targetState = (currentState == AIState.Chase || currentState == AIState.Attack) ? 2 : 1; if (animator.GetInteger(ANIM_STATE_PARAM) != targetState) animator.SetInteger(ANIM_STATE_PARAM, targetState); }
    private void UpdateRigWeights() { if (isDead) return; float targetWeight = (currentState == AIState.Chase || currentState == AIState.Attack) ? 1.0f : 0.0f; float aimWeight = (currentState == AIState.Attack && CanSeePlayer()) ? 1.0f : 0.0f; leftHandIK.weight = Mathf.Lerp(leftHandIK.weight, targetWeight, Time.deltaTime * rigWeightSpeed); aimIK.weight = Mathf.Lerp(aimIK.weight, aimWeight, Time.deltaTime * rigWeightSpeed); }
    private void PatrolLogic(float dist) { if (dist <= sightRange && CanSeePlayer()) { currentState = AIState.Chase; agent.speed = chaseSpeed; return; } if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) { waitCounter += Time.deltaTime; if (waitCounter >= waitTime) { PickNewDestination(); waitCounter = 0f; } } }
    private void ChaseLogic(float dist) { agent.SetDestination(playerTransform.position); if (dist <= firingRange && CanSeePlayer()) { currentState = AIState.Attack; return; } if (dist > sightRange * 1.5f) { currentState = AIState.Patrol; agent.speed = patrolSpeed; PickNewDestination(); return; } agent.isStopped = false; }
    private void PickNewDestination() { agent.isStopped = false; Vector3 randDir = Random.insideUnitSphere * 50f + spawnPosition; NavMeshHit hit; if (NavMesh.SamplePosition(randDir, out hit, 50f, NavMesh.AllAreas)) agent.SetDestination(hit.position); }
    public void FootStep() { if (!isDead && audioSource != null && footstepSounds.Length > 0) audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]); }
}