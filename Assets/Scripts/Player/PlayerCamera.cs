using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerModel playerModel;

    private Vector3 cameraOffset;

    private float rotationX = 0f;

    [SerializeField] private float offSetY; // Posicion de la camara en eje y


    void Awake()
    {
        SuscribeToUpdateManagerEvent();
        GetComponents();
        InitializeCameraPosition();
    }

    // Simulacion de Update
    void UpdatePlayerCamera()
    {
        UpdateCameraFollow();
    }

    void OnDestroy()
    {
        UnsuscribeToUpdateManagerEvent();
    }


    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdatePlayerCamera;
    }

    private void UnsuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate -= UpdatePlayerCamera;
    }

    private void GetComponents()
    {
        playerModel = GetComponentInParent<PlayerModel>();
    }

    private void InitializeCameraPosition()
    {
        cameraOffset = new Vector3(0f, offSetY, -0.1f);
        transform.position = playerModel.transform.position + cameraOffset;
    }

    private void UpdateCameraFollow()
    {
        if (PauseManager.Instance == null) return;
        if (PauseManager.Instance.IsGamePaused) return;
        if (playerModel.IsCooking || playerModel.IsAdministrating || playerModel.IsInTeleportPanel || playerModel.IsInTrashPanel || playerModel.IsInTutorial) return;

        cameraOffset = new Vector3(0f, offSetY, 0.3f);

        float x, y;

        if (DeviceManager.Instance.CurrentDevice == Device.Joystick)
        {
            x = PlayerInputs.Instance.JoystickRotation().x * Time.deltaTime;
            y = PlayerInputs.Instance.JoystickRotation().y * Time.deltaTime;
        }

        else
        {
            x = PlayerInputs.Instance.MouseRotation().x * Time.deltaTime;
            y = PlayerInputs.Instance.MouseRotation().y * Time.deltaTime;
        }

        rotationX -= y;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        playerModel.transform.Rotate(Vector3.up * x);
    }
}
