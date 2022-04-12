using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionObj : MonoBehaviour {

    public int r;
    public int c;
    public List<RoadObj> roads;
    public string hash;
    public RoadObj parent = null;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public TrafficLightController.State GetState()
    { 
        return transform.Find("TrafficLight").GetComponent<TrafficLightController>().GetState();
    }
}
