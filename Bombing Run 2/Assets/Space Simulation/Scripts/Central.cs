using UnityEngine;
using System.Collections;
using System.Collections.Generic;   //To use list

public class Central : MonoBehaviour
{
    //Prefabs
    public GameObject BomberPrefab;
    public GameObject MissilePrefab;        //Missiles from bombers.
    public GameObject MissileDefPrefab;     //Defensive missiles from station.
    private GameObject PassTarget;          //Empty for bombers to seek.
    private GameObject bombersCenterTarget; //Empty representing the center of the flock.
    public GameObject ExpLargePrefab;
    public GameObject ExpSmallPrefab;
    public float passDistance;              //Distance from the station to put the pass point.

    //Bombers
    public int numBombers;
    private List<GameObject> bombers;
    public List<GameObject> Bombers
    {
        get { return bombers; }
    }

    //New Bombers
    public int numBomberNews;
    private List<GameObject> bomberNews;
    public List<GameObject> BomberNews
    {
        get { return bomberNews; }
    }

    //Turrets
    private GameObject turret1;
    private GameObject turret2;

    //Missiles
    private List<GameObject> missiles;
    public List<GameObject> Missiles
    {
        get { return missiles; }
    }
    public float reloadTime;

    //Defensive missiles
    private List<GameObject> missileDefs;
    public List<GameObject> MissileDefs
    {
        get { return missileDefs; }
    }
    public float reloadTimeDef;


    //Flock attributes
    private Vector3 bombersCenter;
    public Vector3 BombersCenter
    {
        get { return bombersCenter; }
    }
    private Vector3 bombersDirection;
    public Vector3 BombersDirection
    {
        get { return bombersDirection; }
    }

    //Strings
    private string[] namesStation;
    private string[] namesBombers;

	// Use this for initialization
	void Start()
    {
        //Instantiate GameObjects
        PassTarget = new GameObject("PassTarget");
        bombersCenterTarget = new GameObject("CenterTarget");
        bombers = new List<GameObject>();
        bomberNews = new List<GameObject>();
        missiles = new List<GameObject>();
        missileDefs = new List<GameObject>();
        turret1 = GameObject.Find("Turret1");
        turret2 = GameObject.Find("Turret2");

        //Add bombers
        Vector3 position;
	    for(int i = 0; i < numBombers; i++)
        {
            position = new Vector3(
                Random.Range(-5, 5),
                Random.Range(-5, 5),
                Random.Range(-5, 5));
            GameObject bomber = (GameObject)GameObject.Instantiate(
                BomberPrefab,
                position,
                Quaternion.identity);
            bomber.GetComponent<Bomber2>().target = GameObject.Find("PassTarget");
            bomber.GetComponent<Bomber2>().station = GameObject.Find("Station");
            bomber.GetComponent<Bomber2>().armed = true;
            bomber.name = "bomber" + (i + 1);
            bombers.Add(bomber);
        }

        //Set initial flock attributes and pass point.
        SetPassTargetPos();
        CalcAverage();
        CalcDirection();

        //Instantiate name arrays.
        namesStation = new string[1];
        namesStation[0] = "Station";
        namesBombers = new string[bombers.Count];
        for(int i = 0; i < bombers.Count; ++i)
        {
            namesBombers[i] = bombers[i].name;
        }
	}
	
