#include "SoyLeapMotion.h"
#include <SoyJson.h>


std::ostream& operator<<(std::ostream& out,const LeapMotion::TFrame& in)
{
	//	json
	TJsonWriter Json;
	Json.Push("Time", in.mTime);
	Json.Close();
	
	out << Json.mStream.str();
	return out;
}


LeapMotion::TDevice::TDevice() :
	SoyWorkerThread	("LeapMotion::TDevice", SoyWorkerWaitMode::Sleep )
{

	Start();
}

LeapMotion::TDevice::~TDevice()
{
	WaitToFinish();
}


bool LeapMotion::TDevice::Iteration()
{
	TFrame Frame;
	mOnFrame.OnTriggered( Frame );
	
	return true;
}



