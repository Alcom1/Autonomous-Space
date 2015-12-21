using UnityEngine;
using System.Collections;
using System.Collections.Generic;   //To use list

public class Bomber2 : MonoBehaviour
{
    public Vector3 vel;     //Velocity

    public float maxVel;    //Maximun velocity
    public float minVel;    //Minimum velocity
    public float maxAcc;    //Maximum acceleration

    //Weights
    public float WeightSeparation;
    public float WeightAlignment;
    public float WeightCohesion;
    public float WeightSeek;
    public float WeightEvade;

    //Objects
    private Central central;
    public GameObject target;
    public GameObject station;
    private GameObject missileArmed;    //Visual representation of armed missile.
    
    //Collided status.
    private bool collided;
    public bool Collided
    {
        get { return collided; }
    }

    //Misc.
    public float radius;    //Radius of the Bomber.
    public bool armed;     //True if armed.
    private float counter;  //Arming counter.

	// Use this for initialization
	void Start ()
    {
        collided = false;
        counter = 0;
        central = GameObject.Find("Central").GetComponent<Central>();
        missileArmed = transform.Find("MissileArmed").gameObject;
        if (!armed)
        {
            missileArmed.GetComponent<Renderer>().enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Apply behaviors.
        Vector3 acc = Vector3.zero;
        acc += CalcSeparation(central.Bombers, 5) * WeightSeparation;   //Flock separation
        acc += CalcAlignment(central.Bombers) * WeightAlignment;        //Flock alignment
        acc += Seek(central.BombersCenter) * WeightCohesion;            //Flock cohesion
        acc += Seek(target.transform.position) * WeightSeek;            //Seek the pass point.
        acc += CalcAvoid(station, 8) * .01f;                           //Avoid the station

        //Evade each defensive missile.
        foreach(GameObject i in central.MissileDefs)
        {
            acc += CalcEvade(i.transform.position, i.GetComponent<Missile>().vel, 8) * WeightEvade * 2;
        }

        acc = Vector3.ClampMagnitude(acc, maxAcc);                              //Limit acceleration
        vel += acc;                                                             //Add acceleration to velocity
        vel = vel.normalized * Map(vel.magnitude, 0, maxVel, minVel, maxVel);   //Map velocity between min and max values.

        transform.position += vel * Time.deltaTime;                                                                     //Displace
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(vel), 4.0f * Time.deltaTime);  //Lerp rotation to reduce wiggle-waggle.
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
        if(!armed)
        {
            counter += Time.deltaTime;
            if(counter > reloadTime)
            {
                counter = 0;
                armed = true;
                missileArmed.GetComponent<Renderer>().enabled = true;

                return true;
            }
        }
        return false;
    }

    //Calculate flock separation
    Vector3 CalcSeparation(List<GameObject> bombers, float border)
    {
        Vector3 action = Vector3.zero;

        //For each bomber.
        foreach(GameObject i in bombers)
        {
            //If bombers are too close, add vector to move away.
            Vector3 temp = transform.position - i.transform.position;
            float distance = temp.magnitude;
            if(distance > 0 && distance < border)
            {
                action += temp / (temp.magnitude * temp.magnitude); //Separation scales with the square of the distance.
            }
        }

        action = action.normalized * maxVel;

        action -= vel;

        return action;
    }

    //Calculate flock alignment
    Vector3 CalcAlignment(List<GameObject> bombers)
    {
        Vector3 action = central.BombersDirection;

        action = action.normalized * maxVel;

        action -= vel;

        return action;
    }

    //Seek
    Vector3 Seek(Vector3 targetPos)
    {
        Vector3 action = targetPos - transform.position;
        action = action.normalized * maxVel;
        action -= vel;
        return action;
    }

    //Flee (Did I even use this?)
    Vector3 Flee(Vector3 targetPos)
    {
        Vector3 action = transform.position - targetPos;
        action = action.normalized * maxVel;
        action -= vel;
        return action;
    }

