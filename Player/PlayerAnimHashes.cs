using UnityEngine;

public static class PlayerAnimHashes
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Move = Animator.StringToHash("Move");
    public static readonly int Receive = Animator.StringToHash("Receive");
    public static readonly int Serve = Animator.StringToHash("Serve");
    public static readonly int Toss = Animator.StringToHash("Toss");
    public static readonly int Spike_Start = Animator.StringToHash("Spike_Start");
    public static readonly int Spike_End = Animator.StringToHash("Spike_End");
}