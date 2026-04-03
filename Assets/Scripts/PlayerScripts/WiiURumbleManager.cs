using System;
using System.Collections;
using UnityEngine;
using WiiU = UnityEngine.WiiU;

public class WiiURumbleManager : MonoBehaviour 
{
    private bool _isRumbling = false;

    public void Rumble()
    {
        StartCoroutine(RumbleLoop());
        _isRumbling = true;
    }
    public void StopRumble()
    {
        WiiU.GamePad gpWii = WiiU.GamePad.access;
        WiiU.GamePadState state = gpWii.state;
        gpWii.StopMotor();
        _isRumbling = false;
    }

    private IEnumerator RumbleLoop()
    {
        // Build full-on pattern once
        byte[] fullOnPattern = new byte[15];
        int totalBits = 120;
        for (int b = 0; b < totalBits; b++)
        {
            int byteIndex = b / 8;
            int bitIndex = b % 8;
            fullOnPattern[byteIndex] |= (byte)(1 << bitIndex);
        }

        while (true)
        {
            if (_isRumbling)
            {
                try
                {
                    WiiU.GamePad gp = WiiU.GamePad.access;
                    WiiU.GamePadState state = gp.state;
                    if (state.gamePadErr == WiiU.GamePadError.None)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            gp.ControlMotor(fullOnPattern, totalBits);
                        }
                    }
                }
                catch (Exception)
                {
                    // access failed; ignore and try again later
                }
            }
            yield return new WaitForSeconds(5f);
        }
    }

}
