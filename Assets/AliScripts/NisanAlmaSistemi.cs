using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class NisanAlmaSistemi : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimCamera;
    [SerializeField] private int aimPriority = 11;
    [SerializeField] private int hipPriority = 9;

    // Diðer script'ler bu deðiþkene bakarak niþan alýp almadýðýmýzý anlar
    public bool IsAiming { get; private set; }

    void Start()
    {
        IsAiming = false;
        if (aimCamera != null)
        {
            aimCamera.Priority = hipPriority;
        }
        else
        {
            Debug.LogError("NisanAlmaSistemi: 'Aim Camera' atanmamýþ! Lütfen Inspector'dan atayýn.");
        }
    }

    void Update()
    {
        if (aimCamera == null) return;

        // Mouse sað tuþuna "basýlý tutuluyorsa"
        if (Mouse.current.rightButton.isPressed)
        {
            aimCamera.Priority = aimPriority;
            IsAiming = true;
        }
        else
        {
            // Tuþ býrakýldýysa, önceliði tekrar düþür.
            aimCamera.Priority = hipPriority;
            IsAiming = false;
        }
    }
}