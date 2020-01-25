using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBridge : Structure {

    protected override void Awake() {
        base.Awake();

        // UnitBuilder sets the tile type to Structure, override this to Ground so units can walk on it
        // tile.type = TileType.Ground;
    }
}
