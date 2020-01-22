using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  Takes a number of gameobjects and arranges them in a grid infront of an ortho camera for rendering in a raw image
 */
public class ObjectUIRenderer : MonoBehaviour {

    public new Camera camera;
    public Transform container;

    public Dictionary<GameObject, RawImage> images = new Dictionary<GameObject, RawImage>();

    // void Update() {
    //     UpdateCamera();
    // }

    public void UpdateCamera() {
        if (images.Count == 0 || !camera) return;

        int size = Mathf.CeilToInt(Mathf.Sqrt(images.Count));

        RenderTexture texture = new RenderTexture(128 * size, 128 * size, 24);
        camera.orthographicSize = size;
        camera.targetTexture = texture;

        float gap = 2;

        int i = 0;
        foreach(GameObject gameObject in images.Keys) {
            float x = i % size;
            float y = Mathf.Floor((float) i / size);
            gameObject.transform.localPosition = new Vector3(-gap * (size - 1) / 2f + gap * x, -gap * (size - 1) / 2f + gap * y);
            images[gameObject].uvRect = new Rect(x / size, y / size, 1f / size, 1f / size);
            images[gameObject].texture = texture;
            i++;
        }
    }

    public void AddObject(GameObject gameObject, RawImage image) {
        gameObject.transform.SetParent(container);
        images.Add(gameObject, image);
        UpdateCamera();
    }

    public void RemoveObject(GameObject gameObject, bool destroy) {
        if (images.ContainsKey(gameObject)) images.Remove(gameObject);
        UpdateCamera();

        if(destroy) Destroy(gameObject);
    }
}
