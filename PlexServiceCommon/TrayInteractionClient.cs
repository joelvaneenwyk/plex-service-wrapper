using System.Diagnostics.CodeAnalysis;
using PlexServiceCommon.Interface;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PlexServiceCommon
{
    public class TrayInteractionClient : DuplexClientBase<ITrayInteraction>
    {
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public TrayInteractionClient(TrayCallback callbackInstance, Binding binding, EndpointAddress remoteAddress)
            : base((object)callbackInstance as InstanceContext, binding, remoteAddress) {
        }
    }
}
