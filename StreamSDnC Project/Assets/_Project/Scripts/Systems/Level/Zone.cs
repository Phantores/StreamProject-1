using UnityEngine;
using UnityEngine.UIElements;

namespace Level{
    [RequireComponent(typeof(Collider))]
    public class Zone : MonoBehaviour
    {
        public Bounds bounds { get; private set; }
        public bool includeInactive = true;

        [Header("Simulation Data")]
        public float simulationRange;


        Collider col => GetComponent<Collider>(); // find a way to do programatic bounds later

        private void Awake()
        {
            ZoneRegistry.Instance.RegisterZone(this);
        }
        private void OnDestroy()
        {
            ZoneRegistry.Instance.UnregisterZone(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RecalculateBounds();
            if (!col.isTrigger) Debug.LogWarning($"{gameObject.name}'s should have a trigger collider.");
        }
#endif

        void RecalculateBounds()
        {
            var colliders = GetComponentsInChildren<Collider>(includeInactive: includeInactive);

            if (colliders.Length == 0)
            {
                bounds = new Bounds(transform.position, Vector3.zero);
                return;
            }
            Bounds b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                b.Encapsulate(colliders[i].bounds);

            bounds = b;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.7f, 0.4f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, simulationRange);
        }

        void InformAround(bool enable, Zone second = null)
        {
            ZoneRegistry.Instance.UpdateSimulation(enable, this, second);
        }

        private void OnTriggerEnter(Collider other)
        {
            Simulatable sim = other.GetComponent<Simulatable>();
            if(sim != null)
            {
                sim.to = this;
                InformAround(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Simulatable sim = other.GetComponent<Simulatable>();
            if (sim != null)
            {
                InformAround(false, sim.to);
            }
        }
    }
}
