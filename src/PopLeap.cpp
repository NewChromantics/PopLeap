#include "PopLeap.h"
#include <SoyHttpServer.h>
#include "SoyLeapMotion.h"
#include <SoyJson.h>
//#include "TUdpJsonServer.h"


namespace PopLeap
{
	namespace Private
	{
		std::mutex		DebugLogLock;
		std::string		DebugLog;
	}
	
	std::string			GetDebugLog();
	void				PushDebugLog(const std::string& String);

	std::string			gLastError = "Waiting for first frame";
	std::string			gLastFrameJson;
	SoyTime				gLastFrameTime;
	void				OnLeapMotionFrame(TJsonWriter& Frame);
	void				OnLeapMotionError(const std::string& Error);
	void				GetLeapMotionFrame(TJsonWriter& Json);
	void				OnLeapMotionChanged();
	SoyEvent<bool>		gOnLeapMotionChanged;
}



std::string PopLeap::GetDebugLog()
{
	std::lock_guard<std::mutex> Lock( Private::DebugLogLock );
	return Private::DebugLog;
}

void PopLeap::PushDebugLog(const std::string& String)
{
	std::lock_guard<std::mutex> Lock( Private::DebugLogLock );
	Private::DebugLog += String;
}

void PopLeap::OnLeapMotionFrame(TJsonWriter& Frame)
{
	gLastFrameJson = Frame.mStream.str();
	gLastError = std::string();
	gLastFrameTime = SoyTime(true);
	OnLeapMotionChanged();
}

void PopLeap::OnLeapMotionError(const std::string& Error)
{
	gLastError = Error;
	OnLeapMotionChanged();
}

void PopLeap::OnLeapMotionChanged()
{
	bool Dummy = false;
	gOnLeapMotionChanged.OnTriggered( Dummy );
}

void PopLeap::GetLeapMotionFrame(TJsonWriter& Json)
{
	auto& Error = gLastError;
	auto& Frame = gLastFrameJson;
	auto& FrameTime = gLastFrameTime;
	
	Json.Push("Time", FrameTime );

	//	meta
	Json.Push("TimeNow", SoyTime(true) );
	if ( !Error.empty() )
		Json.Push("Error", Error );

	if ( !Frame.empty() )
		Json.MergeJson( Frame );
	
	Json.Close();
}



void OnHttpRequest(const Http::TRequestProtocol& Request,SoyRef Client,THttpServer& HttpServer)
{
	std::Debug << "http request for " << Request.mUrl << " from " << Client << std::endl;

	if ( Request.mUrl.empty() )
	{
		Http::TResponseProtocol Response;
		Response.SetContent("hello! Try /debug and /ping");
		HttpServer.SendResponse( Response, Client );
		return;
	}
	
	if ( Request.mUrl == "debug" )
	{
		Http::TResponseProtocol Response;
		auto DebugLog = PopLeap::GetDebugLog();
		Response.SetContent( DebugLog );
		HttpServer.SendResponse( Response, Client );
		return;
	}
	
	if ( Request.mUrl == "ping" )
	{
		Http::TResponseProtocol Response;
		Response.SetContent( GetArrayBridge(Request.mContent), Request.mContentMimeType );
		HttpServer.SendResponse( Response, Client );
		return;
	}
	
	if ( Request.mUrl == "leap" )
	{
		Http::TResponseProtocol Response;
		
		TJsonWriter Json;
		
		auto EchoVar = Request.GetVariable("echo");
		if ( !EchoVar.empty() )
			Json.Push("echo", EchoVar );
		
		PopLeap::GetLeapMotionFrame( Json );

		Response.SetContent( Json.mStream.str(), SoyMediaFormat::Json );
		HttpServer.SendResponse( Response, Client );
		return;
	}
	
	Http::TResponseProtocol Response( Http::Response_FileNotFound );
	HttpServer.SendResponse( Response, Client );
};

/*

void OnUdpRequest(const Json::TReadProtocol& Request,SoyRef Client,TUdpJsonServer& Server)
{
	std::Debug << "udp request from " << Client << std::endl;
	
	
	
	Http::TResponseProtocol Response( Http::Response_FileNotFound );
	HttpServer.SendResponse( Response, Client );
};
*/




int main()
{
	TPopLeapApp App;

	//	copy the debug output
	std::Debug.GetOnFlushEvent().AddListener( PopLeap::PushDebugLog );
	
	LeapMotion::TDevice LeapMotion;
	LeapMotion.mOnFrame.AddListener( PopLeap::OnLeapMotionFrame );
	LeapMotion.mOnError.AddListener( PopLeap::OnLeapMotionError );
	
	try
	{
		THttpServer HttpServer( 8080, OnHttpRequest );
	//	TUdpJsonServer UdpServer( 9090, OnUdpRequest );

		std::Debug << "HTTP Listening on " << HttpServer.GetListeningPort() << std::endl;
		
		App.mConsoleApp.WaitForExit();
		return 0;
	}
	catch(std::exception& e)
	{
		std::Debug << "Exception: " << e.what() << ". Exiting." << std::endl;
		return 1;
	}
}




