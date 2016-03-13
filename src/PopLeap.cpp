#include "PopLeap.h"
#include <SoyHttpServer.h>
#include "SoyLeapMotion.h"


namespace PopLeap
{
	namespace Private
	{
		std::mutex		DebugLogLock;
		std::string		DebugLog;
	}
	
	std::string			GetDebugLog();
	void				PushDebugLog(const std::string& String);

	LeapMotion::TFrame	gLastFrame;
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



void OnHttpRequest(const Http::TRequestProtocol& Request,SoyRef Client,THttpServer& HttpServer)
{
	std::Debug << "request for " << Request.mUrl << " from " << Client << std::endl;

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
		std::stringstream FrameJson;
		FrameJson << PopLeap::gLastFrame;
		Response.SetContent( FrameJson.str(), SoyMediaFormat::Json );
		HttpServer.SendResponse( Response, Client );
		return;
	}
	
	Http::TResponseProtocol Response( Http::Response_FileNotFound );
	HttpServer.SendResponse( Response, Client );
};




int main()
{
	TPopLeapApp App;

	//	copy the debug output
	std::Debug.GetOnFlushEvent().AddListener( PopLeap::PushDebugLog );
	
	auto OnLeapFrame = [](LeapMotion::TFrame& Frame)
	{
		PopLeap::gLastFrame = Frame;
	};
	
	LeapMotion::TDevice LeapMotion;
	LeapMotion.mOnFrame.AddListener( OnLeapFrame );
	
	THttpServer HttpServer( 8081, OnHttpRequest );

	std::Debug << "HTTP Listening on " << HttpServer.GetListeningPort() << std::endl;
	
	App.mConsoleApp.WaitForExit();

	return 0;
}




