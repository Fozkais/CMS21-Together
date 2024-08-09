namespace CMS21Together.Shared;

public enum PacketTypes
{
    connect,
    disconnect,
    userData,
    readyState,
    start,
    
    position,
    rotation,
    item,
    groupItem,
    stat,
    lifter,
    loadJobCar,
    loadCar,
    bodyPart,
    partScript,
    deleteCar,
    carPosition,
    garageUpgrade,
    newJob,
    jobAction,
    selectedJob,
    sceneChange,
    contentInfo,
    toolMove,
    setSpringClamp,
    clearSpringClamp,
    setTireChanger,
    clearTireChanger
}