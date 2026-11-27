using System.Collections.Generic;

namespace DudeChaos.Net
{
    public sealed class LoopbackTransport : INetworkTransport
    {
        private readonly Queue<byte[]> _wireQueue = new Queue<byte[]>();
        private LoopbackTransport _peer;

        public bool IsConnected => _peer != null;

        public static (LoopbackTransport clientA, LoopbackTransport clientB) CreateLinkedPair()
        {
            var a = new LoopbackTransport();
            var b = new LoopbackTransport();
            a._peer = b;
            b._peer = a;
            return (a, b);
        }

        public void SendCommand(NetworkCommand command)
        {
            if (_peer == null) return;
            byte[] packet = new byte[NetworkCommand.ByteSize];
            command.WriteTo(packet, 0);
            _peer._wireQueue.Enqueue(packet);
        }

        public bool TryReceiveCommand(out NetworkCommand command)
        {
            if (_wireQueue.Count > 0)
            {
                byte[] packet = _wireQueue.Dequeue();
                command = NetworkCommand.ReadFrom(packet, 0);
                return true;
            }
            command = default;
            return false;
        }
    }
}
