using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int selectedWeapon = 0;
    
    private GrapplingGun grappling;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SelectWeapon();
    }

    // Update is called once per frame
    void Update()
    {

        int previousSelectedWeapon = selectedWeapon;
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount - 1)
                selectedWeapon = 0;
            else
                selectedWeapon++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= 0)
                selectedWeapon = transform.childCount - 1;
            else
                selectedWeapon--;
        }

        if (previousSelectedWeapon != selectedWeapon)
        {
            if (grappling != null && grappling.IsGrappling())
            {
                grappling.StopGrapple();
            }
            
            SelectWeapon();
            
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            bool isSelected = (i == selectedWeapon);
            weapon.gameObject.SetActive(isSelected);

            if (isSelected)
            {
                // Try to get a GrapplingGun component from this weapon
                grappling = weapon.GetComponent<GrapplingGun>();
            }

            i++;
        }
    }

   
}
