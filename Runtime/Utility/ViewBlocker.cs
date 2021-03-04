using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewBlocker : MonoBehaviour {
    // Where the game believes the camera should be.
    public GameObject targetPosition;
    // The object to use to block the camera.
    public GameObject viewBlocker;

    // How far away the camera needs to get before it activates.
    public float triggerBuffer = 0.15f;
    public float maxScale = 0.5f;


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        Vector3 displacement = transform.position - targetPosition.transform.position;
        float distance = displacement.magnitude;

        if (distance > triggerBuffer) {
            if (!viewBlocker.activeSelf) {
                viewBlocker.SetActive(true);
            }
            viewBlocker.transform.localScale = Mathf.Clamp(distance - triggerBuffer, 0, maxScale) * Vector3.one;
            viewBlocker.transform.position = transform.position + 0.5f * maxScale * displacement.normalized;
            viewBlocker.transform.forward = displacement;
        }
        else if (viewBlocker.activeSelf) {
            viewBlocker.SetActive(false);
        }
    }

}
