using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Ayarlarý")]
    public TextMeshProUGUI dusmanSayacText; // Sol üstteki küçük sayaç
    public GameObject oyunBittiPaneli;      // Oyun bitince açýlacak büyük yazý/panel

    private int toplamDusman;
    private int kalanDusman;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Baþlangýçta oyun bitti yazýsýný gizle (eðer açýksa)
        if (oyunBittiPaneli != null)
        {
            oyunBittiPaneli.SetActive(false);
        }

        // Düþmanlarý say
        AI_Controller[] tumDusmanlar = FindObjectsOfType<AI_Controller>();
        toplamDusman = tumDusmanlar.Length;
        kalanDusman = toplamDusman;

        Debug.Log($"[GAME MANAGER] Oyun baþladý. Toplam Düþman: {toplamDusman}");
        UpdateUI();
    }

    public void DusmanOldu()
    {
        kalanDusman--;
        if (kalanDusman < 0) kalanDusman = 0;

        UpdateUI();

        // Eðer düþman kalmadýysa kazanma fonksiyonunu çaðýr
        if (kalanDusman <= 0)
        {
            OyunBittiKazanma();
        }
    }

    void UpdateUI()
    {
        if (dusmanSayacText != null)
        {
            dusmanSayacText.text = $"DÜÞMANLAR: {kalanDusman} / {toplamDusman}";
        }
    }

    void OyunBittiKazanma()
    {
        Debug.Log("TEBRÝKLER! BÖLGE TEMÝZLENDÝ!");

        // Oyun bitti panelini (büyük yazýyý) aktif et
        if (oyunBittiPaneli != null)
        {
            oyunBittiPaneli.SetActive(true);

            // Ýstersen oyunun arka planda akmasýný durdurmak için alttaki yorumu kaldýrabilirsin:
            // Time.timeScale = 0f;

            // Mouse imlecini tekrar görünür yap (Menüye týklamak gerekirse diye)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}