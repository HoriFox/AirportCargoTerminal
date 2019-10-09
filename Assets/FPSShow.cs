using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSShow : MonoBehaviour
{
    // FPS
    public float deltaMeasurement = 1f;
    private float pastTense;
    private int m_frameCount;
    private float m_fps;

    void Update()
    {
        // FPS
        ++m_frameCount;
        pastTense += Time.deltaTime;
        if (pastTense <= deltaMeasurement)
            return;
        m_fps = (float)m_frameCount / pastTense;
        pastTense = 0.0f;
        m_frameCount = 0;
    }

    private void OnGUI()
    {
        string text = string.Format("{0:0.} fps", m_fps);
        GUILayout.Label(text);
    }
}
