using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
 
public class TileDraw : MonoBehaviour {

    public Tilemap groundTileMap;
    public TileBase groundTileBase;
    
    public Tilemap objectTileMap;
    public TileBase rockTileBase;

    public Tilemap peopleTileMap;  // Note, currently dwarfs draw themselves so this should not really be used much

    public Tilemap buildingTileMap;
    public TileBase constructionBase;

    #region specific drawing

    #region ground
    public void drawGround(Tile t) {
        int posX = Mathf.RoundToInt(t.position.x);
        int posY = Mathf.RoundToInt(t.position.y);
        if (groundTileBase == null)
        {
            throw new SystemException();
        }
        groundTileMap.SetTile(new Vector3Int(posX, posY, 0), groundTileBase);
    }
    public void drawGround(IEnumerable<Tile> tiles) {
        foreach(Tile t in tiles) {
            drawGround(t);
        }
    }
    public void drawGround(Tile[,] tiles) {
        foreach(Tile t in tiles) {
            drawGround(t);
        }
    }
    #endregion

    #region rocks
    public void drawNewRock(Tile tile) {
        int posX = Mathf.RoundToInt(tile.position.x);
        int posY = Mathf.RoundToInt(tile.position.y);

        objectTileMap.SetTile(new Vector3Int(posX, posY, 0), rockTileBase);

    }
    public void drawNewRock(IEnumerable<Tile> tiles) {
        foreach(Tile t in tiles) {
            drawNewRock(t);
        }
    }
    #endregion

    #region buildings
    public void drawBuilding(IBuilding b) {
        Vector3Int drawPos = new Vector3Int(b.position().x, b.position().y, GameSetup.BUILDING_LAYER);
        buildingTileMap.SetTile(drawPos, b.buildingIcon());
    }

    public void construction(Vector2Int pos) {
        Vector3Int drawPos = new Vector3Int(pos.x, pos.y, GameSetup.BUILDING_LAYER);
        buildingTileMap.SetTile(drawPos, constructionBase);
    }
    #endregion

    #endregion

    #region general drawing

    private Tilemap getTileMap(GridLayers layer) {
        switch(layer) {
            case GridLayers.Ground: return this.groundTileMap;
            case GridLayers.People: return this.peopleTileMap;
            case GridLayers.Objects: return this.objectTileMap;
            case GridLayers.NONE:
            default: Debug.Log("Error: can't find a NONE gridLayer"); return null;
        }
    }

    public void drawOnTile(TileBase picture, GridLayers layer, int x, int y, int z = 0) {
        getTileMap(layer).SetTile(new Vector3Int(x, y, z), picture);
    }

    #endregion 

}


public enum GridLayers {
    People,  // Note, currently dwarfs draw themselves so this should not really be used much
    Ground,
    Objects,
    NONE
}
