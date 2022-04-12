using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour {

    // NOTE: negative x is NORTH

    private GameObject nsRedLight;
    private GameObject nsYellowLight;
    private GameObject nsGreenLight;
    private GameObject ewRedLight;
    private GameObject ewYellowLight;
    private GameObject ewGreenLight;

    public enum State { NS_GREEN, NS_YELLOW, EW_WAIT, EW_GREEN, EW_YELLOW, NS_WAIT };

    private int stateIndex;
    private State[] states = new State[] { State.NS_GREEN, State.NS_YELLOW, State.EW_WAIT, State.EW_GREEN, State.EW_YELLOW, State.NS_WAIT };
    private enum LightColor {GREEN, YELLOW, RED};
    private float[] times;
    private double timer;

    void Start () {

        float green_time = Random.Range(4f, 6f);
        float yellow_time = Random.Range(1f, 3f);
        float wait_time = 1f;
        times = new float[] { green_time, yellow_time, wait_time, green_time, yellow_time, wait_time };

        nsRedLight    = transform.Find("Red_ns").gameObject;
        nsYellowLight = transform.Find("Yellow_ns").gameObject;
        nsGreenLight  = transform.Find("Green_ns").gameObject;
        ewRedLight    = transform.Find("Red_ew").gameObject;
        ewYellowLight = transform.Find("Yellow_ew").gameObject;
        ewGreenLight  = transform.Find("Green_ew").gameObject;

        stateIndex = Random.Range(0, 5); // initial state for the light is random
        timer = times[stateIndex];
        setState();
    }

    public State GetState()
    {
        return states[stateIndex];
    }

    private void setState()
    {
        switch (states[stateIndex])
        {
            case State.NS_GREEN:
                setEW(LightColor.RED);
                setNS(LightColor.GREEN);
                break;
            case State.NS_YELLOW:
                setEW(LightColor.RED);
                setNS(LightColor.YELLOW);
                break;
            case State.EW_WAIT:
                setNS(LightColor.RED);
                setEW(LightColor.RED);
                break;
            case State.EW_GREEN:
                setNS(LightColor.RED);
                setEW(LightColor.GREEN);
                break;
            case State.EW_YELLOW:
                setNS(LightColor.RED);
                setEW(LightColor.YELLOW);
                break;
            case State.NS_WAIT:
                setEW(LightColor.RED);
                setNS(LightColor.RED);
                break;
        }
    }

    private void setNS(LightColor color)
    {
        // turn off all emmison
        nsRedLight.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        nsYellowLight.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        nsGreenLight.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");

        switch (color)
        {
            case LightColor.RED:
                nsRedLight.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                break;
            case LightColor.YELLOW:
                nsYellowLight.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                break;
            case LightColor.GREEN:
                nsGreenLight.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                break;
        }
    }

    private void setEW(LightColor color)
    {
        // turn off all emmison
        ewRedLight.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        ewYellowLight.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        ewGreenLight.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");

        switch (color)
        {
            case LightColor.RED:
                ewRedLight.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                break;
            case LightColor.YELLOW:
                ewYellowLight.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                break;
            case LightColor.GREEN:
                ewGreenLight.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                break;
        }
    }

    void Update () {
		
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            stateIndex = (stateIndex + 1) % 6;
            timer = times[stateIndex];
            setState();
        }

    }
}
