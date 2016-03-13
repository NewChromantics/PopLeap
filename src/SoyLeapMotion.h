#pragma once

#include <SoyThread.h>
#include <Leap.h>


class TJsonWriter;

namespace LeapMotion
{
	class TDevice;
}




class LeapMotion::TDevice : public SoyWorkerThread, public Leap::Listener
{
public:
	TDevice();
	~TDevice();
	
protected:
	virtual bool			Iteration() override;
	
protected:
	virtual void			onConnect(const Leap::Controller&) override		{	Wake();	}
	virtual void			onDisconnect(const Leap::Controller&) override	{	Wake();	}
	virtual void			onFrame(const Leap::Controller&) override			{	Wake();	}
	
public:
	SoyEvent<TJsonWriter>		mOnFrame;
	SoyEvent<const std::string>	mOnError;
	
private:
	Leap::Controller		mController;
};



