using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventHanlder))]
public class EventHanlderEditor : Editor
{
    private EventHanlder script;
    public override void OnInspectorGUI()
    {
        script = (EventHanlder)target;

        script._actionTypeEventHanlder = (EventHanlder.ActionTypeEventHanlder)EditorGUILayout.EnumPopup("Action Type", script._actionTypeEventHanlder);

        AddAWarningBubble("Make sure you have a trigger collider.");

        switch (script._actionTypeEventHanlder)
        {
            case EventHanlder.ActionTypeEventHanlder.CamSwitch:
                ShowCamSwitchOptions();
                break;

            case EventHanlder.ActionTypeEventHanlder.KillPlayerInsta:
                ShowKillOptions();
                break;

            case EventHanlder.ActionTypeEventHanlder.LoadingAndUnloading:
                ShowLoadingAndUnloading();
                break;
            case EventHanlder.ActionTypeEventHanlder.LoadingAndUnloadingScenes:
                ShowLoadingAndUnloadingScenes();
                break;
            case EventHanlder.ActionTypeEventHanlder.Music:
                ShowMusicOptions();
                break;
        }

        if (GUI.changed)
            EditorUtility.SetDirty(script);
    }

    private void ShowCamSwitchOptions()
    {
        script.ChangeToNoFollow = EditorGUILayout.Toggle("Does player controll the camera?", script.ChangeToNoFollow);
        AddATextBubble("If the camera looks towards something, the player will not be able to control the camera.");
        if (script.ChangeToNoFollow == false)
        {
            script.CameraLooksTowardsSth = EditorGUILayout.Toggle("Does the camera look towards something?", script.CameraLooksTowardsSth);
            if (script.CameraLooksTowardsSth == true)
                script.LookTowardsTransfom = (Transform)EditorGUILayout.ObjectField("Object that the camera looks towards.", script.LookTowardsTransfom, typeof(Transform), true);
        }
    }
    private void ShowKillOptions()
    {
        AddATextBubble("This will kill the player instantly.");
    }
    private void ShowLoadingAndUnloading()
    {
        AddATextBubble("This will load or unload the objects you assign in the next field. You can assign any gameobject.");
        script.mode = (EventHanlder.Mode)EditorGUILayout.EnumPopup("Action Type", script.mode);
        //Multi-object editing for arrays is a bit hard, so we use SerializedProperty to handle it..
        serializedObject.Update();

        SerializedProperty objectsProp = serializedObject.FindProperty("objects");
        EditorGUILayout.PropertyField(objectsProp, new GUIContent("Objects"), true);

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowLoadingAndUnloadingScenes()
    {
        AddATextBubble("This will load or unload the scenes you assign in the next field. You have to write the name of the scene exactly as it is in the build settings. Make sure to add the scene to the build settings.");

        script.Scenemanager = (Scenemanager)EditorGUILayout.ObjectField("SceneManager to do the scene switching.",script.Scenemanager,typeof(Scenemanager),true );
        script.modeScene = (EventHanlder.ModeScene)EditorGUILayout.EnumPopup("Mode", script.modeScene);

        serializedObject.Update();

        SerializedProperty stringsProp = serializedObject.FindProperty("LevelNameStrings");
        EditorGUILayout.PropertyField(stringsProp, new GUIContent("Scenes"), true);

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowMusicOptions()
    {
        script.Scenemanager = (Scenemanager)EditorGUILayout.ObjectField("SceneManager to do the scene switching.", script.Scenemanager, typeof(Scenemanager), true);

        AddATextBubble("This will play the music you assign in the next field. You have to write the name of the music exactly as it is in the MusicManager script.");
        script.modeMusic = (EventHanlder.ModeMusic)EditorGUILayout.EnumPopup("Mode", script.modeMusic);
        if (script.modeMusic == EventHanlder.ModeMusic.Play)
        {
            script.MusicName = EditorGUILayout.TextField("Music Name", script.MusicName);
        }
        script.OneTimeUseM = EditorGUILayout.Toggle("Is it used once?", script.OneTimeUseM);

    }

    #region Text
    private void AddATextBubble(string text)
    {
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(text, MessageType.Info);

        EditorGUILayout.Space();
    }

    private void AddAWarningBubble(string text)
    {
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(text, MessageType.Warning);

        EditorGUILayout.Space();
    }

    private void AddAErrorBubble(string text)
    {
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(text, MessageType.Error);

        EditorGUILayout.Space();
    }

    #endregion
}
