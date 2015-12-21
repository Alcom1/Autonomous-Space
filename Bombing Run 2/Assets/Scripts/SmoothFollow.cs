using UnityEngine;
using System.Collections;
/*
//This camera smoothes out rotation around the y-axis and height.
//Horizontal Distance to the target is always fixed.
//For every of those smoothed values we calculate the wanted value and the current value.
//Then we smooth it using the Lerp function.
//Then we apply the smoothed values to the transform's position.
*/
public class SmoothFollow : MonoBehaviour 
{
	public GameObject target;
	public float distance = 3.0f;
	public float height = 1.50f;
	public float heightDamping = 2.0f;
	public float positionDamping =2.0f;
	public float rotationDamping = 2.0f;

    // Update is called once per frame
    void Update()
    {
        //Keys that change the position.

        //Bomber positions.
        if (Input.GetKeyDown("1") && GameObject.Find("bomber1") != null)
        {
            target = GameObject.Find("bomber1");
        }
        else if (Input.GetKeyDown("2") && GameObject.Find("bomber2") != null)
        {
            target = GameObject.Find("bomber2");
        }
        else if (Input.GetKeyDown("3") && GameObject.Find("bomber3") != null)
        {
            target = GameObject.Find("bomber3");
        }
        else if (Input.GetKeyDown("4") && GameObject.Find("bomber4") != null)
        {
            target = GameObject.Find("bomber4");
        }

        //Initial position.
        else if (Input.GetKeyDown("5"))
        {
            target = GameObject.Find("Camera Pos 0");
        }

        else if (Input.GetKeyDown("6"))
        {
            target = GameObject.Find("Camera Pos 1");
        }

        else if (Input.GetKeyDown("7"))
        {
            target = GameObject.Find("Camera Pos 2");
        }

        else if (Input.GetKeyDown("8"))
        {
            target = GameObject.Find("Camera Pos 3");
        }

        else if (Input.GetKeyDown("9"))
        {
            target = GameObject.Find("Camera Pos 4");
        }
    }

	// Update is called once per frame
	void LateUpdate ()
	{
		// Early out if we don't have a target
		if (target == null)
			return;
		
		float wantedHeight = target.transform.position.y + height;
		float currentHeight = transform.position.y;
		
		// Damp the height
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);
		
		// Set the position of the camera 
        Vector3 wantedPosition = target.transform.position - target.transform.forward * distance;
		transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * positionDamping);
		
		// adjust the height of the camera
		transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);


        //transform.forward = Vector3.Lerp (transform.forward, target.transform.position - transform.position, Time.deltaTime * rotationDamping);
        transform.forward = Vector3.Lerp(transform.forward, target.transform.forward, Time.deltaTime * rotationDamping);
		
	}
}