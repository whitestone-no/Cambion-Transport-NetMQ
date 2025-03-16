﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Whitestone.Cambion.Transport.NetMQ
{
    internal class MessageHost
    {
        private XPublisherSocket _toSocket;
        private XSubscriberSocket _fromSocket;
        private Proxy _proxy;

        private readonly string _toAddress;
        private readonly string _fromAddress;

        public MessageHost(string toAddress, string fromAddress)
        {
            _toAddress = toAddress;
            _fromAddress = fromAddress;
        }

        public void Start()
        {
            var mre = new ManualResetEvent(false);

            // NetMQ.Bind to Publish and Subscribe addresses
            Task.Factory.StartNew(() =>
            {
                using (_toSocket = new XPublisherSocket())
                using (_fromSocket = new XSubscriberSocket())
                {
                    _toSocket.Bind(_toAddress);
                    _fromSocket.Bind(_fromAddress);

                    _proxy = new Proxy(_fromSocket, _toSocket);

                    mre.Set();

                    // This method is blocking, so important to set the ManualResetEvent before this.
                    _proxy.Start();
                }
            }, TaskCreationOptions.LongRunning);

            // Wait until the message host is actually started before returning
            mre.WaitOne(-1);
        }

        public void Stop()
        {
            _proxy.Stop();
            try
            {
                _fromSocket.Unbind(_fromAddress);
            }
            catch (ObjectDisposedException)
            {
                // Ignored
            }
            _fromSocket.Close();
            _fromSocket.Dispose();

            try
            {
                _toSocket.Unbind(_toAddress);
            }
            catch (ObjectDisposedException)
            {
                // Ignored
            }
            _toSocket.Close();
            _toSocket.Dispose();
        }
    }
}
