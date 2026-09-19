using UnityEngine;
using System.Collections;

public enum BotState { Idle, Patrol, Detect, Chase, Attack, TakeCover, Dead }

[RequireComponent(typeof(CharacterController), typeof(EnemyHealth))]
public class BotAI : MonoBehaviour
{
    public BotState CurrentState;

    public float moveSpeed = 3.5f;
    public float runSpeed = 5.5f;
    public float detectionRange = 30f;
    public float attackRange = 20f;
    public float attackInterval = 0.8f;
    public int attackDamage = 8;
    public float fieldOfView = 120f;
    public float coverHealthThreshold = 0.3f; 

    private CharacterController controller;
    private Animator animator;
    private EnemyHealth health;
    private Transform target;

    private Vector3 patrolTarget;
    private float nextPatrolTime;
    private float nextAttackTime;
    private float coverWaitTime;
    private float gravity = -9.81f;
    private float velocityY = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<EnemyHealth>();
        
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) target = pc.transform;

        health.OnEnemyDeath += HandleDeath;
        CurrentState = BotState.Patrol;
    }

    private void Update()
    {
        if (GameUIManager.Instance != null && GameUIManager.Instance.CurrentState != GameUIManager.GameState.Playing)
            return;

        if (CurrentState == BotState.Dead) return;

        if (target == null)
        {
            PlayerController pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) target = pc.transform;
            return;
        }

        switch (CurrentState)
        {
            case BotState.Patrol: UpdatePatrol(); break;
            case BotState.Chase: UpdateChase(); break;
            case BotState.Attack: UpdateAttack(); break;
            case BotState.TakeCover: UpdateTakeCover(); break;
        }

        // Always check detection if not dead or already attacking/chasing
        if (CurrentState == BotState.Patrol || CurrentState == BotState.Idle)
        {
            CheckDetection();
        }

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded) velocityY = -2f;
        else velocityY += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocityY * Time.deltaTime, 0));
    }

    private void UpdatePatrol()
    {
        if (Time.time > nextPatrolTime)
        {
            if (Vector3.Distance(transform.position, patrolTarget) < 1f || patrolTarget == Vector3.zero)
            {
                patrolTarget = transform.position + new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                nextPatrolTime = Time.time + Random.Range(2f, 4f);
            }
            else
            {
                MoveTowards(patrolTarget, moveSpeed);
                if (animator != null) animator.SetFloat("Speed", 0.5f);
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    private void CheckDetection()
    {
        Vector3 dirToTarget = target.position - transform.position;
        float dist = dirToTarget.magnitude;

        if (dist < detectionRange)
        {
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle < fieldOfView / 2f)
            {
                if (Physics.Raycast(transform.position + Vector3.up, dirToTarget.normalized, out RaycastHit hit, detectionRange))
                {
                    if (hit.transform.IsChildOf(target))
                    {
                        CurrentState = BotState.Chase;
                    }
                }
            }
        }
    }

    private void UpdateChase()
    {
        float dist = Vector3.Distance(transform.position, target.position);
        
        if (dist < attackRange)
        {
            CurrentState = BotState.Attack;
            return;
        }
        
        if (dist > detectionRange * 1.5f)
        {
            CurrentState = BotState.Patrol;
            return;
        }

        MoveTowards(target.position, runSpeed);
        if (animator != null) animator.SetFloat("Speed", 1.0f);
    }

    private void UpdateAttack()
    {
        float dist = Vector3.Distance(transform.position, target.position);
        
        if (dist > attackRange * 1.2f)
        {
            CurrentState = BotState.Chase;
            return;
        }

        if (health.CurrentHealth < health.maxHealth * coverHealthThreshold)
        {
            CurrentState = BotState.TakeCover;
            coverWaitTime = Time.time + 3f;
            return;
        }

        FaceTarget(target.position);
        if (animator != null) animator.SetFloat("Speed", 0f);

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackInterval;
            if (animator != null) animator.SetTrigger("Shoot");
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projObj.transform.position = transform.position + Vector3.up * 1.5f + transform.forward;
        projObj.transform.localScale = Vector3.one * 0.2f;
        projObj.GetComponent<Renderer>().material.color = Color.red;
        
        Collider col = projObj.GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = projObj.AddComponent<Rigidbody>();
        Projectile proj = projObj.AddComponent<Projectile>();
        
        Vector3 dir = (target.position + Vector3.up * 1f - projObj.transform.position).normalized;
        proj.Launch(gameObject, dir, 30f, attackDamage);
    }

    private void UpdateTakeCover()
    {
        if (Time.time < coverWaitTime)
        {
            GameObject[] covers = GameObject.FindGameObjectsWithTag("Cover");
            GameObject bestCover = null;
            float bestDist = 15f;
            foreach (var c in covers)
            {
                float d = Vector3.Distance(transform.position, c.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestCover = c;
                }
            }

            if (bestCover != null)
            {
                Vector3 coverPos = bestCover.transform.position + (bestCover.transform.position - target.position).normalized * 2f;
                MoveTowards(coverPos, runSpeed);
                if (animator != null) animator.SetFloat("Speed", 1.0f);
            }
        }
        else
        {
            if (health.CurrentHealth < health.maxHealth * coverHealthThreshold)
            {
                coverWaitTime = Time.time + 3f; // Stay in cover
            }
            else
            {
                CurrentState = BotState.Chase;
            }
        }
    }

    private void MoveTowards(Vector3 pos, float speed)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            controller.Move(dir.normalized * speed * Time.deltaTime);
            FaceTarget(pos);
        }
    }

    private void FaceTarget(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    private void HandleDeath()
    {
        CurrentState = BotState.Dead;
        controller.enabled = false;
        if (animator != null) animator.SetTrigger("Die");
        
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.AddKill(gameObject.name);
        }

        StartCoroutine(DespawnRoutine());
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(3f);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;
    }
}
