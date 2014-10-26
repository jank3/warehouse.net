﻿using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Practices.Prism.Events;
using Warehouse.Silverlight.SignalRModule.Events;

namespace Warehouse.Silverlight.SignalRModule
{
    public class SignalRClient
    {
        // remote consts
        private const string ProductsHub = "ProductsHub";
        private const string OnProductUpdated = "OnProductUpdated";
        private const string RaiseProductUpdated = "RaiseProductUpdated";

        private IHubProxy hubProxy;

        private readonly IEventAggregator eventAggregator;

        public SignalRClient(/*IEventAggregator eventAggregator*/)
        {
            //this.eventAggregator = eventAggregator;
            SubscribeLocal();
        }

        public async Task StartAsync()
        {
            var hubConnection = new HubConnection(System.Windows.Browser.HtmlPage.Document.DocumentUri.ToString());
            hubProxy = hubConnection.CreateHubProxy(ProductsHub);
            SubscribeRemote();
            await hubConnection.Start();
        }

        private void SubscribeLocal()
        {
            eventAggregator.GetEvent<ProductUpdatedEvent>().Subscribe(OnProductUpdatedLocal);
        }

        private void SubscribeRemote()
        {
            hubProxy.On<string>(OnProductUpdated, OnProductUpdatedRemote);
        }

        private void OnProductUpdatedLocal(ProductUpdatedEventArgs e)
        {
            if (e.FromRemote) return;

            // product updated locally
            // we need to notify other clients
            // await hubProxy.Invoke(RaiseProductUpdated, e.ProductId);
        }

        private void OnProductUpdatedRemote(string productId)
        {
            // product updated remotely
            // we need to notify local modules
            var e = new ProductUpdatedEventArgs(productId, true);
            eventAggregator.GetEvent<ProductUpdatedEvent>().Publish(e);
        }
    }
}
