namespace DudeChaos.Player
{
    // Exact 14 states (§1.8 & P1).
    public enum PlayerState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Sprint = 3,
        Crouch = 4,
        Jump = 5,
        Fall = 6,
        Land = 7,
        EnteringVehicle = 8,
        InVehicle = 9,
        ExitingVehicle = 10,
        Ragdoll = 11,
        GettingUp = 12,
        Dead = 13
    }
}
