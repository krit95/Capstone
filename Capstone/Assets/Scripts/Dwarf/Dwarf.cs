using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class Dwarf : MonoBehaviour {


    Tile currentTile;

    // How much time in seconds should the dwarf wait before moving to a new tile?
    // Can move n tiles per tick
    private static float MOVEMENT_SPEED { get { return GameSetup.TICK_LENGHT_SEC / 10.0f; } } 
    private static Tilemap tileMap;
    private static TileBase selfPicture;

    // Fields for moving a dwarf
    private Queue<Tile> path;
    private Tile targetTile;

    public static void staticInitalize(Tilemap tileMap, TileBase picture) {
        Dwarf.tileMap = tileMap;
        Dwarf.selfPicture = picture;
    }

    public void initalize(Tile startingTile) {
        currentTile = startingTile;
        drawSelf();
    }

    public void eraseSelf() {
        Vector3Int erasePos = new Vector3Int(this.currentTile.position.x, this.currentTile.position.y, GameSetup.CHARACTER_LAYER);
        tileMap.SetTile(erasePos, null);
    }

    public void drawSelf() {
        // Redraw this dwarf at this.currentTile
        Vector3Int drawPos = new Vector3Int(this.currentTile.position.x, this.currentTile.position.y, GameSetup.CHARACTER_LAYER);
        tileMap.SetTile(drawPos, selfPicture);
    }

    #region Pathfinding
    
    public IEnumerator startPathing(Tile targetTile) {
        updatePath(targetTile);
        yield return StartCoroutine(followPath());
    }

    private void updatePath(Tile targetTile) {
        AStar pathFinder = new AStar(currentTile, targetTile);
        this.path = pathFinder.getPath();
        this.targetTile = targetTile;
    }

    private IEnumerator followPath() {
        while (this.path.Count > 0) {
            // Ask the mapcontroller if the next tile is still open
            if (MapController.singleton.moveDwarf(this, this.path.Peek())) {
                // If we're cleared to move
                this.changePosition(this.path.Dequeue());
                yield return new WaitForSeconds(MOVEMENT_SPEED);
            } else {
                // Else, we were denied clearence to move
                // Something must of changed as we were walking
                // Find a new path
                updatePath(targetTile);
            }

            yield return null;
        }
    }

    private void changePosition(Tile newPos) {
        // Draw yourself onto the map at the provided Tile
        
        eraseSelf();
        this.currentTile = newPos;
        drawSelf();
    }

    #endregion
    
    public IEnumerator assignWork(IBuilding building, Func<Dwarf, IBuilding, bool> callback) {
        Vector2Int buildingPos = building.position();
        yield return StartCoroutine(startPathing(new Tile(buildingPos.x, buildingPos.y)));

        // Once we're ready tell the GameController that we're ready for assignment
        callback(this, building);
        yield break;
    }

}
