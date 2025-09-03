using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    [field: SerializeField] public float moveSpeed { get; private set; }
    [field: SerializeField] public float jumpForce { get; private set; }
    [field: SerializeField] public float runMult { get; private set; }
    [field: SerializeField] public float crouchMult { get; private set; }
    [field: SerializeField] public float crouchMagnitude { get; private set; }

    [field: SerializeField] public float tiltStrength { get; private set; }

    [field: SerializeField] public float mouseSensitivity { get; private set; }
    [field: SerializeField] public float viewLimit { get; private set; }

    [field: SerializeField] public float terminalVelocity { get; private set; }

    [field: SerializeField] public float interactionRange { get; private set; }

    [field: SerializeField] public float throwStrength { get; private set; } = 1;
}
