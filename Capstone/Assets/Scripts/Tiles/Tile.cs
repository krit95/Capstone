using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile {
    /**
     * A tile is the basic unit of position in the game
     * The only important identifying feature of a Tile is the position
     * Two tiles are the same if they have the same position
     * A tile has no consept of edges or neighbors
     */

    public readonly Vector2Int position;
    public int x { get { return position.x; } }
    public int y { get { return position.y; } }

    // TODO: Move all this into the MapController class
    private bool walkable;
    public bool isWalkable { get { return walkable; }
                             set { this.walkable = value; } }

    public Tile(int x, int y) : this(new Vector2Int(x, y)) { }

    public Tile (Vector2Int pos) {
        this.position = pos;
        this.walkable = true;
    }

    public override bool Equals(object obj) {
        // Two tiles are equal if they have the same position
        Tile otherTile = obj as Tile;
        if (otherTile == null) { return false; }

        return this.position.x == otherTile.position.x &&
            this.position.y == otherTile.position.y;
    }

    public override int GetHashCode()
    {
        // Two tiles are equal if they have the same position
        int hash = 17;
        hash = hash * 23 + ( (int)this.position.x );
        hash = hash * 23 + ( (int)this.position.y );
        return hash;
    }
}