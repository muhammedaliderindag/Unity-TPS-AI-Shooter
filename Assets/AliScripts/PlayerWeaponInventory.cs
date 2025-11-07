using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using System.Collections;
using TMPro;

public class PlayerWeaponInventory : MonoBehaviour
{
    // --- 1. AYARLANACAK ALANLAR ---
    [Header("A. Tutma Noktalarý (Hold Points)")]
    [SerializeField] private Transform primaryHoldPoint;
    [SerializeField] private Transform secondaryHoldPoint;

    [Header("B. Input Eylemleri (New Input System)")]
    [SerializeField] private InputActionReference pickupAction;
    [SerializeField] private InputActionReference dropAction;
    [SerializeField] private InputActionReference primaryAction;
    [SerializeField] private InputActionReference secondaryAction;
    [SerializeField] private InputActionReference unarmedAction;

    [Header("C. IK (Animasyon) Ayarlarý")]
    [SerializeField] private Rig rigLayer;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;

    [Header("D. UI Referanslarý")]
    [SerializeField] private TextMeshProUGUI ammoTextUI;

    [Header("E. Niþan Alma Ayarlarý")]
    [Tooltip("Niþan alýrken karakterin kameraya dönme hýzý")]
    [SerializeField] private float aimRotationSpeed = 20f;
    private Transform mainCameraTransform;

    // --- 2. SÝSTEM DEÐÝÞKENLERÝ ---
    private Animator animator;
    private NisanAlmaSistemi nisanAlmaSistemi;

    // --- 3. SÝSTEMÝN HAFIZASI ---
    private GameObject primaryEquippedPrefab = null;
    private GameObject secondaryEquippedPrefab = null;

    [Header("Mermi Hafýzasý (Debug için)")]
    [SerializeField] private int primaryCurrentAmmo = -1;
    [SerializeField] private int primaryReserveAmmo = -1;
    [SerializeField] private int secondaryCurrentAmmo = -1;
    [SerializeField] private int secondaryReserveAmmo = -1;

    // --- 4. MEVCUT DURUM ---
    private GameObject currentEquippedWeapon = null;
    private WeaponType currentEquippedType = WeaponType.Primary;
    private bool isUnarmed = true;
    private WeaponPickup nearbyWeaponProp = null;
    private Transform currentIK_Target = null; // TEK IK HEDEFÝ

    // --- 5. ANA FONKSÝYONLAR ---
    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogError("Animator BULUNAMADI!");

        nisanAlmaSistemi = GetComponent<NisanAlmaSistemi>();
        if (nisanAlmaSistemi == null) Debug.LogError("NisanAlmaSistemi BULUNAMADI! (Player objesine eklemeyi unutma)");

        if (ammoTextUI == null)
            Debug.LogError("Mermi Text'i (ammoTextUI) Inspector'dan atanmamýþ!");

        mainCameraTransform = Camera.main.transform;

