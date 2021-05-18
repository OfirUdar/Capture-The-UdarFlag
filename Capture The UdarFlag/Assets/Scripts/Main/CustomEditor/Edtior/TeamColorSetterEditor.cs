
using UnityEditor;


#if UNITY_EDITOR


[CustomEditor(typeof(TeamColorSetter))]
public class TeamColorSetterEditor : Editor
{
    TeamColorSetter teamColorSetter;

    private void OnEnable()
    {
        teamColorSetter = (TeamColorSetter)target;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        teamColorSetter.objectType = (TeamColorSetter.ObjectType)EditorGUILayout.EnumPopup("Object Type", teamColorSetter.objectType);
        teamColorSetter.rendererType = (TeamColorSetter.RendererType)EditorGUILayout.EnumPopup("Renderer Type", teamColorSetter.rendererType);
        DisplayObjectTypes();
        DisplayRendererTypes();

        serializedObject.ApplyModifiedProperties();
    }
    private void DisplayRendererTypes()
    {
        switch (teamColorSetter.rendererType)
        {
            case TeamColorSetter.RendererType.Renderer:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("renderers"));
                    break;
                }
            case TeamColorSetter.RendererType.Sprite:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteRenderers"));
                    break;
                }
            case TeamColorSetter.RendererType.Image:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("images"));
                    break;
                }
            case TeamColorSetter.RendererType.Text:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("texts"));
                    break;
                }
            case TeamColorSetter.RendererType.ParticleSystem:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("particleSystems"));
                    break;
                }
        }
    }

    private void DisplayObjectTypes()
    {
        switch (teamColorSetter.objectType)
        {
            case TeamColorSetter.ObjectType.Flag:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("flag"));
                    break;
                }
            case TeamColorSetter.ObjectType.Base:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("flagBase"));
                    break;
                }
            case TeamColorSetter.ObjectType.Player:
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("player"));
                    break;
                }

        }
    }
}
#endif


