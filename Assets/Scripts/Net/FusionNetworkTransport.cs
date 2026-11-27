namespace DudeChaos.Net
{
    /// <summary>
    /// Optional Photon Fusion transport adapter scaffold.
    /// IMPORTANT STATUS (§1.8 & P10): Guarded behind #if PHOTON_FUSION; this class has NEVER been compiled
    /// against the real Photon Fusion package and is NOT shipped runtime netcode.
    /// </summary>
#if PHOTON_FUSION
    public sealed class FusionNetworkTransport : INetworkTransport
    {
        public bool IsConnected => false;
        public void SendCommand(NetworkCommand command) { }
        public bool TryReceiveCommand(out NetworkCommand command)
        {
            command = default;
            return false;
        }
    }
#else
    public static class FusionNetworkTransportStatus
    {
        public const string Status = "NOT COMPILED AGAINST PHOTON FUSION PACKAGE (scaffold only)";
    }
#endif
}
