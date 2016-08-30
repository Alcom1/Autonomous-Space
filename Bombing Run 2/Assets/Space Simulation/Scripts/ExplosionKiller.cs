using UnityEngine;
using System.Collections;

public class ExplosionKiller : MonoBehaviour
{
    private float count;    //Counter for how long the explosion has lasted.

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        count += Time.deltaTime;    //Increment counter.

        //If counter is past explosion lifetime, destroy it.
	    if(count > gameObject.GetComponent<ParticleSystem>().startLifetime)
        {
            Destroy(gameObject, 0);
        }
	}
}
