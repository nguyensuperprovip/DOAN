namespace DOAN.LegacyCombat {
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    public float lifetime = 3f;
    private GameObject owner;
    private int damage;

    public void Launch(GameObject owner, Vector3 direction, float speed, int damage)
    {
        this.owner = owner;
        this.damage = damage;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner || other.transform.IsChildOf(owner.transform)) return;

        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}

}