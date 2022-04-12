using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayController : MonoBehaviour {

    public GameObject parentCar;
    public int carCounter;

    private CarController carController;
    private IntersectionObj intersection;

    private float goodDrivingTime;
    private float badDrivingTime;
    private float goodDrivingTimer;
    private float badDrivingTimer;
    private bool badDriving;



    void Start () {

        // track the state that this driver is in
        goodDrivingTime  = Random.Range(20f, 15f);
        badDrivingTime   = Random.Range(2f, 5f);
        goodDrivingTimer = goodDrivingTime;
        badDrivingTimer  = badDrivingTime;
        badDriving = (Random.Range(0f, 1f) < 0.5f) ? true : false;

        // save parent and detach to avoid collider issues
        parentCar = transform.parent.gameObject;
        carController = parentCar.GetComponent<CarController>();
        transform.parent = null;
        intersection = null;
        carCounter = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Car") carCounter += 1;
        if (other.tag == "Intersection") intersection = other.gameObject.GetComponent<IntersectionObj>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Car") carCounter -= 1;
        if (other.tag == "Intersection") intersection = null;
    }

    private bool IntersectionState()
    {

        // ignore state of current intersection when turning
        if ((carController.prevIntersectionPos - intersection.gameObject.transform.position).magnitude < 0.1f) return false;

        // car never stops at an intersection with no lights 
        if (intersection.roads.Count > 2)
        {
            // 0 = down, 1 = left, 2 = up, 3 = right
            int facingAngle = ((int)Mathf.Round(transform.eulerAngles.y / 90f)) % 4;

            // only advance if the light is currently green
            if (facingAngle == 1 || facingAngle == 3)
            {
                if (!(intersection.GetState() == TrafficLightController.State.EW_GREEN))
                {
                    return true;
                }
            }
            else // (facingAngle == 0 || facingAngle == 2)
            {
                if (!(intersection.GetState() == TrafficLightController.State.NS_GREEN))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Update () {

        if (parentCar == null)
        {
            Destroy(this);
            return;
        }

        if (badDriving)
        {
            badDrivingTimer -= Time.deltaTime;
            if (badDrivingTimer <= 0f)
            {
                badDriving = false;
                goodDrivingTimer = goodDrivingTime;
            }
            else
            {
                carController.skip = false;
                return;
            }
        }
        else
        {
            goodDrivingTimer -= Time.deltaTime;
            if (goodDrivingTimer <= 0f)
            {
                badDriving = true;
                badDrivingTimer = badDrivingTime;
            }
        }

        // remain with parent car
        transform.position = parentCar.transform.position;
        transform.eulerAngles = parentCar.transform.eulerAngles;

        // update parent car
        bool redLight = false;
        if (intersection != null) redLight = IntersectionState();
        carController.skip = (carCounter > 0) || redLight;

    }
}
