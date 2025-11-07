using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections; // Coroutine için bu gerekli

public class RigActivator : MonoBehaviour
{
    // Inspector'dan buraya RigBuilder'ý sürükleyeceðiz
    public RigBuilder rigBuilder;

    // Ayar: Rig'in açýlmadan önce kaç saniye bekleneceði
    public float delayInSeconds = 1.0f;

    void Start()
    {
        // RigBuilder'ýn baþlangýçta kapalý olduðundan emin olalým
        if (rigBuilder != null)
        {
            rigBuilder.enabled = false;
            // Gecikmeli olarak Rig'i açan fonksiyonu baþlat
            StartCoroutine(EnableRigAfterDelay());
        }
        else
        {
            Debug.LogError("RigActivator: 'Rig Builder' atanmamýþ!");
        }
    }

    private IEnumerator EnableRigAfterDelay()
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(delayInSeconds);

        // Gecikme bittikten sonra Rig Builder'ý aç
        Debug.Log("Rig sistemi gecikmeli olarak aktifleþtiriliyor...");
        rigBuilder.enabled = true;
    }
}