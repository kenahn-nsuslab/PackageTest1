using System.Collections.Generic;
using System.Text;
using Common.Scripts.Define;
using Common.Scripts.Localization;
using Common.Scripts.Managers;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Bebop.ImageFillEnd))]
public class ImageFillEndEditor : UnityEditor.Editor
{
    private Bebop.ImageFillEnd fillEnd;

    private void OnEnable()
    {
        fillEnd = target as Bebop.ImageFillEnd;

        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        fillEnd = null;
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (fillEnd == null)
            return;

        fillEnd.UpdateFillEnd(fillEnd.targetFrame.fillAmount);
    }
}
