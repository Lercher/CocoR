using Fleck;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsEditor
{
    public class Websocket {
        private IWebSocketConnection socket = null;

        public Websocket() {
            var url = "ws://0.0.0.0:8181";
            var server = new WebSocketServer(url);
            server.Start(socket =>
            {
                this.socket = socket;
                socket.OnOpen = () => Console.WriteLine("Listening on {0} ...", url);
                socket.OnClose = () => Console.WriteLine("Closed {0}", url);
                //socket.OnMessage = message => socket.Send(message);
            });
        }

        public void send(string json) {
            if (socket != null) {
                socket.Send(json);
                System.Console.WriteLine("sent {0:n0} bytes of JSON", json.Length);
            } else
                System.Console.WriteLine("socket is null");
        }
    }
}