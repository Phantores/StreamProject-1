using UnityEngine;

public class CameraController : MonoBehaviour
{
    float sensitivity = 1f;
    float viewLimit = 80f;

    float horizontalRotation = 0f;
    float verticalRotation = 0f;

    float verticalAngle = 0f;

    GameObject Parent;

    public void SetData(float sens, float view, GameObject Par = null)
    {
        sensitivity = sens;
        viewLimit = view;
        Parent = Par;
    }

    public void PassRotation(Vector2 rotation)
    {
        horizontalRotation = rotation.x;
        verticalRotation = rotation.y;
    }

    private void Update()
    {
        Parent.transform.Rotate(0, horizontalRotation * sensitivity, 0);

        verticalAngle -= verticalRotation * sensitivity;
        verticalAngle = Mathf.Clamp(verticalAngle, -viewLimit, viewLimit);
        transform.localRotation = Quaternion.Euler(verticalAngle, 0, 0);
    }
}
