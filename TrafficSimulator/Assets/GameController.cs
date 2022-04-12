using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public GameObject carPrefab;

	void Start () {

	}
	
    public void BeginCar(List<RoadObj> roads, IntersectionObj[,] intersections)
    {
        // select random road
        int cars = 80;

        while (cars > 0)
        {
            RoadObj road = roads[Random.Range(0, roads.Count - 1)];

            float x_offset = 0f;
            float z_offset = 0f;
            Quaternion rot = Quaternion.identity;

            if (road.i1.r == road.i2.r)
            {
                x_offset = -0.25f;
                rot = Quaternion.Euler(new Vector3(0f, 90f, 0f));

            } else {
                z_offset = -0.25f;
            }

            // place car on road
            GameObject car = Instantiate(carPrefab, new Vector3(
                road.gameObject.transform.position.x + x_offset,
                0.4449f,
                road.gameObject.transform.position.z + z_offset
                ), rot);

            car.GetComponent<CarController>().curRoad = road;
            cars -= 1;
        }

        // once placed car should handle the rest
    }

    void Update () {
		
	}
}
