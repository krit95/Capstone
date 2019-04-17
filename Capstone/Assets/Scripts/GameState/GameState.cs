using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class GameState {
    /**
     * To represent the current resources and expected income of a game world
     */

    // All the key values are lowercase!
    public Dictionary<string, Resource> resources;

    public GameState() 
        { this.resources = new Dictionary<string, Resource>(); }

    public GameState(GameState gs) {
        this.resources = new Dictionary<string, Resource>(gs.resources.Count);
        foreach(var kvp in gs.resources) {
            this.resources[kvp.Key] = new Resource(kvp.Value);
        }
    }

    public void timePasses(int time) {
        // Update all the internal resources to increase their value by 
        // their respectice resource rates
        foreach(Resource r in this.resources.Values) {
            r.update(time);
        }
    }


    public bool addResource(Resource newResource) {
        // Returns true if the new resource was added
        // Returns false if the new resource was not
        // NOTE: False result => this GS already has a resource with that given name
        // There can only be 1 resource with a given name
        // If you need to override use the overrideResource func
        if (this.resources.ContainsKey(newResource.name)) { return false; }
        this.resources[newResource.name] = newResource;
        return true;
    }


    public override int GetHashCode() {
        int hash = 17;
        foreach(Resource r in this.resources.Values)
            { hash = (hash * 23) + r.GetHashCode(); }
        return hash;
    }

    public override bool Equals(object obj) {
        // Two Game States are equal if all their fields are the same
        // TODO: To simplify things, perhapse we should round the fields down to the nearest 10s place? Depending on scale? 
        GameState otherGS = obj as GameState;
        if (otherGS == null) { return false; }

        // Different number of resources => fail
        if (this.resources.Count != otherGS.resources.Count) { return false; }

        // Check type of resources
        foreach(KeyValuePair<string, Resource> kvp in this.resources) {
            Resource otherR;
            if ( ! otherGS.resources.TryGetValue(kvp.Key, out otherR)) {
                // Try to access a given key in the other dictionary
                // If it fails, return false
                return false;
            }
            if ( ! kvp.Value.Equals(otherR)) {
                // If the key is contained, but the result is different
                return false;
            }
        }
        return true;
    }

    public override string ToString()
    {
        StringBuilder output = new StringBuilder();
        foreach (Resource r in this.resources.Values)
        {
            Debug.Log("inside: " + r.name + ":" + r.resourceCount);
            output.Append(r.name);
            output.Append(":");
            output.Append(r.resourceCount);
            output.Append(", ");
        }
        return output.ToString();
    }
}