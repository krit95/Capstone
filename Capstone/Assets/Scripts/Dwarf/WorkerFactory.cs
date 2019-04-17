using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorkerFactory
{

    public static readonly IEnumerable<ResourceChange> WORKER_COST = new List<ResourceChange>()
    {
        new ResourceChange(ResourceType.Gold, 10)
    };

}
