using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    public Vector3 vel;             //Velocity
    public float maxVel;            //Maximum velocity

    //objects
    public GameObject target;
    public GameObject station;

    //Weights
    public float WeightPursuit;
    public float WeightAvoid;

    //Collided status.
    private bool collided;
    public bool Collided
    {
        get { return collided; }
    }
    
    //Dead status.
    private bool dead;
    public bool Dead
    {
        get { return dead; }
    }

    //Misc
    public bool AvoidStation;       //True if the station is being avoided.
    public string[] targetNames;    //Names of the objects to collide with.
    public float radius;            //Radius
    public float lifeSpan;          //lifespan
    private float counter;          //counter up to lifespan.

	// Use this for initialization
	void Start ()
    {
        dead = false;
        collided = false;
        counter = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //If counter is past the lifespan, missile is dead.
        counter += Time.deltaTime;
        if (counter > lifeSpan)
        {
            dead = true;
            return;
        }

        Vector3 acc = Vector3.zero;

        if (target != null)
        {
            acc += CalcPursuit(target.transform.position, Vector3.zero) * WeightPursuit;    //Pursue target.
        }
        if(AvoidStation)                                                                //Avoid the station.
        {
            acc += CalcAvoid(station, 8) * WeightAvoid;
        }

        vel += acc; //Add acceleration to velocity.

        transform.position += vel.normalized * maxVel * Time.deltaTime; //Displace
        transform.rotation = Quaternion.LookRotation(vel);              //Face movement direction.
	}

    //Calculate Pursuit
    Vector3 CalcPursuit(Vector3 target, Vector3 tVel)
    {
        Vector3 distance = target - transform.position;    //Distance from vehicle to target.

        //Get pair of possible intercept times based on a quadratic equation.
        //0 = at^2 + bt + c
        float a = Vector3.Dot(tVel, tVel) - maxVel * maxVel;  //a-segment of quadratic equation.
        float b = 2 * Vector3.Dot(tVel, distance);      //b-segment of quadratic equation.
        float c = Vector3.Dot(distance, distance);      //c-segment of quadratic equation.

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
            action = targetPredict - transform.position;
        }
        else  //catch-up is impossible or inconvenient. Seek anyways.
        {
            action = target - transform.position;
        }
        action = action.normalized * maxVel;        //action magnitude == maxVel.
        action -= vel;                              //action magnitude == velocity addition needed for maxVel.
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

    //Check collision.
    void OnCollisionEnter(Collision col)
    {
        //If any targets were collided with, collided is true.
        foreach (string i in targetNames)
        {
            if (col.gameObject.name == i)
            {
                collided = true;
            }
        }
    }
}
