using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadNetworkCreator : MonoBehaviour
{

    public GameObject topLeftIntersection;
    public GameObject intersectionPrefab;
    public GameObject roadPrefab;
    public int columns;
    public int rows;

    private float interDistance = 3f; // space between intersections
    private float ROAD_HEIGHT = 0.3f;
    public IntersectionObj[,] intersections;

    private GameController gc;

    void Start()
    {
        gc = GameObject.Find("GameController").GetComponent<GameController>();

        intersections = new IntersectionObj[rows, columns];

        float offsetRow = topLeftIntersection.transform.position.x;
        float offsetCol = topLeftIntersection.transform.position.z;

        // place intersections
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (r == 0 && c == 0)
                {
                    //intersections[0, 0] = new Intersection(r, c, topLeftIntersection, r.ToString() + ":" + c.ToString());
                    intersections[0, 0] = topLeftIntersection.GetComponent<IntersectionObj>();
                    intersections[0, 0].r = r;
                    intersections[0, 0].c = c;
                    intersections[0, 0].hash = r.ToString() + ":" + c.ToString();
                    continue;
                }

                /*
                intersections[r, c] = new Intersection(r, c,
                    Instantiate(intersectionPrefab, new Vector3(
                        offsetRow + r * interDistance,
                        ROAD_HEIGHT,
                        offsetCol + c * interDistance
                    ), Quaternion.identity),
                    r.ToString() + ":" + c.ToString()); */

                GameObject interGO = Instantiate(intersectionPrefab, new Vector3(
                        offsetRow + r * interDistance,
                        ROAD_HEIGHT,
                        offsetCol + c * interDistance
                ), Quaternion.identity);
                intersections[r, c] = interGO.GetComponent<IntersectionObj>();
                intersections[r, c].r = r;
                intersections[r, c].c = c;
                intersections[r, c].hash = r.ToString() + ":" + c.ToString();
            }
        }

        Dictionary<string, RoadObj> roads = new Dictionary<string, RoadObj>();
        Dictionary<string, float> weights = new Dictionary<string, float>();

        // place roads and create road weights
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {

                int[,] neigh = new int[,] { { -1, 0, 0 }, { 1, 0, 0 }, { 0, -1, 90 }, { 0, 1, 90 } };

                for (int n = 0; n < 4; n++)
                {
                    string key = hash(r, c, r + neigh[n, 0], c + neigh[n, 1]);
                    if (r + neigh[n, 0] < 0 || r + neigh[n, 0] >= rows || c + neigh[n, 1] < 0 || c + neigh[n, 1] >= columns)
                    {
                        continue;
                    }

                    // for each intersection, we try to place an edge
                    // unless one exists already
                    // we also maintain a dictionary of weights

                    if (weights.ContainsKey(key)) continue;

                    weights.Add(key, Random.Range(1f, 100f));

                    float rowPos = intersections[r, c].gameObject.transform.position.x + neigh[n, 0] * (interDistance / 2f);
                    float colPos = intersections[r, c].gameObject.transform.position.z + neigh[n, 1] * (interDistance / 2f);

                    Quaternion rot = Quaternion.Euler(new Vector3(0f, neigh[n, 2], 0f));

                    IntersectionObj i1 = intersections[r, c];
                    IntersectionObj i2 = intersections[r + neigh[n, 0], c + neigh[n, 1]];

                    /*
                    RoadObj road = new Road(r, c, r + neigh[n, 0], c + neigh[n, 1],
                        Instantiate(roadPrefab, new Vector3(rowPos, ROAD_HEIGHT, colPos), rot),
                        i1, i2); */
                    
                    GameObject roadGO = Instantiate(roadPrefab, new Vector3(rowPos, ROAD_HEIGHT, colPos), rot);
                    RoadObj road = roadGO.GetComponent<RoadObj>();
                    road.r1 = r;
                    road.c1 = c;
                    road.r2 = r + neigh[n, 0];
                    road.c2 = c + neigh[n, 1];
                    road.i1 = i1;
                    road.i2 = i2;

                    roadGO.SetActive(false);
                    roads.Add(key, road);

                    i1.roads.Add(road);
                    i2.roads.Add(road);
                }
            }
        }

        // find minimum spanning tree
        prims(roads, weights);

        // create list of inactive roads
        List<RoadObj> inactive_roads = new List<RoadObj>();
        foreach (KeyValuePair<string, RoadObj> pair in roads)
        {
            RoadObj road = pair.Value;

            if (!road.gameObject.activeInHierarchy)
            {
                inactive_roads.Add(road);
            }
        }

        List<RoadObj> shuffle_roads = inactive_roads.OrderBy<RoadObj, int>((item) => Random.Range(0, 1000)).ToList<RoadObj>();

        // set more roads to active to create multiple routes 
        int total_roads = roads.Count;
        int number = (int)(total_roads * 0.15f);
        foreach (RoadObj road in shuffle_roads)
        {
            if (number <= 0) break;

            if (!road.gameObject.activeInHierarchy)
            {
                road.gameObject.SetActive(true);
                number -= 1;
            }
        }

        // remove traffic lights from 2-way intersections 
        foreach(IntersectionObj inter in intersections)
        {
            int count = 0;
            foreach(RoadObj road in inter.GetComponent<IntersectionObj>().roads)
            {
                if (road.gameObject.activeInHierarchy) count += 1;
            }
            if (count <= 2)
            {
                inter.gameObject.transform.Find("TrafficLight").gameObject.SetActive(false);
            }
        }

        // delete roads that are inactive
        /*
        foreach (KeyValuePair<string, RoadObj> pair in roads)
        {
            RoadObj road = pair.Value;
            if (!road.gameObject.activeInHierarchy)
            {
                Destroy(road.gameObject.gameObject);
            }
        } */

        List<RoadObj> active_roads = new List<RoadObj>();
        foreach (KeyValuePair<string, RoadObj> pair in roads)
        {
            if (pair.Value.gameObject.activeInHierarchy)
            {
                active_roads.Add(pair.Value);
            }
        }

        // prune intersection lists removing roads that are currently inactive
        foreach (IntersectionObj inter in intersections) 
        {
            List<RoadObj> roads_replace = new List<RoadObj>();
            foreach (RoadObj road in inter.roads) 
            {
                if (road.gameObject.activeInHierarchy) {
                    roads_replace.Add(road);

                } else {
                    Destroy(road.gameObject);

                }
            }
            inter.roads = roads_replace;
        }

        gc.BeginCar(active_roads, intersections);
    }

    string hash(int r1, int c1, int r2, int c2)
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

    void prims(Dictionary<string, RoadObj> roads, Dictionary<string, float> weights)
    {
        Dictionary<string, bool> visited = new Dictionary<string, bool>();
        List<IntersectionObj> q = new List<IntersectionObj>();
        q.Add(roads[hash(8, 8, 9, 8)].i1); // NOTE this is a random value

        Dictionary<IntersectionObj, float> dist = new Dictionary<IntersectionObj, float>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                dist[intersections[r, c]] = float.PositiveInfinity;
            }
        }

        while (visited.Count < roads.Count)
        {
            if (q.Count == 0) break;

            // simulated heap
            q.Sort((i1, i2) => (dist[i1] < dist[i2]) ? -1 : 1);

            IntersectionObj nx = q[0];
            q.RemoveAt(0);

            if (visited.ContainsKey(nx.hash)) continue;
            visited.Add(nx.hash, true);

            if (nx.parent != null)
            {
                nx.parent.gameObject.SetActive(true);
            }

            // add reachable intersections to queue
            foreach (RoadObj road in nx.roads)
            {
                IntersectionObj other = road.getOtherIntersection(nx);
                if (visited.ContainsKey(other.hash)) continue;

                if (weights[road.hash()] < dist[other])
                {
                    dist[other] = weights[road.hash()];
                    q.Add(other);
                    other.parent = road;
                }
            }
        }
    }

    void Update()
    {

    }

}
/*
public class Road
{
    public int r1;
    public int c1;
    public int r2;
    public int c2;
    public Intersection i1;
    public Intersection i2;
    public GameObject road;
    public Road(int r1, int c1, int r2, int c2, GameObject road, Intersection i1, Intersection i2)
    {
        this.r1 = r1;
        this.c1 = c1;
        this.r2 = r2;
        this.c2 = c2;
        this.road = road;
        this.i1 = i1;
        this.i2 = i2;
    }

    public Intersection getOtherIntersection(Intersection inter)
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
} */

/*
public class Intersection
{
    public int r;
    public int c;
    public GameObject intersection;
    public List<Road> roads;
    public string hash;
    public Road parent = null;
    public Intersection(int r, int c, GameObject intersection, string hash)
    {
        this.r = r;
        this.c = c;
        this.intersection = intersection;
        this.hash = hash;
        roads = new List<Road>();
    }
} */
