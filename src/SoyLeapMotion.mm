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


void GetJson(TJsonWriter& Json,const std::string& NamePrefix,const Leap::Bone& Bone)
{
	Json.Push(NamePrefix+"Position", GetVector( Bone.prevJoint() ) );
	Json.Push(NamePrefix+"PositionTip", GetVector( Bone.nextJoint() ) );
	Json.Push(NamePrefix+"Direction", GetVector( Bone.direction() ) );
}

void GetJson(TJsonWriter& Json,const std::string& NamePrefix,const Leap::Finger& Finger)
{
	Json.Push(NamePrefix+"Id", Finger.id() );
	Json.Push(NamePrefix+"Type", Finger.type() );
	
	auto Bone0 = Finger.bone(Leap::Bone::Type::TYPE_METACARPAL);
	auto Bone1 = Finger.bone(Leap::Bone::Type::TYPE_PROXIMAL);
	auto Bone2 = Finger.bone(Leap::Bone::Type::TYPE_INTERMEDIATE);
	auto Bone3 = Finger.bone(Leap::Bone::Type::TYPE_DISTAL);

	GetJson( Json, NamePrefix+"_Bone0_", Bone0 );
	GetJson( Json, NamePrefix+"_Bone1_", Bone1 );
	GetJson( Json, NamePrefix+"_Bone2_", Bone2 );
	GetJson( Json, NamePrefix+"_Bone3_", Bone3 );
}

void GetJson(TJsonWriter& Json,const std::string& NamePrefix,const Leap::Hand& Hand)
{
	Json.Push( NamePrefix+"Id", Hand.id() );
	Json.Push( NamePrefix+"Confidence", Hand.confidence() );

	Json.Push( NamePrefix+"GrabStrength", Hand.grabStrength() );
	Json.Push( NamePrefix+"PalmNormal", GetVector( Hand.palmNormal() ) );
	Json.Push( NamePrefix+"PalmPosition", GetVector( Hand.palmPosition() ) );

	auto Fingers = Hand.fingers();
	for ( auto it=Fingers.begin();	it!=Fingers.end();	it++ )
	{
		auto& Finger = *it;
		
		std::stringstream Name;
		Name << NamePrefix << "Finger" << Finger.type() << "_";

		GetJson( Json, Name.str(), Finger );
	}
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
	Json.Push("InteractionMinMax", GetBounds( LastFrame.interactionBox() ) );

	auto HandList = LastFrame.hands();
	for ( auto it=HandList.begin();	it!=HandList.end();	it++ )
	{
		auto& Hand = *it;
		
		std::string NamePrefix = Hand.isLeft() ? "HandLeft_" : "HandRight_";
		
		GetJson( Json, NamePrefix, Hand );
	}

	Json.Close();
	
	mOnFrame.OnTriggered( Json );

	return true;
}



