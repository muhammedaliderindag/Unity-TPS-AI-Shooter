using UnityEngine;

// Bu script, Animator'den gelen 'FootStep' mesajýný yakalayýp
// bir üstteki 'AI_Controller' script'ine iletmek için var.
public class AnimationEventHelper : MonoBehaviour
{
    private AI_Controller aiController;

    void Start()
    {
        // Bir üst ebeveyndeki (SWAT_NPC) AI_Controller script'ini bul
        aiController = GetComponentInParent<AI_Controller>();

        if (aiController == null)
        {
            Debug.LogError("AnimationEventHelper: Üst objede AI_Controller bulunamadý!", gameObject);
        }
    }

    // BU FONKSÝYON, 'swat@T-Pose' ÜZERÝNDEKÝ ANIMATOR TARAFINDAN ÇAÐRILACAK:
    public void FootStep()
    {
        if (aiController != null)
        {
            // Mesajý, bulduðumuz asýl AI_Controller'a iletiyoruz
            aiController.FootStep();
        }
    }
}