    //Calculate obstacle avoidance
    Vector3 CalcAvoid(GameObject obst, float safeDistance)
    {
        Vector3 desiredVel = Vector3.zero;                      //Desired velocity.
        float obRadius = obst.GetComponent<Station>().Radius;   //Radius of the obstacle.

        //Displacement between objects.
        Vector3 vecToCenter = obst.transform.position - transform.position;
        vecToCenter.y = 0;

        //Distance betwen objects.
        float dist = Mathf.Abs(vecToCenter.magnitude - obRadius - radius);
        dist = Mathf.Max(dist, 0.01f);

        //Do nothing if too far away.
        if (dist > safeDistance)
        {
            return Vector3.zero;
        }

        //Do nothing if obstacle is behind.
        if (Vector3.Dot(vecToCenter, transform.forward) < 0)
        {
            return Vector3.zero;
        }

        //Vector to the right.
        float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);

        //I forgot why we do this.
        if (Mathf.Abs(rightDotVTC) > radius + obRadius)
        {
            return Vector3.zero;
        }

        //If object is to the right, avoid right, else avoid left.
        if (rightDotVTC > 0)
        {
            desiredVel = transform.right * -maxVel * (safeDistance / dist);
        }
        if (rightDotVTC < 0)
        {
            desiredVel = transform.right * maxVel * (safeDistance / dist);
        }

        desiredVel -= vel;
        desiredVel.y = 0;

        return desiredVel;
    }

    //Calculate perfect evasion.
    Vector3 CalcEvade(Vector3 target, Vector3 tVel)
    {
        Vector3 distance = target - transform.position;    //Distance from vehicle to target.

        //Get pair of possible intercept times based on a quadratic equation.
        //0 = at^2 + bt + c
        float a = maxVel * maxVel - Vector3.Dot(tVel, tVel);  //a-segment of quadratic equation.
        float b = -2 * Vector3.Dot(tVel, distance);      //b-segment of quadratic equation.
        float c = -Vector3.Dot(distance, distance);      //c-segment of quadratic equation.

        //Quadratic formula.
        float quad1 = -b / (2 * a);
        float quad2 = (float)Mathf.Sqrt(b * b - 4 * a * c) / (2 * a);

        //Resulting pair of intercept times.
        float time1 = quad1 - quad2;
        float time2 = quad1 + quad2;

        //Set timeShort to be equal to the shorter of the intercepts.
        float timeShort = 0;  //Shorter of the two time intercepts.
        if (time1 < time2 && time1 > 0 || time2 < 0)
        {
            timeShort = time1;
        }
        else
        {
            timeShort = time2;
        }

        Vector3 action;
        if (!float.IsNaN(timeShort) && timeShort < 5000 && timeShort > 0)  //case where catch-up is possible and timely.
        {
            Vector3 targetPredict = target + tVel * timeShort; //Predicted target position.
            action = transform.position - targetPredict;
        }
        else  //catch-up is impossible or inconvenient. Flee anyways.
        {
            action = transform.position - target;
        }
        action = action.normalized * maxVel;        //action magnitude == maxVel.
        action -= vel;                              //action magnitude == velocity addition needed for maxVel.
        return action;
    }

    //Calculate evasion overload, calculates evasion within a border and with linear distance decay.
    Vector3 CalcEvade(Vector3 target, Vector3 tVel, float border)
    {
        Vector3 action = CalcEvade(target, tVel);                   //Action.
        float distance = (target - transform.position).magnitude;   //Distance between target and position.

        //If withindistance, scale evade and return evade, otherwise return zero vector.
        if(distance < border)
        {
            action *= (border - distance) / border;
            return action;
        }
        else
        {
            return Vector3.zero;
        }
    }

    //Map. Returns a mapped value.
    float Map(float value, float fromA, float toA, float fromB, float toB)
    {
        return (value - fromA) / (toA - fromA) * (toB - fromB) + fromB;
    }
    
    //Check collision.
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "MissileDef(Clone)")
        {
            collided = true;
        }
    }
}