        isUnarmed = true;
        if (animator != null)
            animator.SetBool("IsHoldingRifle", false);
        UpdateIK(null);
    }

    private void Update()
    {
        if (animator == null || nisanAlmaSistemi == null || mainCameraTransform == null) return;

        bool isTryingToAim = nisanAlmaSistemi.IsAiming;
        bool isHoldingRifle = (!isUnarmed && currentEquippedType == WeaponType.Primary);

        // 1. Animator'ü güncelle (Animasyon geçiþi için)
        animator.SetBool("IsAiming", isTryingToAim && isHoldingRifle);

        // 2. Karakteri Kameraya Döndür (Niþan alýrken)
        if (isTryingToAim && isHoldingRifle)
        {
            Vector3 targetDirection = mainCameraTransform.forward;
            targetDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * aimRotationSpeed);
        }
    }

    // --- Input Sistemini Aktif Etme/Kapatma ---
    private void OnEnable()
    {
        pickupAction.action.performed += OnPickupPressed;
        dropAction.action.performed += OnDropPressed;
        primaryAction.action.performed += OnPrimaryPressed;
        secondaryAction.action.performed += OnSecondaryPressed;
        unarmedAction.action.performed += OnUnarmedPressed;
        pickupAction.action.Enable();
        dropAction.action.Enable();
        primaryAction.action.Enable();
        secondaryAction.action.Enable();
        unarmedAction.action.Enable();
    }

    private void OnDisable()
    {
        pickupAction.action.performed -= OnPickupPressed;
        dropAction.action.performed -= OnDropPressed;
        primaryAction.action.performed -= OnPrimaryPressed;
        secondaryAction.action.performed -= OnSecondaryPressed;
        unarmedAction.action.performed -= OnUnarmedPressed;
        pickupAction.action.Disable();
        dropAction.action.Disable();
        primaryAction.action.Disable();
        secondaryAction.action.Disable();
        unarmedAction.action.Disable();
    }

    // --- 6. INPUT EYLEM FONKSÝYONLARI ---
    private void OnPickupPressed(InputAction.CallbackContext context) { if (nearbyWeaponProp != null) PickupNearbyWeapon(); }
    private void OnDropPressed(InputAction.CallbackContext context) { DropCurrentWeapon(); }
    private void OnPrimaryPressed(InputAction.CallbackContext context) { EquipWeapon(WeaponType.Primary); }
    private void OnSecondaryPressed(InputAction.CallbackContext context) { EquipWeapon(WeaponType.Secondary); }
    private void OnUnarmedPressed(InputAction.CallbackContext context) { EquipUnarmed(); }

    // --- 7. ANA SÝLAH MANTIÐI ---
    private void PickupNearbyWeapon()
    {
        if (nearbyWeaponProp == null) return;
        WeaponType propType = nearbyWeaponProp.type;
        GameObject propEquipped = nearbyWeaponProp.equippedPrefab;
        // propProp bilgisi artýk buradan alýnmýyor
        Vector3 propPosition = nearbyWeaponProp.transform.position;
        Quaternion propRotation = nearbyWeaponProp.transform.rotation;
        Destroy(nearbyWeaponProp.gameObject);
        nearbyWeaponProp = null;

        if (propType == WeaponType.Primary)
        {
            // Elimizde zaten birincil silah varsa, onu atmamýz lazým
            if (currentEquippedType == WeaponType.Primary && !isUnarmed)
            {
                // Silahý yere at (yeni silahý almadan önce)
                DropCurrentWeapon();
            }

            primaryCurrentAmmo = -1;
            primaryReserveAmmo = -1;
            primaryEquippedPrefab = propEquipped;
            EquipWeapon(WeaponType.Primary);
        }
        else if (propType == WeaponType.Secondary)
        {
            // Elimizde zaten ikincil silah varsa, onu atmamýz lazým
            if (currentEquippedType == WeaponType.Secondary && !isUnarmed)
            {
                // Silahý yere at (yeni silahý almadan önce)
                DropCurrentWeapon();
            }

            secondaryCurrentAmmo = -1;
            secondaryReserveAmmo = -1;
            secondaryEquippedPrefab = propEquipped;
            EquipWeapon(WeaponType.Secondary);
        }
    }

    private void DropCurrentWeapon()
    {
        if (isUnarmed || currentEquippedWeapon == null) return;

        // 1. Eldeki silahýn script'ini al
        AtesEtme gunScript = currentEquippedWeapon.GetComponent<AtesEtme>();

        // 2. Script'ten 'propPrefab'ý iste
        if (gunScript == null || gunScript.propPrefab == null)
        {
            Debug.LogError("ATMA HATASI: Bu silahta (" + currentEquippedWeapon.name + ") 'Prop Prefab' atanmamýþ!");
            return;
        }

        GameObject propToDrop = gunScript.propPrefab;

        // 3. Hafýzayý temizle (Hangi slottan attýysak)
        if (currentEquippedType == WeaponType.Primary)
        {
            primaryEquippedPrefab = null;
            primaryCurrentAmmo = -1;
            primaryReserveAmmo = -1;
        }
        else if (currentEquippedType == WeaponType.Secondary)
        {
            secondaryEquippedPrefab = null;
            secondaryCurrentAmmo = -1;
            secondaryReserveAmmo = -1;
        }

        // 4. Prop'u Yere Spawn Et ve Fýrlat
        Vector3 dropPosition = transform.position + transform.forward * 1f;
        GameObject spawnedProp = Instantiate(propToDrop, dropPosition, transform.rotation);
        Rigidbody rb = spawnedProp.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwForce = (transform.forward * 4f) + (transform.up * 3f);
            rb.AddForce(throwForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10f);
        }

        // 5. Elimizdeki silahý yok et ve silahsýz moda geç
        Destroy(currentEquippedWeapon);
        currentEquippedWeapon = null;
        isUnarmed = true;

        if (animator != null)
            animator.SetBool("IsHoldingRifle", false);
        if (ammoTextUI != null) ammoTextUI.text = "";

        currentIK_Target = null;
        UpdateIK(null);
    }

    private void EquipWeapon(WeaponType typeToEquip)
    {
        // 1. Deðiþkenleri tanýmla
        GameObject prefabToEquip = null;
        Transform holdPoint = null;
        int savedCurrentAmmo = -1;
        int savedReserveAmmo = -1;

        if (typeToEquip == WeaponType.Primary)
        {
            prefabToEquip = primaryEquippedPrefab;
            holdPoint = primaryHoldPoint;
            savedCurrentAmmo = primaryCurrentAmmo;
            savedReserveAmmo = primaryReserveAmmo;
        }
        else if (typeToEquip == WeaponType.Secondary)
        {
            prefabToEquip = secondaryEquippedPrefab;
            holdPoint = secondaryHoldPoint;
            savedCurrentAmmo = secondaryCurrentAmmo;
            savedReserveAmmo = secondaryReserveAmmo;
        }
        if (prefabToEquip == null)
        {
            Debug.Log("Bu slot (" + typeToEquip + ") boþ.");
            return;
        }
        if (!isUnarmed && currentEquippedType == typeToEquip)
        {
            Debug.Log("Zaten o silahý tutuyorsun.");
            return;
        }
        // 4. Mermiyi kaydet
        if (currentEquippedWeapon != null)
        {
            AtesEtme currentGunScript = currentEquippedWeapon.GetComponent<AtesEtme>();
            if (currentGunScript != null)
            {
                if (currentEquippedType == WeaponType.Primary)
                {
                    primaryCurrentAmmo = currentGunScript.GetCurrentAmmo();
                    primaryReserveAmmo = currentGunScript.GetReserveAmmo();
                }
                else
                {
                    secondaryCurrentAmmo = currentGunScript.GetCurrentAmmo();
                    secondaryReserveAmmo = currentGunScript.GetReserveAmmo();
                }
            }
            Destroy(currentEquippedWeapon);
            currentEquippedWeapon = null;
        }

        // 5. Yeni silahý spawn et
        currentEquippedWeapon = Instantiate(prefabToEquip, holdPoint);
        currentEquippedWeapon.transform.localPosition = Vector3.zero;
        currentEquippedWeapon.transform.localRotation = Quaternion.identity;

        isUnarmed = false;
        currentEquippedType = typeToEquip;

        // 6. Yeni silahý ayarla (Referanslar)
        AtesEtme newGunScript = currentEquippedWeapon.GetComponent<AtesEtme>();
        if (newGunScript != null)
        {
            newGunScript.SetAimingSystemReference(nisanAlmaSistemi);
            newGunScript.SetUiReference(ammoTextUI);

            if (savedCurrentAmmo == -1)
                newGunScript.InitializeAmmo(newGunScript.sarjorKapasitesi, newGunScript.toplamMermi);
            else
                newGunScript.InitializeAmmo(savedCurrentAmmo, savedReserveAmmo);

            currentIK_Target = newGunScript.leftHandIKTarget;
        }

        // 7. Animatörü güncelle
        if (animator != null)
        {
            if (typeToEquip == WeaponType.Primary)
                animator.SetBool("IsHoldingRifle", true);
            else
                animator.SetBool("IsHoldingRifle", false);
        }

        // 8. IK Sistemini Güncelle
        UpdateIK(currentIK_Target);
    }

    public void EquipUnarmed()
    {
        if (isUnarmed) return;
        if (currentEquippedWeapon != null)
        {
            AtesEtme currentGunScript = currentEquippedWeapon.GetComponent<AtesEtme>();
            if (currentGunScript != null)
            {
                if (currentEquippedType == WeaponType.Primary)
                {
                    primaryCurrentAmmo = currentGunScript.GetCurrentAmmo();
                    primaryReserveAmmo = currentGunScript.GetReserveAmmo();
                }
                else
                {
                    secondaryCurrentAmmo = currentGunScript.GetCurrentAmmo();
                    secondaryReserveAmmo = currentGunScript.GetReserveAmmo();
                }
            }
            Destroy(currentEquippedWeapon);
            currentEquippedWeapon = null;
        }
        isUnarmed = true;
        Debug.Log("Silahsýz (Unarmed) moda geçildi.");
        if (animator != null)
            animator.SetBool("IsHoldingRifle", false);
        if (ammoTextUI != null) ammoTextUI.text = "";
        currentIK_Target = null;
        UpdateIK(null);
    }

    // --- 8. YARDIMCI FONKSÝYONLAR ---
    private void UpdateIK(Transform target)
    {
        if (leftHandIK == null || rigLayer == null)
        {
            if (target != null)
                Debug.LogWarning("IK Target bulundu ama IK bileþenleri atanmamýþ.");
            return;
        }
        if (target != null)
        {
            leftHandIK.data.target = target;
            rigLayer.weight = 1f;
        }
        else
        {
            leftHandIK.data.target = null;
            rigLayer.weight = 0f;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponProp"))
        {
            WeaponPickup weapon = other.GetComponent<WeaponPickup>();
            if (weapon != null)
                nearbyWeaponProp = weapon;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WeaponProp") && other.GetComponent<WeaponPickup>() == nearbyWeaponProp)
        {
            nearbyWeaponProp = null;
        }
    }
}