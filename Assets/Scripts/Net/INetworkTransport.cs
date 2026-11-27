namespace DudeChaos.Net
{
    public interface INetworkTransport
    {
        bool IsConnected { get; }
        void SendCommand(NetworkCommand command);
        bool TryReceiveCommand(out NetworkCommand command);
    }
}
