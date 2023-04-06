using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractive : MonoBehaviour
{
    private bool _isAnimating;
    private WaitForSeconds _pause = new WaitForSeconds(Settings.ReapAnimDuration);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isAnimating)
        {
            if (other.transform.position.x < transform.position.x)
            {
                StartCoroutine(RotateRight());
            }
            else
            {
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isAnimating)
        {
            if (other.transform.position.x < transform.position.x)
            {
                StartCoroutine(RotateRight());
            }
            else
            {
                StartCoroutine(RotateLeft());
            }
            EventHandler.CallPlaySoundEvent(SoundName.Rustle);
        }
    }

    private IEnumerator RotateLeft()
    {
        _isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0,0,2);
            yield return _pause; 
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0,0,-2);
            yield return _pause;
        }
        transform.GetChild(0).Rotate(0,0,2);
        yield return _pause;
        
        _isAnimating = false;
    }
    
    private IEnumerator RotateRight()
    {
        _isAnimating = true;

        for (int i = 0; i < 4; i++)
        {
            transform.GetChild(0).Rotate(0,0,-2);
            yield return _pause; 
        }

        for (int i = 0; i < 5; i++)
        {
            transform.GetChild(0).Rotate(0,0,2);
            yield return _pause;
        }
        transform.GetChild(0).Rotate(0,0,-2);
        yield return _pause;
        
        _isAnimating = false;
    }
}
