using System;

namespace CMS21Together.Shared;

[Serializable]
public enum PacketTypes
{
	connect,
	disconnect,
	userData,
	readyState,
	start,

	spawn,
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
	clearTireChanger,
	wheelBalance,
	endJob,
	oilBinUse,
	engineCrane,
	skillChange,
	engineStandAngle,
	engineStandSetGroup,
	engineStandTakeOff,
	carFluid,
	exp,
	point,
	resync,
	carWash,
	carPaint,
	useWelder,
	repairPart
}