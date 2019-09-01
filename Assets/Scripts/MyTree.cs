using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTree : MonoBehaviour, IDeletable, IFlamable
{
    private const float IGNITION_TEMPERATURE = 500;
    private const float BURNING_TIME_T = 1800;
    private const float TEMPERATURE_DROP_PER_T = 1;
    private const float HEATING_TIME_CHECK_T = 30;

    public int terrainIndex;

    private FlameState flameState;
    private float currTemperatureOrBurnTime;
    private float lastCheck_S;
    private float lastTemp;
    private FireSimulation fireSim;
    private Vector2 pos;
    private List<IFlamable> fireCache;

    void Awake()
    {
        currTemperatureOrBurnTime = 0;
        flameState = FlameState.NONE;
        lastCheck_S = 0;
        lastTemp = 0;
        fireSim = Terrain.activeTerrain.GetComponent<FireSimulation>();
        fireCache = new List<IFlamable>();
        pos = new Vector2(transform.position.x, transform.position.z);


        fireSim.AddTree(this);
        enabled = false;
    }

    void Update()
    {
        if (!fireSim.enabled)
            return;
        if (flameState == FlameState.BURNING)
        {
            if (currTemperatureOrBurnTime >= BURNING_TIME_T)
            {
                SetFlameState(FlameState.BURNED_OUT);
            }
            else
            {
                currTemperatureOrBurnTime += 1;
            }
        }
        if (flameState == FlameState.COOLING)
        {
            if (currTemperatureOrBurnTime <= 0)
            {
                SetFlameState(FlameState.NONE);
            }
            else
            {
                currTemperatureOrBurnTime -= TEMPERATURE_DROP_PER_T;
            }
        }
        if (flameState == FlameState.HEATING)
        {
            if (lastCheck_S >= HEATING_TIME_CHECK_T)
            {
                lastCheck_S = 0;
                if (lastTemp == currTemperatureOrBurnTime)
                {
                    SetFlameState(FlameState.COOLING);
                }
                lastTemp = currTemperatureOrBurnTime;
            }
            lastCheck_S++;
        }
    }

    public List<IFlamable> GetCacheList()
    {
        return fireCache;
    }

    public Vector2 GetPosition()
    {
        return pos;
    }

    public void AddTemperature(float value)
    {
        if (flameState != FlameState.HEATING)
        {
            Debug.LogAssertion("Adding temperature and flameState is not HEATING");
            return;
        }
        currTemperatureOrBurnTime += value;
        if (currTemperatureOrBurnTime >= IGNITION_TEMPERATURE)
        {
            SetFlameState(FlameState.BURNING);
        }
    }

    private void SetTreeColor(Color color)
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData td = terrain.terrainData;
        TreeInstance tree = td.GetTreeInstance(terrainIndex);
        tree.color = color;
        td.SetTreeInstance(terrainIndex, tree);
        //terrain.Flush();
    }

    public void SetFlameState(FlameState state)
    {
        if (state == flameState)
            return;
        switch (state)
        {
            case FlameState.NONE:
                ResetFire();
                break;
            case FlameState.HEATING:
                SetHeating();
                break;
            case FlameState.BURNED_OUT:
                SetBurnedOut();
                break;
            case FlameState.BURNING:
                SetBurning();
                fireSim.AddFire(this);
                break;
            case FlameState.COOLING:
                SetCooling();
                break;
            default:
                break;
        }
        if (flameState == FlameState.BURNING && state != FlameState.BURNING)
        {
            fireSim.RemoveFire(this);
        }
        flameState = state;
    }

    public FlameState GetFlameState()
    {
        return flameState;
    }

    private void SetCooling()
    {
        enabled = true;
        SetTreeColor(Color.cyan);
    }

    private void SetHeating()
    {
        lastTemp = currTemperatureOrBurnTime;
        lastCheck_S = 0;
        enabled = true;
        SetTreeColor(Color.yellow);
    }

    private void SetBurning()
    {
        currTemperatureOrBurnTime = Random.Range(0, 20);
        enabled = true;
        SetTreeColor(Color.red);
    }

    private void SetBurnedOut()
    {
        enabled = false;
        SetTreeColor(Color.black);
    }

    private void ResetFire()
    {
        currTemperatureOrBurnTime = 0;
        enabled = false;
        SetTreeColor(Color.green);
    }

    public void Delete()
    {
        Terrain terrain = Terrain.activeTerrain;
        List<TreeInstance> trees = new List<TreeInstance>(terrain.terrainData.treeInstances);
        trees[terrainIndex] = new TreeInstance();
        terrain.terrainData.treeInstances = trees.ToArray();
        fireSim.RemoveTree(this);
        Destroy(gameObject);
    }
}
