using UnityEngine;

namespace QuickFlick.Input
{
    public interface IControllerInput
    {
        Vector2 LeftStick { get; }   // each axis in [-1, 1]
        Vector2 RightStick { get; }  // each axis in [-1, 1]
        bool LeftFire { get; }       // held state
        bool RightFire { get; }      // held state
    }
}
