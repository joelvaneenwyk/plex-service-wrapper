using PlexServiceCommon.Interface;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PlexServiceCommon
{
    public class TrayInteractionClient(object callbackInstance, Binding binding, EndpointAddress remoteAddress) 
        : DuplexClientBase<ITrayInteraction>(callbackInstance, binding, remoteAddress)
    {
    }
}
