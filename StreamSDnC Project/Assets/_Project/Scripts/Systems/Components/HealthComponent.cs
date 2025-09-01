using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [field: SerializeField] public float Health { get; private set; }
    [field: SerializeField] public float maxHealth { get; private set; }

    [field: SerializeField] public bool deathMode { get; private set; }

    private void Update()
    {
        if (Health <= 0) Death();
        Health = Mathf.Clamp(Health, 0, maxHealth);
    }

    public void Damage(float damage)
    {
        Health -= damage;
        Debug.Log($"{gameObject.name}: I was hit with {damage} damage");
    }

    void Death()
    {
        if(deathMode) Destroy(this.gameObject);
    }
}