	// Update is called once per frame
	void Update()
    {
        //Add new incoming bombers
        if(bombers.Count <= 2 - bomberNews.Count)
        {
            Vector3 position;
            for (int i = 0; i < numBomberNews; i++)
            {
                //Random position around the station.
                position = new Vector3(
                    Random.Range(-1, 1),
                    Random.Range(-1, 1),
                    Random.Range(-1, 1));
                position = position.normalized * 100;
                position += GameObject.Find("Station").transform.position;
                GameObject bomber = (GameObject)GameObject.Instantiate(
                    BomberPrefab,
                    position,
                    Quaternion.identity);
                bomber.GetComponent<Bomber2>().target = GameObject.Find("CenterTarget");
                bomber.GetComponent<Bomber2>().station = GameObject.Find("Station");
                bomber.GetComponent<Bomber2>().armed = false;
                foreach(string j in namesBombers)   //Give bomber an unused name.
                {
                    if(GameObject.Find(j) == null)
                    {
                        bomber.name = j;
                    }
                }
                bomberNews.Add(bomber);
            }            
        }

        //Add new bombers to flock if they are close enough.
        for(int i = 0; i < bomberNews.Count; ++i)
        {
            if((bomberNews[i].transform.position - bombersCenter).magnitude < 8 || bombers.Count == 0)
            {
                bomberNews[i].GetComponent<Bomber2>().target = GameObject.Find("PassTarget");
                bomberNews[i].GetComponent<Bomber2>().Arm(0);
                bombers.Add(bomberNews[i]);
                bomberNews.RemoveAt(i);
                i--;
                if (i < 0)
                {
                    i = bomberNews.Count - 1;
                }
            }
        }

        //For each bomber.
	    for(int i = 0; i < bombers.Count; ++i)
        {
            //Bombers firing.
            if (bombers[i].GetComponent<Bomber2>().Fire(GameObject.Find("Station").transform.position, 25))
            {
                GameObject missile = (GameObject)GameObject.Instantiate(
                    MissilePrefab,
                    bombers[i].transform.Find("FiringPoint").transform.position,
                    Quaternion.identity);
                missile.GetComponent<Missile>().vel = bombers[i].GetComponent<Bomber2>().vel;
                missile.GetComponent<Missile>().target = GameObject.Find("Station");
                missile.GetComponent<Missile>().targetNames = namesStation;
                missile.GetComponent<Missile>().AvoidStation = false;
                missile.GetComponent<Missile>().station = GameObject.Find("Station");
                missiles.Add(missile);
            }

            //Bombers reloading.
            if(bombers[i].GetComponent<Bomber2>().Arm(reloadTime))
            {
                SetPassTargetPos();
            }

            //Bomber destruction.
            if (bombers[i].GetComponent<Bomber2>().Collided)
            {
                Instantiate(ExpLargePrefab, bombers[i].transform.position, Quaternion.identity);
                Destroy(bombers[i], 0);
                bombers.RemoveAt(i);
                i--;
                if(i < 0)
                {
                    i = bombers.Count - 1;
                }
            }

            //Turret 1 firing.
            if(turret1.GetComponent<Turret>().Fire(bombers[i].transform.position, 30))
            {
                GameObject missile = (GameObject)GameObject.Instantiate(
                    MissileDefPrefab,
                    turret1.transform.position,
                    turret1.transform.rotation);
                missile.GetComponent<Missile>().vel = missile.transform.forward * missile.GetComponent<Missile>().maxVel;
                missile.GetComponent<Missile>().target = bombers[i];
                missile.GetComponent<Missile>().targetNames = namesBombers;
                missile.GetComponent<Missile>().AvoidStation = true;
                missile.GetComponent<Missile>().station = GameObject.Find("Station");
                missileDefs.Add(missile);
            }

            //Turret 1 reloading.
            turret1.GetComponent<Turret>().Arm(reloadTimeDef);

            //Turret 2 firing.
            if (turret2.GetComponent<Turret>().Fire(bombers[i].transform.position, 30))
            {
                GameObject missile = (GameObject)GameObject.Instantiate(
                    MissileDefPrefab,
                    turret2.transform.position,
                    turret2.transform.rotation);
                missile.GetComponent<Missile>().vel = missile.transform.forward * missile.GetComponent<Missile>().maxVel;
                missile.GetComponent<Missile>().target = bombers[i];
                missile.GetComponent<Missile>().targetNames = namesBombers;
                missile.GetComponent<Missile>().AvoidStation = true;
                missile.GetComponent<Missile>().station = GameObject.Find("Station");
                missileDefs.Add(missile);
            }

            //Turret 2 reloading.
            turret2.GetComponent<Turret>().Arm(reloadTimeDef);
        }

        //Missile destruction
        for (int i = 0; i < missiles.Count; ++i)
        {
            //Explode missiles if it has collided.
            if (missiles[i].GetComponent<Missile>().Collided)
            {
                Instantiate(ExpLargePrefab, missiles[i].transform.position, Quaternion.identity);
                Destroy(missiles[i], 0);
                missiles.RemoveAt(i);
            }

            //Kill missile if it is dead.
            else if (missiles[i].GetComponent<Missile>().Dead)
            {
                Destroy(missiles[i], 0);
                missiles.RemoveAt(i);
            }
        }

        //Defensive missile destruction
        for (int i = 0; i < missileDefs.Count; ++i)
        {
            //Explode missiles if it has collided.
            if (missileDefs[i].GetComponent<Missile>().Collided)
            {
                Instantiate(ExpSmallPrefab, missileDefs[i].transform.position, Quaternion.identity);
                Destroy(missileDefs[i], 0);
                missileDefs.RemoveAt(i);
            }

            //Kill missile if it is dead.
            else if (missileDefs[i].GetComponent<Missile>().Dead)
            {
                Destroy(missileDefs[i], 0);
                missileDefs.RemoveAt(i);
            }
        }

        //Recalculate flock position and direction.
        CalcAverage();
        CalcDirection();
	}

    //Sets the new position for the pass point.
    void SetPassTargetPos()
    {
        Vector3 offset = GameObject.Find("Station").transform.position - bombersCenter;
        offset = offset.normalized * passDistance;
        PassTarget.transform.position = GameObject.Find("Station").transform.position + offset;
    }

    //Calculates the average position of the flock.
    public void CalcAverage()
    {
        Vector3 total = Vector3.zero;

        foreach (GameObject i in bombers)
        {
            total += i.transform.position;
        }

        total /= bombers.Count;

        bombersCenter = total;
        bombersCenterTarget.transform.position = bombersCenter;
    }

    //Calculates the average direction of the flock.
    public void CalcDirection()
    {
        bombersDirection = Vector3.zero;

        foreach (GameObject i in bombers)
        {
            bombersDirection += i.transform.forward;
        }
    }
}
