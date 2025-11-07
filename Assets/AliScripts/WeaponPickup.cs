using UnityEngine;

// Silahýn kategorisini belirler (Hangi slota gidecek?)
// Primary = 1 Tuþu (Rifle, Shotgun vb.)
// Secondary = 2 Tuþu (Pistol vb.)
public enum WeaponType
{
    Primary,
    Secondary
}

// Bu script, yerde duran "alýnabilir" silah objelerinin üzerine konulacak.
public class WeaponPickup : MonoBehaviour
{
    [Header("1. Silahýn Türü (Hangi Slota Gidecek?)")]
    public WeaponType type;

    [Header("2. Silahýn Prefab'larý (Kalýplarý)")]

    [Tooltip("Bu silah alýndýðýnda OYUNCUNUN ELÝNDE görünecek olan Prefab")]
    public GameObject equippedPrefab;

    [Tooltip("Bu silah G tuþuyla ATILDIÐINDA YERDE görünecek olan Prefab (Genellikle bu objenin kendisi)")]
    public GameObject propPrefab;


    // --- Kurulum Kontrolleri (Bu kýsým otomatik çalýþýr) ---
    private void Start()
    {
        // Bu objenin bir "Tetikleyici" (Trigger) Collider'ý olmalý ki oyuncu onu algýlasýn.
        Collider col = GetComponent<Collider>();
        if (col == null || !col.isTrigger)
        {
            Debug.LogError("HATA: " + gameObject.name + " objesinde 'Is Trigger = true' ayarlý bir Collider yok!");
        }

        // Bu objenin "WeaponProp" adýnda bir "Tag"i (Etiketi) olmalý ki oyuncu onu tanýsýn.
        if (!gameObject.CompareTag("WeaponProp"))
        {
            Debug.LogWarning("UYARI: " + gameObject.name + " objesinin Tag'ini 'WeaponProp' olarak ayarlayýn.");
        }
    }
}