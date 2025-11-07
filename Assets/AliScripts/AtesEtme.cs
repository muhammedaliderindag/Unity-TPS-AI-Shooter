using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class AtesEtme : MonoBehaviour
{
    [Header("Genel Ayarlar")]
    public Camera mainCamera;
    private TextMeshProUGUI mermiYazisi;

    [Header("Saldırı Gücü")]
    public float damage = 20f;

    [Header("Efekt Ayarları (NPC Tarzı)")]
    public Transform weaponMuzzle;
    public ParticleSystem muzzleFlash;
    public GameObject bulletImpactPrefab;
    public GameObject bulletTracerPrefab;
    public float muzzleVelocity = 100f;

    [Header("Ses")]
    public AudioSource atesSesi;

    [Header("Silah Ayarları")]
    public float menzil = 100f;
    public int anlikMermi = 30; // Varsayılan olarak mermi verelim
    public int sarjorKapasitesi = 30;
    public int toplamMermi = 90;
    public float reloadSuresi = 2f;
    private bool mermiDolduruyor = false;

    [Header("Geri Tepme")]
    public float verticalRecoil = 1.0f;
    public float horizontalRecoil = 0.5f;
    public Transform playerBody;

    [Header("IK ve Pozisyon")]
    public Transform leftHandIKTarget;
    public Transform weaponModel;
    public Transform idlePose;
    public Transform aimPose;
    public float poseTransitionSpeed = 20f;

    [Header("Ateş Etme Türü")]
    public bool isAutomatic = true;
    public float fireRate = 10f;
    private float nextTimeToFire = 0f;

    [Header("Diğer")]
    public GameObject propPrefab;

    private Animator characterAnimator;
    private NisanAlmaSistemi nisanAlmaSistemi;
    private int layerMask;

    void Start()
    {
        Debug.Log("<color=cyan>[SİLAH_DEBUG] BAŞLATILIYOR...</color>");

        if (mainCamera == null) mainCamera = Camera.main;
        if (playerBody == null) playerBody = transform.root;
        if (characterAnimator == null) characterAnimator = transform.root.GetComponent<Animator>();

        // Muzzle Kontrolü
        if (weaponMuzzle == null)
        {
            Debug.LogError("<color=red>[SİLAH_DEBUG] HATA: 'Weapon Muzzle' atanmamış! Mermiler rastgele bir yerden çıkacak!</color>");
        }
        else
        {
            Debug.Log($"[SİLAH_DEBUG] Muzzle noktası bulundu: {weaponMuzzle.name}. Başlangıç konumu: {weaponMuzzle.position}");
        }

        // Muzzle Flash Kontrolü
        if (muzzleFlash == null)
        {
            Debug.LogWarning("<color=yellow>[SİLAH_DEBUG] UYARI: 'Muzzle Flash' (Ateş Efekti) atanmamış!</color>");
        }

        if (weaponModel != null && idlePose != null)
        {
            weaponModel.localPosition = idlePose.localPosition;
            weaponModel.localRotation = idlePose.localRotation;
        }

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1) layerMask = ~(1 << playerLayer);

        nextTimeToFire = Time.time;
        mermiDolduruyor = false;
        UpdateUI();
        Debug.Log($"<color=green>[SİLAH_DEBUG] HAZIR. Mermi: {anlikMermi}/{toplamMermi}</color>");
    }

    // Diğer scriptler için getter/setter'lar
    public int GetCurrentAmmo() { return anlikMermi; }
    public int GetReserveAmmo() { return toplamMermi; }
    public void SetUiReference(TextMeshProUGUI uiText) { mermiYazisi = uiText; UpdateUI(); }
    public void SetAimingSystemReference(NisanAlmaSistemi system) { nisanAlmaSistemi = system; }
    public void InitializeAmmo(int currentAmmo, int reserveAmmo) { anlikMermi = currentAmmo; toplamMermi = reserveAmmo; UpdateUI(); }

    void Update()
    {
        // DEBUG: Namlunun baktığı yönü sahnede kırmızı çizgiyle göster
        if (weaponMuzzle != null)
        {
            Debug.DrawRay(weaponMuzzle.position, weaponMuzzle.forward * 5f, Color.red);
        }

        if (nisanAlmaSistemi != null && weaponModel != null && idlePose != null && aimPose != null)
        {
            Transform targetPose = nisanAlmaSistemi.IsAiming ? aimPose : idlePose;
            weaponModel.transform.localPosition = Vector3.Lerp(weaponModel.transform.localPosition, targetPose.localPosition, Time.deltaTime * poseTransitionSpeed);
            weaponModel.transform.localRotation = Quaternion.Slerp(weaponModel.transform.localRotation, targetPose.localRotation, Time.deltaTime * poseTransitionSpeed);
        }

        if (mermiDolduruyor || mainCamera == null || nisanAlmaSistemi == null) return;

        bool isAimingForShooting = nisanAlmaSistemi.IsAiming;

        // Tetik Kontrolü
        if (isAutomatic)
        {
            if (Mouse.current.leftButton.isPressed && isAimingForShooting && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                FireWeaponDebug(); // Debug versiyonunu çağırıyoruz
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && isAimingForShooting)
            {
                FireWeaponDebug();
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (anlikMermi < sarjorKapasitesi && toplamMermi > 0) StartCoroutine(ReloadCoroutine());
        }
    }

    private void FireWeaponDebug()
    {
        // 1. Mermi Kontrolü
        if (anlikMermi <= 0)
        {
            Debug.Log("<color=red>[SİLAH_DEBUG] Ateş başarısız: MERMİ YOK!</color>");
            if (!mermiDolduruyor && toplamMermi > 0) StartCoroutine(ReloadCoroutine());
            return;
        }

        anlikMermi--;
        Debug.Log($"[SİLAH_DEBUG] Tetik çekildi! Kalan mermi: {anlikMermi}");

        // 2. Muzzle Flash Oynatma Denemesi
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
            muzzleFlash.Play();
            // Debug.Log("[SİLAH_DEBUG] Muzzle Flash oynatıldı.");
        }
        else
        {
            Debug.LogError("[SİLAH_DEBUG] Muzzle Flash OYNATILAMADI çünkü atanmamış!");
        }

        if (atesSesi != null) atesSesi.Play();
        if (characterAnimator != null) characterAnimator.SetTrigger("Shoot");

        ApplyRecoil();

        // 3. Raycast ve Hedefleme
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, menzil, layerMask))
        {
            targetPoint = hit.point;
            Debug.Log($"[SİLAH_DEBUG] Vuruş! Hedef: {hit.transform.name} | Mesafe: {hit.distance:F1}m");

            if (bulletImpactPrefab != null)
            {
                GameObject impact = Instantiate(bulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                impact.transform.SetParent(hit.transform);
                Destroy(impact, 5f);
            }

            AI_Controller npc = hit.transform.GetComponentInParent<AI_Controller>();
            if (npc == null) npc = hit.transform.GetComponent<AI_Controller>();
            if (npc != null)
            {
                npc.TakeDamage(damage);
                Debug.Log($"<color=green>[SİLAH_DEBUG] NPC'ye {damage} hasar verildi.</color>");
            }
        }
        else
        {
            targetPoint = ray.GetPoint(menzil);
            // Debug.Log("[SİLAH_DEBUG] Iska! (Menzil dışı veya boşluk)");
        }

        // 4. Tracer (Mermi Görseli) Oluşturma
        if (bulletTracerPrefab != null && weaponMuzzle != null)
        {
            Debug.Log($"[SİLAH_DEBUG] Tracer oluşturuluyor. Çıkış noktası: {weaponMuzzle.position}");
            StartCoroutine(SpawnTracer(targetPoint));
        }
        else
        {
            if (weaponMuzzle == null) Debug.LogError("<color=red>[SİLAH_DEBUG] Tracer oluşturulamadı: Weapon Muzzle YOK!</color>");
            if (bulletTracerPrefab == null) Debug.LogError("<color=red>[SİLAH_DEBUG] Tracer oluşturulamadı: Tracer Prefab YOK!</color>");
        }

        UpdateUI();
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

    void ApplyRecoil()
    {
        mainCamera.transform.localEulerAngles += new Vector3(-verticalRecoil, 0f, 0f);
        if (playerBody != null) playerBody.Rotate(0f, Random.Range(-horizontalRecoil, horizontalRecoil), 0f);
    }

    IEnumerator ReloadCoroutine()
    {
        mermiDolduruyor = true;
        Debug.Log("[SİLAH_DEBUG] Şarjör değiştiriliyor...");
        UpdateUI();
        yield return new WaitForSeconds(reloadSuresi);
        int eklenecek = sarjorKapasitesi - anlikMermi;
        if (toplamMermi < eklenecek) eklenecek = toplamMermi;
        anlikMermi += eklenecek;
        toplamMermi -= eklenecek;
        mermiDolduruyor = false;
        Debug.Log($"[SİLAH_DEBUG] Şarjör hazır. Yeni durum: {anlikMermi}/{toplamMermi}");
        UpdateUI();
    }

    void UpdateUI()
    {
        if (mermiYazisi != null) mermiYazisi.text = mermiDolduruyor ? "Dolduruluyor..." : anlikMermi + " / " + toplamMermi;
    }
}