using UnityEngine;
using System.Collections;
using System;

public class PlayerGun : MonoBehaviour
{
    public int CurrentAmmo => currentAmmo;
    public int TotalAmmo => totalAmmo;
    public int magazineSize = 30;
    public bool IsReloading => isReloading;
    public event Action<int, int> OnAmmoChanged;
    public event Action OnReloadStart;
    public event Action OnReloadEnd;
    public event Action OnFire;

    public Transform muzzle;
    public Camera aimCamera;
    public Animator animator;

    [Header("Weapon")] 
    public int damage = 25;
    public float fireInterval = 0.15f;
    public float maxRange = 200f;
    public float reloadTime = 2f;
    
    private int currentAmmo = 30;
    private int totalAmmo = 90;
    private bool isReloading;
    private float nextFireTime;

    private void Awake()
    {
        if (aimCamera == null) aimCamera = Camera.main;
        if (animator == null) animator = GetComponentInParent<Animator>();
        if (muzzle == null)
        {
            GameObject m = new GameObject("Muzzle");
            m.transform.SetParent(transform);
            m.transform.localPosition = Vector3.forward * 0.5f;
            muzzle = m.transform;
        }
    }

    private void Update()
    {
        if (GameUIManager.Instance != null && GameUIManager.Instance.CurrentState != GameUIManager.GameState.Playing)
            return;

        if (Input.GetButton("Fire1"))
        {
            Fire();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }

        if (currentAmmo <= 0 && totalAmmo > 0 && !isReloading)
        {
            StartReload();
        }
    }

    private void Fire()
    {
        if (currentAmmo <= 0 || isReloading || Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireInterval;
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        OnFire?.Invoke();

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
                var hud = FindAnyObjectByType<GameHUD>();
                if (hud != null) hud.ShowHitmarker();
            }

            // Impact particle
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.transform.position = hit.point;
            impact.transform.localScale = Vector3.one * 0.1f;
            Destroy(impact.GetComponent<Collider>());
            impact.GetComponent<Renderer>().material.color = Color.black;
            Destroy(impact, 0.2f);
        }

        // Muzzle flash
        GameObject flash = new GameObject("MuzzleFlash");
        flash.transform.position = muzzle.position;
        Light l = flash.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 3f;
        l.intensity = 3f;
        l.color = Color.yellow;
        Destroy(flash, 0.05f);

        if (animator != null) animator.SetTrigger("Shoot");

        if (currentAmmo == 0) StartReload();
    }

    private void StartReload()
    {
        if (totalAmmo <= 0 || isReloading || currentAmmo >= magazineSize) return;

        isReloading = true;
        OnReloadStart?.Invoke();
        if (animator != null) animator.SetTrigger("Reload");
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);

        int ammoToReload = Mathf.Min(magazineSize - currentAmmo, totalAmmo);
        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;
        isReloading = false;

        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        OnReloadEnd?.Invoke();
    }
}
