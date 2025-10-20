
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public Transform orientation;

    float _xRotation;
    float _yRotation;

    private bool isTeleportPannelOpened = false;
    private void Awake()
    {
        PlayerDungeonHUD.OnShowTeleportConfirm += TeleportMessageShow;
        PlayerDungeonHUD.OnHideTeleportConfirm += HideTeleportMessage;
    }

    private void OnDestroy()
    {
        PlayerDungeonHUD.OnShowTeleportConfirm -= TeleportMessageShow;
        PlayerDungeonHUD.OnHideTeleportConfirm -= HideTeleportMessage;
    }

    private void Update()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (isTeleportPannelOpened)  return;

        float x, y;
        if (DeviceManager.Instance == null) return;
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
        _yRotation += x;
        _xRotation -= y;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);

        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }

    private void TeleportMessageShow(string nada)
    {
        isTeleportPannelOpened = true;
    }
    private void HideTeleportMessage()
    {
        isTeleportPannelOpened = false;
    }
}
