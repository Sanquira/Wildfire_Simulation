using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonAction
{
    GENERATE,
    CLEAR,
    SIMULATE,
    FIRE,
    QUIT
}

public enum ModeAction
{
    ADD,
    REMOVE,
    TOGGLE
}

public class GameManager : MonoBehaviour
{

    private const int NUMBER_OF_TREE_INSTANCES = 20000;
    private const int NUMBER_OF_TREE_GENERATED_PER_FRAME = 150;
    private const int NUMBER_OF_RANDOM_FIRES = 100;

    public event Action<bool> simulationStateChanged;
    public event Action<float> windSpeedChanged;
    public event Action<float> windDirectionChanged;

    public GameObject firepitPrefab;
    public GameObject treePrefab;
    public GameObject treeColliderPrefab;

    private Terrain activeTerrain;
    private FireSimulation fireSim;

    bool isSimulating;
    bool isGenerating;
    ModeAction actionMode;
    public float windSpeed
    {
        get;
        private set;
    }
    public float windDirection
    {
        get;
        private set;
    }

    // Use this for initialization
    void Start()
    {
        isSimulating = false;
        isGenerating = false;
        actionMode = ModeAction.ADD;
        windDirection = 0;
        windSpeed = 0;
        activeTerrain = Terrain.activeTerrain;
        fireSim = activeTerrain.GetComponent<FireSimulation>();
        ClearMap();

        fireSim.SetSimulationActive(isSimulating);
    }

    void OnApplicationQuit()
    {
        ClearMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                TerrainData td = activeTerrain.terrainData;
                switch (actionMode)
                {
                    case ModeAction.ADD:
                        Vector3 hitPoint = hit.point;
                        activeTerrain.AddTreeInstance(CreateTreeInstance(hitPoint.x / td.size.x, hitPoint.z / td.size.z));
                        break;
                    case ModeAction.REMOVE:
                        IDeletable tree = hit.collider.gameObject.GetComponent<IDeletable>();
                        if (tree != null && hit.collider.gameObject.tag == "Tree")
                        {
                            tree.Delete();
                        }
                        break;
                    case ModeAction.TOGGLE:
                        if (hit.collider.gameObject.tag == "Tree")
                        {
                            //Fire fire = hit.collider.GetComponentInChildren<Fire>();
                            IFlamable flammable = hit.collider.GetComponent<IFlamable>();
                            if (flammable == null)
                                break;
                            if (flammable.GetFlameState() == FlameState.BURNING)
                            {
                                flammable.SetFlameState(FlameState.NONE);
                            }
                            else
                            {
                                flammable.SetFlameState(FlameState.BURNING);
                            }
                            break;
                        }
                        if (hit.collider.gameObject.tag == "Fire")
                        {
                            IDeletable firepit = hit.collider.gameObject.GetComponent<IDeletable>();
                            if (firepit != null)
                            {
                                firepit.Delete();
                            }
                            break;
                        }
                        Instantiate(firepitPrefab, hit.point, Quaternion.identity, activeTerrain.transform);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void OnButtonClicked(int action)
    {
        switch ((ButtonAction)action)
        {
            case ButtonAction.GENERATE:
                if (!isGenerating)
                {
                    isGenerating = true;
                    StartCoroutine(GenerateMap());
                }
                isGenerating = false;
                break;
            case ButtonAction.CLEAR:
                ClearMap();
                break;
            case ButtonAction.SIMULATE:
                if (isSimulating)
                    StopSimulation();
                else
                    StartSimulation();
                if (simulationStateChanged != null)
                    simulationStateChanged(isSimulating);
                break;
            case ButtonAction.FIRE:
                StartRandomFire();
                break;
            case ButtonAction.QUIT:
                QuitApp();
                break;
            default:
                break;
        }
    }

    public void OnWindSpeedChanged(float speed)
    {
        windSpeed = speed;
        if (windSpeedChanged != null)
            windSpeedChanged(windSpeed);
    }

    public void OnWindDirectionChanged(float dir)
    {
        windDirection = dir;
        if (windDirectionChanged != null)
            windDirectionChanged(windDirection);
    }

    public void OnModeChanged(int mode)
    {
        actionMode = (ModeAction)mode;
    }

    private IEnumerator GenerateMap()
    {
        ClearMap();

        activeTerrain.drawTreesAndFoliage = false;
        for (int i = 0; i < NUMBER_OF_TREE_INSTANCES; i++)
        {
            activeTerrain.AddTreeInstance(CreateTreeInstance(UnityEngine.Random.value, UnityEngine.Random.value));
            if (i % NUMBER_OF_TREE_GENERATED_PER_FRAME == 0)
                yield return null;
        }
        activeTerrain.drawTreesAndFoliage = true;
        activeTerrain.Flush();
    }

    private TreeInstance CreateTreeInstance(float xPos, float zPos)
    {
        TreeInstance tree = new TreeInstance();
        Vector3 position = new Vector3(xPos, 0, zPos);
        tree.color = Color.green;
        tree.position = position;
        tree.prototypeIndex = 0;
        tree.rotation = 0;
        tree.widthScale = 1;
        tree.heightScale = 1;
        tree.lightmapColor = Color.white;

        position = Vector3.Scale(position, activeTerrain.terrainData.size);
        position.y = activeTerrain.SampleHeight(position);

        GameObject colliderGO = Instantiate(treeColliderPrefab, position, Quaternion.identity, activeTerrain.transform);
        colliderGO.GetComponent<MyTree>().terrainIndex = activeTerrain.terrainData.treeInstanceCount;

        return tree;
    }

    private void ClearMap()
    {
        activeTerrain.terrainData.treeInstances = new TreeInstance[0];
        Collider[] treeColliders = activeTerrain.gameObject.GetComponentsInChildren<Collider>();
        for (int i = 0; i < treeColliders.Length; i++)
        {
            Collider go = treeColliders[i];
            if (go.tag == "Tree")
            {
                GameObject.Destroy(go.gameObject);
            }
            if (go.tag == "Fire")
            {
                GameObject.Destroy(go.gameObject);
            }
        }
        fireSim.ClearSimulation();
        activeTerrain.Flush();
    }

    private void StartSimulation()
    {
        isSimulating = true;
        fireSim.SetSimulationActive(isSimulating);
    }

    private void StopSimulation()
    {
        isSimulating = false;
        fireSim.SetSimulationActive(isSimulating);
    }

    private void StartRandomFire()
    {
        for (int i = 0; i < NUMBER_OF_RANDOM_FIRES; i++)
        {
            Vector3 position = Vector3.Scale(new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), activeTerrain.terrainData.size);
            position.y = activeTerrain.SampleHeight(position);
            Instantiate(firepitPrefab, position, Quaternion.identity, activeTerrain.transform);
        }
    }

    private void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
