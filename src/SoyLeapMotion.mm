#include "SoyLeapMotion.h"
#include <SoyJson.h>


vec3f GetVector(const Leap::Vector& v)
{
	return vec3f( v.x, v.y, v.z );
}

Soy::Bounds3f GetBounds(const Leap::InteractionBox& Box)
{
	vec3f HalfSize( Box.width(), Box.height(), Box.depth() );
	HalfSize.x /= 2.f;
	HalfSize.y /= 2.f;
	HalfSize.z /= 2.f;
	vec3f Center = GetVector( Box.center() );
	vec3f Min( Center.x - HalfSize.x, Center.y - HalfSize.y, Center.z - HalfSize.z );
	vec3f Max( Center.x + HalfSize.x, Center.y + HalfSize.y, Center.z + HalfSize.z );
	return Soy::Bounds3f( Min, Max );
}


std::string GetJson(const Leap::Bone& Bone)
{
	TJsonWriter Json;
	Json.Push("Position", Bone.prevJoint() );
	Json.Push("PositionTip", Bone.nextJoint() );
	Json.Push("Direction", Bone.direction() );
	Json.Close();
	return Json.mStream.str();
}

void GetJson(TJsonWriter& Json,const Leap::Finger& Finger)
{
	Json.Push("Id", Finger.id() );
	Json.Push("Type", Finger.type() );
	
	auto Bone0 = Finger.bone(Leap::Bone::Type::TYPE_METACARPAL);
	auto Bone1 = Finger.bone(Leap::Bone::Type::TYPE_PROXIMAL);
	auto Bone2 = Finger.bone(Leap::Bone::Type::TYPE_INTERMEDIATE);
	auto Bone3 = Finger.bone(Leap::Bone::Type::TYPE_DISTAL);

	BufferArray<std::string,4> BoneJsons;
	BoneJsons.PushBack( GetJson(Bone0) );
	BoneJsons.PushBack( GetJson(Bone1) );
	BoneJsons.PushBack( GetJson(Bone2) );
	BoneJsons.PushBack( GetJson(Bone3) );
	
	Json.PushJson("Bones", GetArrayBridge(BoneJsons) );
}

void GetJson(TJsonWriter& Json,const Leap::Hand& Hand)
{
	Json.Push("Id", Hand.id() );
	Json.Push("Confidence", Hand.confidence() );

	Json.Push("GrabStrength", Hand.grabStrength() );
	Json.Push("PalmNormal", Hand.palmNormal() );
	Json.Push("PalmPosition", Hand.palmPosition() );

	auto Fingers = Hand.fingers();
	for ( auto it=Fingers.begin();	it!=Fingers.end();	it++ )
	{
		auto& Finger = *it;
		TJsonWriter FingerJson;
		GetJson( FingerJson, Finger );
		FingerJson.Close();
		
		std::stringstream Name;
		Name << "Finger" << Finger.type();
		Json.Push( Name.str().c_str(), FingerJson );
	}
	
	Json.Close();
}

LeapMotion::TDevice::TDevice() :
	SoyWorkerThread	("LeapMotion::TDevice", SoyWorkerWaitMode::Wake ),
	mController		( *this )
{
	Start();
	
	//	do an initial run
	Wake();
}

LeapMotion::TDevice::~TDevice()
{
	WaitToFinish();
}


bool LeapMotion::TDevice::Iteration()
{
	//	if it was connected, send a not-connected frame
	if ( !mController.isConnected() )
	{
		mOnError.OnTriggered("Not connected");
		return true;
	}
	
	auto LastFrame = mController.frame();
	
	if ( !LastFrame.isValid() )
	{
		mOnError.OnTriggered("Last frame invalid");
		return true;
	}
	
	TJsonWriter Json;
	
	Json.Push("FramesPerSecond", LastFrame.currentFramesPerSecond() );
	Json.Push("FrameId", LastFrame.id() );
	Json.Push("InteractionBox", GetBounds( LastFrame.interactionBox() ) );

	auto HandList = LastFrame.hands();
	for ( auto it=HandList.begin();	it!=HandList.end();	it++ )
	{
		auto& Hand = *it;
		TJsonWriter HandJson;
		GetJson( HandJson, Hand );

		if ( Hand.isLeft() )
			Json.Push("LeftHand", HandJson );
		else if ( Hand.isRight() )
			Json.Push("RightHand", HandJson );
		else
			Json.Push("OtherHand", HandJson );
	}

	Json.Close();
	
	mOnFrame.OnTriggered( Json );

	return true;
}



