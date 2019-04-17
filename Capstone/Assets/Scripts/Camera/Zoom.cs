using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour {

    public float zoomSpeed;
    private float scaledZoomSpeed;

    public float zoomFloor;
    public float zoomCeil;
    private Camera cam;

    // Start is called before the first frame update
    void Start() {
        this.cam = this.GetComponent<Camera>();
        this.scaledZoomSpeed = zoomSpeed / 100;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.R)) {
            // R is zoom in
            zoomInOneTick();
        } else if (Input.GetKey(KeyCode.F)) {
            // F is zoom out
            zoomOutOneTick();
        }
    }

    void zoomInOneTick() {
        this.cam.orthographicSize = Mathf.Max(zoomFloor, this.cam.orthographicSize - scaledZoomSpeed);
    }

    void zoomOutOneTick()
    {
        this.cam.orthographicSize = Mathf.Min(zoomCeil, this.cam.orthographicSize + scaledZoomSpeed);

    }
}
