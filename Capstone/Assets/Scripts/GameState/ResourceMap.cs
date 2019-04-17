using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceMap {


    /*
     * A two layer filter mapping of (position -> Resource Type -> All the modifiers)
     * If there is an entry at the end of the mappings, those are all the related mods for that resource + tile combo
     *  
     * NOTE: This is ugly, but it does have one main advantage:
     *  - Easy look up of every resource node at a given tile regardless of knowing the RT beforhand
     */
    private Dictionary<Vector2Int, Dictionary<ResourceType, List<IResourceNode>>> resourceMap;


    public ResourceMap() {
        this.resourceMap = new Dictionary<Vector2Int, Dictionary<ResourceType, List<IResourceNode>>>();
    }


    public List<IResourceNode> getNodes(Vector2Int pos) {
        List<IResourceNode> result = new List<IResourceNode>();
        if (!this.resourceMap.ContainsKey(pos)) {
            return result;
        }
        // Else the key exists
        foreach(List<IResourceNode> leaf in this.resourceMap[pos].Values) {
            result.AddRange(leaf);
        }
        return result;
    }

    public List<IResourceNode> getNodes(Vector2Int pos, ResourceType rt) {
        if ( ! containsLeaf(pos, rt)) {
            return new List<IResourceNode>();
        }
        return this.resourceMap[pos][rt];
    }
    
    public void addNode(IResourceNode newNode, Vector2Int pos) {
        ResourceType rt = newNode.resourceType();
        if ( ! resourceMap.ContainsKey(pos)) {
            resourceMap[pos] = new Dictionary<ResourceType, List<IResourceNode>>();
        }
        if ( ! resourceMap[pos].ContainsKey(rt)) {
            resourceMap[pos][rt] = new List<IResourceNode>();
        }
        this.resourceMap[pos][rt].Add(newNode);
    }
    
    private bool containsLeaf(Vector2Int pos, ResourceType rt) {
        return resourceMap.ContainsKey(pos) &&
               resourceMap[pos].ContainsKey(rt);
    }
}
