using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IResourceProducer {
    // How much of the given resource, do the provided number of workers make per tick?
    //  It's assumed that this number wont change with time. IE to calculate the number
    //  of resources produced across n ticks, simply do n*simulate(numOfWorkers)
    //  If the building gains momentum as time goes on, not based on num of workers then 
    //  A different data structure is needed
    ResourceChange simulate(int numOfWorkers);

    ResourceType targetResource();
}

public class SimpleResourceProducer : IResourceProducer {
    public readonly ResourceType resourceType;
    public readonly Func<int, int> definingEquation; // How much resource can n workers make per tick? F(n) = resources

    public SimpleResourceProducer(ResourceType rt, Func<int, int> definingEquation) {
        this.resourceType = rt;
        this.definingEquation = definingEquation;
    }
    
    public ResourceChange simulate(int numOfWorkers) {
        return new ResourceChange(resourceType, definingEquation(numOfWorkers));
    }
    
    public ResourceType targetResource() {
        return this.resourceType;
    }

    // If you want production per worker see below: 
    // Return the deritive of the definingEquation
    // Note, other methods for calculating this on the fly:
    // [f(x+h) - f(x-h)] / 2h                          two-point method
    // [f(x-2h) - 8f(x-h) + 8f(x+h) - f(x+2h)] / 12h    5 point method
    // Or even some co-routine that's called every GAME_SETUP_TICK_RATE seconds to update some internal state => update derivative

}
