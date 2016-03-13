#pragma once

#include <SoyThread.h>


namespace LeapMotion
{
	class TDevice;
	class TFrame;
}



class LeapMotion::TFrame
{
public:
	TFrame() :
		mTime		( true )
	{
	}
	
public:
	SoyTime		mTime;
};
std::ostream& operator<<(std::ostream& out,const LeapMotion::TFrame& in);


class LeapMotion::TDevice : public SoyWorkerThread
{
public:
	TDevice();
	~TDevice();
	
protected:
	virtual bool			Iteration() override;
	
public:
	SoyEvent<TFrame>		mOnFrame;
};



