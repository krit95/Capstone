using System;

// TODO: Make this an actual object
public enum EWork {
    /**
     * To represent the different orders that the LongTermPlanner can return
     */
    Wait,

    BuildBuilding,

    BuyAndAssignWorker,

    EMPTY // To represent a "null" value of work which is different than Wait
}

public class Work {
    public readonly EWork workType;
    public readonly BuildingType buildingType;
    public readonly int frameWait;

    public Work(EWork workType, BuildingType buildingType, int frameWait) {
        this.workType = workType;
        this.buildingType = buildingType;
        this.frameWait = frameWait;
    }
}