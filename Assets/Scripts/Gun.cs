using UnityEngine;

public class Gun : MonoBehaviour
{
    public GrapplingGun grappling;
    public float damage = 10f;
    public float range = 100f;
    public float impactForce = 30f;
    public bool isAutomatic = false;

    public float fireRate = 1f;

    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;


    private Recoil Recoil_Script;

    void Start()
    {
        Recoil_Script = GameObject.Find("Head/CameraRecoil").GetComponent<Recoil>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAutomatic)
        {
            if (Input.GetButton("Fire1") &&
                Time.time >= nextTimeToFire) //Change GetbuttonDown for single fire and Getbutton for full auto
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") &&
                Time.time >= nextTimeToFire) //Change GetbuttonDown for single fire and Getbutton for full auto
            {
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }
        }
    }

    void Shoot()
    {
        if (grappling.IsGrappling()) return;

        muzzleFlash.Play();

        Recoil_Script.RecoilFire();

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce, ForceMode.Impulse);
            }

            GameObject impactGo = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            Destroy(impactGo, 2f);
        }
    }
}