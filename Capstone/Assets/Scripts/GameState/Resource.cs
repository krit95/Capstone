using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource {
    /**
     * To represent a resource counter for a single type of resource
     */
    
    public readonly string name; // The name/ what is this resource

    public int resourceCount; // How many resources are in the stockpile 
    public int workerCount; // How many workers are working this resource? 
    protected readonly int perWorkerTick;  // This value represents how much resource is produced by one worker in one tick
                                           // This value is usually NOT what you want, instead consider using the resourcePerTick value

    // TODO: Consider cashing this value? 
    public int resourcePerTick { get { return this.perWorkerTick * workerCount; } } // This value represents how much resources in produced by all the current workers in one tick


    public Resource(Resource other) : this(other.name, other.perWorkerTick, other.resourceCount, other.workerCount) { }

    // TODO: Make the name of the resource another Enumeration
    public Resource(string name, int perWorkerTick, int resourceCount = 0, int workerCount = 0) {
        this.name = name.ToLower();
        this.perWorkerTick = perWorkerTick;
        this.resourceCount = resourceCount;
        this.workerCount = workerCount;
    }

    public void addWorkers(int workerCount)
        { this.workerCount += workerCount; }

    public int freeWorkers(int requestedNumber) {
        // Removes workers from this resource
        // The result value is the number of workers now freed
        // If the result < requested then all the workers for this resource have been freed
        if (requestedNumber <= 0) {
            return 0;
        }
        requestedNumber = Mathf.Min(requestedNumber, workerCount);
        this.workerCount -= requestedNumber;
        return requestedNumber;

    }

    public void update(int time) {
        // Update this resource's resourceCount field as if it was being worked by all the workers for the given time unit
        this.resourceCount += perWorkerTick * workerCount * time;
    }



    // TODO: Introduce the concept of workers to these override functions
    public override int GetHashCode()
    {
        // The hash is defined as only the current resource value and the per tick resource values
        // The hash does NOT care about worker count
        int hash = 17;
        hash = (hash * 23) + this.resourceCount;
        hash = (hash * 23) + this.resourcePerTick;
        return hash;
    }

    public override bool Equals(object obj) {
        // Tow objects are equal if they have the same resource count AND the same resource per tick rate
        // NOTE this doesn't care about number of workers
        Resource otherR = obj as Resource;
        if (otherR == null) { return false; }
        return otherR.resourceCount == this.resourceCount &&
            otherR.resourcePerTick == this.resourcePerTick;
    }
}

public class RoundedResource : Resource {
    /**
     * A RoundedResources behaves like a regular Resource, and the exact number of resource is 
     * stored in this object. However, the GetHashCode and Equals function have been overrided
     * to consider nearby resource counts the same. 
     * 
     * For example: With a percision of 2
     * this.resourceCount = 1234  -> 1200
     * other.resourceCount = 1299 -> 1200
     * 
     * this.Equal(other) == true (because 1200 == 1200)
     */

    private int percision;

    public RoundedResource(RoundedResource other) : 
        base(other.name, other.perWorkerTick, other.resourceCount, other.workerCount)
    { this.percision = other.percision; }

    public RoundedResource(string name, int perWorkerTick, int resourceCount, int workerCount, int percision) :
        base(name, perWorkerTick, resourceCount, workerCount)
    {
        this.percision = percision;
    }


    // TODO: Introduce the concept of workers to these override functions
    public override int GetHashCode()
    {
        // The hash is defined as only the current resource value and the per tick resource values
        // The hash does NOT care about worker count
        int hash = 17;
        hash = (hash * 23) + MathHelp.sigfigify(this.resourceCount, percision);
        hash = (hash * 23) + MathHelp.sigfigify(this.resourcePerTick, percision + 2); // per tick needs higher percision
        return hash;
    }

    public override bool Equals(object obj)
    {
        // Tow objects are equal if they have the same resource count AND the same resource per tick rate
        // NOTE this doesn't care about number of workers
        Resource otherR = obj as Resource;
        if (otherR == null) { return false; }
        return otherR.resourceCount == this.resourceCount &&
            otherR.resourcePerTick == this.resourcePerTick;
    }

}



