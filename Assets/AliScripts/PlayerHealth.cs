using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarlar�")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("UI Ba�lant�lar�")]
    public GameObject gameOverPanel;
    public float panelDelay = 4.0f;

    [Header("Bile�en Ba�lant�lar�")]
    public Animator playerAnimator;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    // --- YEN� EKLENEN KISIM ---
    [Header("�l�nce Kapat�lacaklar")]
    [Tooltip("Karakterin y�r�y�� scriptini buraya s�r�kle (�rn: PlayerMovement)")]
    public MonoBehaviour movementScript;
    [Tooltip("Varsa, ate� etme scriptini de buraya s�r�kle")]
    public MonoBehaviour shootingScript;
    // ---------------------------

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        if (healthSlider != null) { healthSlider.maxValue = maxHealth; healthSlider.minValue = 0; healthSlider.value = currentHealth; }

        // Oyun ba�lad���nda mouse'u kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("<color=red>[OYUNCU �LD�]</color>");

        // 1. Y�r�meyi ve Ate� Etmeyi KAPAT (�NEML�)
        if (movementScript != null) movementScript.enabled = false;
        if (shootingScript != null) shootingScript.enabled = false;

        // 2. Fiziksel kaymay� �nlemek i�in Rigidbody'yi dondur (Varsa)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 3. Animasyonu ba�lat
        if (playerAnimator != null) playerAnimator.SetTrigger("Die");

        // 4. Paneli gecikmeli a�
        StartCoroutine(ShowPanelRoutine());
    }

    private IEnumerator ShowPanelRoutine()
    {
        yield return new WaitForSeconds(panelDelay);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (healthText != null) healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
    }

    void Update()
    {
        // Canl�yken test i�in 'H' tu�u
        if (!isDead && Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }
    }
}