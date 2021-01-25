using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluxHelmTool
{
    public class CustomHttpHandler : DelegatingHandler
    {
        public CustomHttpHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Properties["WebAssemblyEnableStreamingResponse"] = true;

            Microsoft.AspNetCore.Components.WebAssembly.Http.WebAssemblyHttpRequestMessageExtensions.SetBrowserRequestMode(request, Microsoft.AspNetCore.Components.WebAssembly.Http.BrowserRequestMode.NoCors);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
