using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

    private const float INTER_WIDTH = 1f;
    private const float INTER_HIT_BOX = 1.3f; // width of intersection hitbox
    private const float CAR_FRONT = 0.2f; // centre of car to front bumper
    private const float INTER_OUTER = 0.15f; // distance from edge of intersection cement to edge of hitbox

    public float forwardForce;
    public float speed;
    public float rotationSpeed;
    public RoadObj curRoad;
    public GameObject[] explosive_parts;
    public float explosiveForce;

    private Rigidbody rb;
    private float CAR_HEIGHT = 0.451f;
    private const float LEFT_TURN_RADIUS  = CAR_FRONT + INTER_OUTER + 3f/4f * INTER_WIDTH;
    private const float RIGHT_TURN_RADIUS = CAR_FRONT + INTER_OUTER + 1f/4f * INTER_WIDTH;
    private const float U_TURN_RADIUS = CAR_FRONT + INTER_OUTER + 1f / 4f * INTER_WIDTH;

    private enum State { FORWARD, TURN_RIGHT, TURN_LEFT, U_TURN };
    private State state;

    public bool skip; // skip is updated by CollisionDetection

    private struct TurnInfo {
        public float stateTimestamp; // timestamp when last state was entered    
        public float stateRot; // y rotation when state was entered 
        public float distCounter; // counts how far the car has driven
        public Vector3 origDir; // the direction the car was facing when 
        public Vector3 origPos; // the position the turn began
    }
    private TurnInfo curTurnInfo;

    public Vector3 prevIntersectionPos = Vector3.zero;

    void Start () {
        rb = GetComponent<Rigidbody>();
        state = State.FORWARD;
        skip = false;
    }

    private void Explode()
    {
        foreach (GameObject part in explosive_parts)
        {
            Rigidbody rb = part.AddComponent<Rigidbody>();
            rb.AddForce(Vector3.up * explosiveForce + Vector3.right * Random.Range(0f, explosiveForce/6f) + Vector3.forward * Random.Range(0f, explosiveForce/6f));
            Destroy(part, 4f);
            Destroy(this.gameObject, 5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Intersection")
        {
            IntersectionObj intersection = other.GetComponent<IntersectionObj>();
            Vector3 curLoc = other.gameObject.transform.position;

            if (state == State.FORWARD && (prevIntersectionPos == Vector3.zero || prevIntersectionPos != curLoc)) {

                prevIntersectionPos = curLoc;

                // we randomly pick a road to turn toward
                int roadIndex = Random.Range(0, intersection.roads.Count - 1);

                RoadObj road = intersection.roads[roadIndex];

                if (curRoad == road) // don't pick the same road twice
                {
                    roadIndex = (roadIndex + 1) % intersection.roads.Count;
                    road = intersection.roads[roadIndex];
                }

                curRoad = road;

                //Debug.Log("intersection.roads.Count: " + intersection.roads.Count);

                state = getTurnDirection(intersection, road);

                //Debug.Log("getTurnDirection: " + state);

                curTurnInfo.stateTimestamp = Time.time;
                curTurnInfo.stateRot = transform.eulerAngles.y;
                curTurnInfo.distCounter = 0f;
                curTurnInfo.origDir = transform.right;
                curTurnInfo.origPos = transform.position;
            }
        }
        if (other.tag == "Car")
        {
            Vector3 otherPos = other.gameObject.transform.parent.transform.position;
            if ((this.gameObject.transform.position - otherPos).magnitude > 0.1f)
            {
                Explode();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Intersection")
        {

        }
    }

    private State getTurnDirection(IntersectionObj intersection, RoadObj road) {

        // 0 = down, 1 = left, 2 = up, 3 = right
        int facingAngle = ((int)(transform.eulerAngles.y / 90f)) % 4;

        //Debug.Log("facingAngle: " + facingAngle);

        // intersection the car is currently at
        int ir = intersection.r;
        int ic = intersection.c;

        // the other intersection which connects with the road
        // that the car will be turning onto
        IntersectionObj other = road.getOtherIntersection(intersection);
        int rr = other.r;
        int rc = other.c;

        if (ir == rr) {

            // s -> d
            if (ic < rc) {

                //Debug.Log("Row equal, intersection less");

                if (facingAngle == 0) // down
                {                    
                    return State.TURN_LEFT;
                }
                else if (facingAngle == 1) // left
                {
                    return State.U_TURN;
                } 
                else if (facingAngle == 2) // up
                { 
                    return State.TURN_RIGHT;
                } 
                else // facingAngle == 3 // right
                { 
                    return State.FORWARD;
                }

            } else { // s <- d

                //Debug.Log("Row equal, intersection more");

                if (facingAngle == 0) // down
                {
                    return State.TURN_RIGHT;
                }
                else if (facingAngle == 1) // left
                {
                    return State.FORWARD;
                }
                else if (facingAngle == 2) // up
                {
                    return State.TURN_LEFT;
                }
                else // facingAngle == 3 // right
                {
                    return State.U_TURN;
                }

            }

        } else { // ic == rc

            // s
            // |
            // V
            // d
            if (ir < rr) {

                //Debug.Log("Col equal, intersection higher");

                if (facingAngle == 0) // down
                {
                    return State.FORWARD;
                }
                else if (facingAngle == 1) // left
                {
                    return State.TURN_LEFT;
                }
                else if (facingAngle == 2) // up
                {
                    return State.U_TURN;
                }
                else // facingAngle == 3 // right
                {
                    return State.TURN_RIGHT;
                }
            
            // d
            // ^
            // |
            // s
            } else {

                //Debug.Log("Col equal, intersection lower");

                if (facingAngle == 0) // down
                {
                    return State.U_TURN;
                }
                else if (facingAngle == 1) // left
                {
                    return State.TURN_RIGHT;
                }
                else if (facingAngle == 2) // up
                {
                    return State.FORWARD;
                }
                else // facingAngle == 3 // right
                {
                    return State.TURN_LEFT;
                }
            }
        }
    }

    void Update () {
    
        // maintain height, there is no collision with
        // roads
        transform.position = new Vector3(
            transform.position.x,
            CAR_HEIGHT,
            transform.position.z);
            
        if (skip) return;

        transform.position += transform.right * speed;
        curTurnInfo.distCounter = Vector3.Project(transform.position - curTurnInfo.origPos, curTurnInfo.origDir).magnitude;

        switch (state) {
            case State.FORWARD:                
                break;

            case State.TURN_RIGHT:

                float t = Mathf.Clamp(curTurnInfo.distCounter/RIGHT_TURN_RADIUS, 0f, 1f);
                Vector3 curRot = Vector3.Lerp(new Vector3(0f, curTurnInfo.stateRot, 0f), new Vector3(0f, curTurnInfo.stateRot + 90f, 0f), t);

                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    curRot.y,
                    transform.eulerAngles.z
                );

                float m1 = 4f;
                if (Mathf.Abs(curRot.y - (curTurnInfo.stateRot + 90f)) <=  m1)
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        curTurnInfo.stateRot + 90f,
                        transform.eulerAngles.z
                    );
                    state = State.FORWARD;
                }
                break;

            case State.TURN_LEFT:
                
                float t2 = Mathf.Clamp(curTurnInfo.distCounter/LEFT_TURN_RADIUS, 0f, 1f);
                Vector3 curRot2 = Vector3.Lerp(new Vector3(0f, curTurnInfo.stateRot, 0f), new Vector3(0f, curTurnInfo.stateRot - 90f, 0f), t2);

                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    curRot2.y,
                    transform.eulerAngles.z
                );

                float m = 4f;
                if (Mathf.Abs(curRot2.y - (curTurnInfo.stateRot - 90f)) <= m)
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        curTurnInfo.stateRot - 90f,
                        transform.eulerAngles.z
                    );
                    state = State.FORWARD;
                }
                break;

            case State.U_TURN:

                // U_TURN has a different measurement
                Vector3 adjDir = Vector3.Cross(-1 * Vector3.up, curTurnInfo.origDir);
                curTurnInfo.distCounter = Vector3.Project(transform.position - curTurnInfo.origPos, adjDir).magnitude + 0.01f;

                float t3 = Mathf.Clamp(curTurnInfo.distCounter / U_TURN_RADIUS, 0f, 1f);
                Vector3 curRot3 = Vector3.Lerp(new Vector3(0f, curTurnInfo.stateRot, 0f), new Vector3(0f, curTurnInfo.stateRot - 180f, 0f), t3);

                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    curRot3.y,
                    transform.eulerAngles.z
                );

                float m2 = 4f;
                if (Mathf.Abs(curRot3.y - (curTurnInfo.stateRot - 180f)) <= m2)
                {
                    transform.eulerAngles = new Vector3(
                        transform.eulerAngles.x,
                        curTurnInfo.stateRot - 180f,
                        transform.eulerAngles.z
                    );
                    state = State.FORWARD;
                }                
                break;
        }
    }
}
