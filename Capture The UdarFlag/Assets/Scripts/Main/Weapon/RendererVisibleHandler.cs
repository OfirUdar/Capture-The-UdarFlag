using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererVisibleHandler : MonoBehaviour
{
    public event Action <bool>OnVisibleChanged;

    private void OnBecameVisible()
    {
        OnVisibleChanged?.Invoke(true);
    }
    private void OnBecameInvisible()
    {
        OnVisibleChanged?.Invoke(false);
    }
}
