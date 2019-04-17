using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceNode {

    ResourceType resourceType();
    float multiplier();
    int remainingResource();

    int extract(int requestedAmount);
}

public class EndlessResourceNode : IResourceNode {

    protected ResourceType rt; 

    public EndlessResourceNode(ResourceType rt) {
        this.rt = rt;
    }

    public int extract(int requestedAmount) { return requestedAmount; }
    public float multiplier() { return 1.0f; }
    public int remainingResource() { return int.MaxValue; }
    public ResourceType resourceType() { return this.rt; }
}

public class LimitedResourceNode : EndlessResourceNode {

    private int remainingResources;

    LimitedResourceNode(ResourceType rt, int richness) : base(rt) {
        remainingResources = richness;
    }

    public new int remainingResource() { return remainingResources; }

    public new int extract(int requestedAmount) {
        if (this.remainingResources >= requestedAmount) {
            remainingResources -= requestedAmount;
            return requestedAmount;
        } else {
            int result = remainingResources;
            remainingResources = 0;
            return result;
        }
    }
}

public class MultiplierResourceNode : IResourceNode {
    protected ResourceType rt;
    protected float mult;

    public MultiplierResourceNode(ResourceType rt, float multiplier) {
        this.rt = rt;
        this.mult = multiplier;
    }

    public int extract(int requestedAmount) { return 0; }
    public float multiplier() { return mult; }
    public int remainingResource() { return 0; }
    public ResourceType resourceType() { return this.rt; }
}
