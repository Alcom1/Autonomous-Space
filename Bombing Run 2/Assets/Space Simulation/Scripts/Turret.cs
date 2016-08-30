using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    private bool armed;                 //True if armed.
    private float counter;              //Arming counter.
    private GameObject missileArmed;    //Visual representation of armed missile.

	// Use this for initialization
	void Start ()
    {
        armed = true;
        counter = 0;
        missileArmed = transform.Find("MissileArmed").gameObject;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    //Here be dragons!
	}

    //Fire a missile.
    public bool Fire(Vector3 targetPos, float border)
    {
        //If armed and within distance to the target, fire. Do nothing otherwise.
        Vector3 distanceVector = targetPos - transform.position;
        float distance = distanceVector.magnitude;
        if (armed && distance < border)
        {
            armed = false;
            missileArmed.GetComponent<Renderer>().enabled = false;
            return true;
        }
        return false;
    }

    //Arms a missile.
    public bool Arm(float reloadTime)
    {
        //If not armed, increment counter, check if counter is beyond reload time, if it is reload.
        if (!armed)
        {
            counter += Time.deltaTime;
            if (counter > reloadTime)
            {
                counter = 0;
                armed = true;
                missileArmed.GetComponent<Renderer>().enabled = true;

                return true;
            }
        }
        return false;
    }
}
