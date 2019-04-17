using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileGeneration {

    // TODO: Make distance a float based on distance. Currently it's just int of weight 1 => might not play nice with A* huristic
    public static TileBuilderWrapper buildTiles() {
        // NOTE: Result is accessed via (x,y)

        TileBuilderWrapper result = new TileBuilderWrapper();
        
        // Add the top row of tiles
        for (int x = 0; x < GameSetup.BOARD_WIDTH; x++) {
            Tile t = new Tile(new Vector2Int(x, 0));
            result.addTile(x, 0, t);
        }

        // Add the leftmost col of tiles
        // NOTE [0,0] has already been made, start at [0,1]
        for (int y = 1; y < GameSetup.BOARD_HEIGHT; y++) {
            Tile t = new Tile(new Vector2Int(0, y));
            Tile upNeighbor = result.allTiles[0, y-1];

            result.addTile(0, y, t);

            // Add a double ended edge from (t <-> upNeighbor) with weight of 1
            result.addTwoWayNeighbor(t, upNeighbor, Vector2.Distance(t.position, upNeighbor.position));
        }

        // Add all the remaining tiles
        // NOTE: start at 1,1 
        for (int y = 1; y < GameSetup.BOARD_HEIGHT; y++) {
            for (int x = 1; x < GameSetup.BOARD_WIDTH; x++) {
                Tile t = new Tile(new Vector2Int(x, y));

                result.addTile(x, y, t);

                // Because we initialized the top row and left col
                // we are guarenteed that these operations are NOT out of bounds
                Tile leftNeighbor = result.allTiles[x-1, y];
                Tile upNeighbor   = result.allTiles[x,   y-1];

                result.addTwoWayNeighbor(t, leftNeighbor, Vector2.Distance(t.position, upNeighbor.position));
                result.addTwoWayNeighbor(t, upNeighbor, Vector2.Distance(t.position, upNeighbor.position));
            } // End x loop
        } // End Y loop
        
        return result;
    }
}


// TODO: Consider making an assertion check that to add an edge requires both start and end tile are already in allTiles
public class TileBuilderWrapper {
    /**
     *  This class is a container for the result of building a network of tiles
     *  This object keeps track of all tiles created and the details about all the edges
     *  
     *  No guarentees are made about connectivity of the final network.
     *  Those type of guarentees should come from whatever function returns this object.
     * 
     */


    public Tile[,] allTiles;  // allTiles[x, y] is how it's accessed

    // A mapping of all the edges coming out of a given tile
    public Dictionary<Tile, HashSet<TileEdge>> outgoingEdges;

    // Given a key (Tile object in network), the value (set of Tile object) is all the Tiles that point to the key
    // Use this in tandom with outgoingEdges to find all incoming edges of a provided Tile
    // Mostly used when making a tile un-walkable or when needed to modify incoming edges of a tile
    public Dictionary<Tile, HashSet<Tile>> incomingTiles;

    public TileBuilderWrapper() {
        this.allTiles = new Tile[GameSetup.BOARD_WIDTH, GameSetup.BOARD_HEIGHT];
        this.outgoingEdges = new Dictionary<Tile, HashSet<TileEdge>>();
        this.incomingTiles = new Dictionary<Tile, HashSet<Tile>>();
    }

    public void addTile(int x, int y, Tile t) {
        allTiles[x, y] = t;
    }

    public void addTwoWayNeighbor(Tile t1, Tile t2, float edgeCosts) {
        this.addNeighbor(t1, t2, edgeCosts);
        this.addNeighbor(t2, t1, edgeCosts);
    }

    public void addNeighbor(Tile start, Tile end, float edgeCost) {
        if ( ! outgoingEdges.ContainsKey(start)) {
            outgoingEdges.Add(start, new HashSet<TileEdge>());
        }
        // Add a new edge from start pointing towards end
        TileEdge newOutEdge = new TileEdge(end, Mathf.RoundToInt(edgeCost));
        outgoingEdges[start].Add(newOutEdge);

        if ( ! incomingTiles.ContainsKey(end)) {
            incomingTiles.Add(end, new HashSet<Tile>());
        }
        // End now has an incoming edge from start
        incomingTiles[end].Add(start);
    }
}



public class TileEdge {
    /**
     * To represent the incomming Connection of a tile
     * The only Tile refrence in this TileEdge object represents what is at the end of this edge
     * This TileEdge has no idea, nore does it care, about it's start
     * The start Tile is recorded in the MapController.outgoingEdges dictionary as the key
     */
    public Tile tile;
    public int weight;

    public TileEdge(Tile tile, int weight)
    {
        // TODO: Ensure that weight is never -1
        //       Actually, weight should probably alwasy be distance between the two tiles
        //       Perhapse a better constructor takes in the starting tile and the neighbor tile and calculates everything from there
        this.tile = tile;
        this.weight = weight;
    }

    public override bool Equals(object obj) {
        TileEdge otherTE = obj as TileEdge;
        if (otherTE == null) { return false; }

        return this.tile.Equals(otherTE.tile);
    }
    
    public override int GetHashCode() {
        return tile.GetHashCode();
    }
}
