using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePit : MonoBehaviour, IDeletable, IFlamable
{

    private FireSimulation fireSim;
    private Vector2 pos;
    private List<IFlamable> fireCache;

    void Awake()
    {
        fireSim = Terrain.activeTerrain.GetComponent<FireSimulation>();
        fireCache = new List<IFlamable>();
        pos = new Vector2(transform.position.x, transform.position.z);


        fireSim.AddFire(this);
        enabled = false;
    }

    public void Delete()
    {
        fireSim.RemoveFire(this);
        Destroy(gameObject);
    }

    public List<IFlamable> GetCacheList()
    {
        return fireCache;
    }

    public Vector2 GetPosition()
    {
        return pos;
    }

    public void SetFlameState(FlameState state)
    {
        throw new System.NotImplementedException();
    }

    public FlameState GetFlameState()
    {
        return FlameState.BURNING;
    }

    public void AddTemperature(float value)
    {
        throw new System.NotImplementedException();
    }
}
