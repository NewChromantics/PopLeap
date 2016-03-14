#pragma once

#include <SoyHttpServer.h>
#include <SoyJson.h>


class TUdpJsonServer : public TSocketServer
{
public:
	TUdpJsonServer(size_t ListenPort,std::function<void(const Json::TReadProtocol&,SoyRef,TUdpJsonServer&)> OnRequest);
	
	void			SendResponse(const Json::TWriteProtocol& Response,SoyRef Client);
	void			SendResponse(std::shared_ptr<Soy::TWriteProtocol> Response,SoyRef Client);
	
protected:
	virtual std::shared_ptr<TSocketReadThread>	CreateReadThread(std::shared_ptr<SoySocket> Socket,SoyRef ConnectionRef) override;
	virtual std::shared_ptr<TSocketWriteThread>	CreateWriteThread(std::shared_ptr<SoySocket> Socket,SoyRef ConnectionRef) override;
	virtual void								OnRecievedData(Soy::TReadProtocol& ReadData,SoyRef Connection) override;
	
public:
	std::function<void(const Json::TReadProtocol&,SoyRef,THttpServer&)>	mOnRequest;
};


