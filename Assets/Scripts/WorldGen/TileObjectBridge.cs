using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectBridge : TileObject {

    public GameObject model;

    public override void SetDark(bool dark) {
        model.SetActive(!dark);
    }
}
