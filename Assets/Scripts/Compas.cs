using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compas : MonoBehaviour {

    public Camera guicamera;

    private Transform arrow;
    GameManager gameManager;

	void Start () {
        GameObject go = GameObject.Find("GameManager");
        if (go != null)
            gameManager = go.GetComponent<GameManager>();
        arrow = transform.GetChild(0);
	}

	void Update () {
        arrow.localRotation = Quaternion.Euler(0, gameManager.windDirection + 90, 0);
        transform.localRotation = Quaternion.Inverse(guicamera.transform.rotation);
	}
}
