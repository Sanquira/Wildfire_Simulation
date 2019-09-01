using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSimulation : MonoBehaviour
{
    private const float FIRESPREAD_SIZE_RAD = 5;
    private const float FIRESPREAD_RAD_SQR = FIRESPREAD_SIZE_RAD * FIRESPREAD_SIZE_RAD;
    private const int NUMBER_OF_SIMULATIONS_PER_FRAME = 200;

    GameManager gameManager;

    QuadTree<IFlamable> quadTree;
    List<IFlamable> activeFire;

    Rect fireRect;
    Vector3 windVector;
    Coroutine fireCacheRecalculateCoroutine;

    // Use this for initialization
    void Awake()
    {
        Terrain activeTerrain = Terrain.activeTerrain;
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        quadTree = new QuadTree<IFlamable>(100, new Rect(-FIRESPREAD_SIZE_RAD * 2.1f, -FIRESPREAD_SIZE_RAD * 2.1f, activeTerrain.terrainData.size.x + FIRESPREAD_SIZE_RAD * 2.1f, activeTerrain.terrainData.size.z + FIRESPREAD_SIZE_RAD * 2.1f));
        activeFire = new List<IFlamable>(1000);

        gameManager.windDirectionChanged += gameManager_windDirectionChanged;
        gameManager.windSpeedChanged += gameManager_windSpeedChanged;

        RecalculateWindVector();

        fireRect = new Rect(0, 0, FIRESPREAD_SIZE_RAD * 2, FIRESPREAD_SIZE_RAD * 2);
        enabled = false;

        StartCoroutine(Simulate());
    }

    void gameManager_windSpeedChanged(float obj)
    {
        if (fireCacheRecalculateCoroutine != null)
        {
            StopCoroutine(fireCacheRecalculateCoroutine);
            fireCacheRecalculateCoroutine = null;
        }
        RecalculateWindVector();
        fireCacheRecalculateCoroutine = StartCoroutine(RecalculateFiresCache(windVector));
    }

    void gameManager_windDirectionChanged(float obj)
    {
        if (fireCacheRecalculateCoroutine != null)
        {
            StopCoroutine(fireCacheRecalculateCoroutine);
            fireCacheRecalculateCoroutine = null;
        }
        RecalculateWindVector();
        fireCacheRecalculateCoroutine = StartCoroutine(RecalculateFiresCache(windVector));
    }

    private void RecalculateWindVector()
    {
        windVector = new Vector3(Mathf.Cos(Mathf.Deg2Rad * gameManager.windDirection), 0, Mathf.Sin(Mathf.Deg2Rad * gameManager.windDirection)) * gameManager.windSpeed;
    }

    void OnDrawGizmos()
    {
        if (quadTree != null)
        {
            quadTree.DrawDebug();
        }
    }

    IEnumerator Simulate()
    {
        while (true)
        {
            if (!enabled)
            {
                yield return null;
                continue;
            }

            for (int i = 0; i < activeFire.Count; i++)
            {
                IFlamable fire = activeFire[i];
                List<IFlamable> fireCache = fire.GetCacheList();
                if (fireCache.Count == 0)
                    continue;
                Vector2 position = fire.GetPosition();
                ArrangeRect(ref fireRect, ref position, ref windVector);
                bool atLeastOneHeating = false;
                for (int j = 0; j < fireCache.Count; j++)
                {
                    IFlamable flammable = fireCache[j];
                    FlameState flameState = flammable.GetFlameState();

                    //keep out invalid
                    if (flammable == null || flameState == FlameState.BURNED_OUT || flameState == FlameState.BURNING)
                        continue;

                    atLeastOneHeating = true;
                    if (flammable.GetFlameState() != FlameState.HEATING)
                        flammable.SetFlameState(FlameState.HEATING);

                    flammable.AddTemperature(Random.value + 0.5f);
                }
                if (!atLeastOneHeating)
                    fireCache.Clear();
            }
            yield return null;
        }
    }

    private void ArrangeRect(ref Rect rect, ref Vector2 position, ref Vector3 windVect)
    {
        rect.x = position.x - (1 - windVect.x) * FIRESPREAD_SIZE_RAD;
        rect.y = position.y - (1 + windVect.z) * FIRESPREAD_SIZE_RAD;
    }

    public void AddTree(IFlamable tree)
    {
        if (fireCacheRecalculateCoroutine != null)
        {
            StopCoroutine(fireCacheRecalculateCoroutine);
            fireCacheRecalculateCoroutine = null;
        }
        quadTree.Insert(tree);
        fireCacheRecalculateCoroutine = StartCoroutine(RecalculateFiresCache(windVector));
    }

    public void RemoveTree(IFlamable tree)
    {
        quadTree.Remove(tree);
    }

    public void ClearSimulation()
    {
        quadTree.Clear();
        activeFire.Clear();
    }

    public void SetSimulationActive(bool isSimulating)
    {
        enabled = isSimulating;
    }

    public void AddFire(IFlamable fire)
    {
        activeFire.Add(fire);
        RecalculateFireCache(fire, windVector);
    }

    public void RemoveFire(IFlamable position)
    {
        activeFire.Remove(position);
    }

    private IEnumerator RecalculateFiresCache(Vector3 windVector)
    {
        for (int i = 0; i < activeFire.Count; i++)
        {
            IFlamable fire = activeFire[i];
            RecalculateFireCache(fire, windVector);
        }
        yield return null;
    }

    private void RecalculateFireCache(IFlamable fire, Vector3 windVector)
    {
        Vector2 position = fire.GetPosition();
        Rect r = new Rect(0, 0, FIRESPREAD_SIZE_RAD * 2, FIRESPREAD_SIZE_RAD * 2);
        ArrangeRect(ref r, ref position, ref windVector);
        Vector2 fireRectCenter = r.center;
        List<IFlamable> tmp = fire.GetCacheList();
        tmp.Clear();
        quadTree.Retrieve(ref tmp, ref r);
        //make circle from rect
        tmp.RemoveAll(x =>
        {
            return (x.GetPosition() - fireRectCenter).sqrMagnitude > FIRESPREAD_RAD_SQR;
        });
    }
}
