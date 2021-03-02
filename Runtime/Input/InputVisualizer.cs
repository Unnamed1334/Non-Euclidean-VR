using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementMapping))]
public class InputVisualizer : MonoBehaviour {

    public GameObject indicators;

    // Start is called before the first frame update
    void Start() {
        MovementMapping movement = GetComponent<MovementMapping>();
        if (movement) {
            GameObject root = new GameObject();
            root.transform.parent = movement.cameraRig.transform;
            root.transform.localPosition = Vector3.zero;
            indicators = root;

            GameObject go = CreateIndicator();
            go.transform.parent = root.transform;
            go.transform.localPosition = Vector3.zero;
            int targetCount = 12;
            for (int i = 0; i < targetCount; i++) {
                go = CreateIndicator();
                go.transform.parent = root.transform;
                go.transform.localPosition = 0.5f * movement.maxRadius * new Vector3(Mathf.Cos(2 * Mathf.PI / targetCount * i), 0, Mathf.Sin(2 * Mathf.PI / targetCount * i));
            }
            int maxCount = 32;
            for (int i = 0; i < maxCount; i++) {
                go = CreateIndicator();
                go.transform.parent = root.transform;
                go.transform.localPosition = movement.maxRadius * new Vector3(Mathf.Cos(2 * Mathf.PI / maxCount * i), 0, Mathf.Sin(2 * Mathf.PI / maxCount * i));
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }

    /// <summary>
    /// Helper function for creating an indicator without using a prefab.
    /// </summary>
    /// <returns> Returns a colliderless sphere to use as a indicator. </returns>
    public GameObject CreateIndicator() {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(indicator.GetComponent<SphereCollider>());
        return indicator;
    }
}
