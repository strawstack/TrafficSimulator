using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadObj : MonoBehaviour {
    
    public int r1;
    public int c1;
    public int r2;
    public int c2;
    public IntersectionObj i1;
    public IntersectionObj i2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IntersectionObj getOtherIntersection(IntersectionObj inter)
    {
        return (inter == i1) ? i2 : i1;
    }

    public string hash()
    {
        // returns unique hash identifier for edge
        // lower coordinate to higher coordinate
        if (r1 == r2)
        {
            if (c1 < c2)
            {
                return r1.ToString() + ":" + c1.ToString() + ":" + r2.ToString() + ":" + c2.ToString();
            }
            else
            {
                return r2.ToString() + ":" + c2.ToString() + ":" + r1.ToString() + ":" + c1.ToString();
            }
        }

        if (r1 < r2)
        {
            return r1.ToString() + ":" + c1.ToString() + ":" + r2.ToString() + ":" + r2.ToString();
        }
        else // r2 < r1
        {
            return r2.ToString() + ":" + c2.ToString() + ":" + r1.ToString() + ":" + r1.ToString();
        }
    }
}
