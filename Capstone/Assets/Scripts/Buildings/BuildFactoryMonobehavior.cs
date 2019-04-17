using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildFactoryMonobehavior : MonoBehaviour
{
    public TileBase bankSprite;
    public TileBase masonSprite;
    public TileBase woodCutterSprite;

    public TileBase silverMineSprite;
    public TileBase steelBlacksmithSprite;

    public TileBase ironMineSprite;
    public TileBase coalMineSprite;

    // Start is called before the first frame update
    void Start() {
        BuildingFactory.bankTile = this.bankSprite;
        BuildingFactory.stoneMasonTile = this.masonSprite;
        BuildingFactory.woodCutterTile = this.woodCutterSprite;

        BuildingFactory.silverMineTile = this.silverMineSprite;
        BuildingFactory.steelBlacksmithTile = this.steelBlacksmithSprite;

        BuildingFactory.ironMineTile = this.ironMineSprite;
        BuildingFactory.coalMineTile = this.coalMineSprite;
    }
}
