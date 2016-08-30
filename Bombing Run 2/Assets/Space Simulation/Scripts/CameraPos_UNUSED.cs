using UnityEngine;
using System.Collections;

public class CameraPos : MonoBehaviour
{
    public Vector3 localPos;        //Local position relative to parent objects.
    public Vector3 localPosReverse; //Reversed local position.
    private Vector3 basePos;        //Initial position of the camera.
    private Quaternion baseRot;     //Inital rotation of the camera.

	// Use this for initialization
	void Start ()
    {
        //Record initial position and rotation.
        basePos = transform.position;
        baseRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Keys that change the position.

        //Bomber positions.
        if (Input.GetKeyDown("1") && GameObject.Find("bomber1") != null)
        {
            transform.parent = GameObject.Find("bomber1").transform;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.localPosition = localPosReverse;
                transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            }
            else
            {
                transform.localPosition = localPos;
                transform.localRotation = Quaternion.identity;
            }
        }
        else if (Input.GetKeyDown("2") && GameObject.Find("bomber2") != null)
        {
            transform.parent = GameObject.Find("bomber2").transform;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.localPosition = localPosReverse;
                transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            }
            else
            {
                transform.localPosition = localPos;
                transform.localRotation = Quaternion.identity;
            }
        }
        else if (Input.GetKeyDown("3") && GameObject.Find("bomber3") != null)
        {
            transform.parent = GameObject.Find("bomber3").transform;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.localPosition = localPosReverse;
                transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            }
            else
            {
                transform.localPosition = localPos;
                transform.localRotation = Quaternion.identity;
            }
        }
        else if (Input.GetKeyDown("4") && GameObject.Find("bomber4") != null)
        {
            transform.parent = GameObject.Find("bomber4").transform;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.localPosition = localPosReverse;
                transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            }
            else
            {
                transform.localPosition = localPos;
                transform.localRotation = Quaternion.identity;
            }
        }

        //Initial position.
        else if (Input.GetKeyDown("5"))
        {
            transform.parent = null;
            transform.position = basePos;
            transform.rotation = baseRot;
        }

        else if (Input.GetKeyDown("6"))
        {
            transform.parent = null;
            transform.position = GameObject.Find("Camera Pos 1").transform.position;
            transform.rotation = GameObject.Find("Camera Pos 1").transform.rotation;
        }

        else if (Input.GetKeyDown("7"))
        {
            transform.parent = null;
            transform.position = GameObject.Find("Camera Pos 2").transform.position;
            transform.rotation = GameObject.Find("Camera Pos 2").transform.rotation;
        }

        else if (Input.GetKeyDown("8"))
        {
            transform.parent = null;
            transform.position = GameObject.Find("Camera Pos 3").transform.position;
            transform.rotation = GameObject.Find("Camera Pos 3").transform.rotation;
        }
	}
}
