using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{

    float sensitivity = 1f;
    float viewLimit = 80f;

    float horizontalRotation = 0f;
    float verticalRotation = 0f;

    float verticalAngle = 0f;

    float tilt = 0f;

    GameObject Parent;
    Animator animator => this.gameObject.GetComponent<Animator>();

    public Camera Camera => this.gameObject.GetComponent<Camera>();


    public void SetData(float sens, float view, GameObject Par = null)
    {
        sensitivity = sens;
        viewLimit = view;
        Parent = Par;
    }

    public void PassRotation(Vector2 rotation, float tiltInput = 0)
    {
        horizontalRotation = rotation.x;
        verticalRotation = rotation.y;
        tilt = tiltInput;
    }

    public void Aim(bool aim, float ads = 1)
    {
        animator.SetBool("Aiming", aim);
        animator.SetFloat("Ads", ads);
    }

    private void Update()
    {
        Parent.transform.Rotate(0, horizontalRotation * sensitivity, 0);

        verticalAngle -= verticalRotation * sensitivity;
        verticalAngle = Mathf.Clamp(verticalAngle, -viewLimit, viewLimit);
        transform.localRotation = Quaternion.Euler(verticalAngle, 0, tilt);
    }
}
