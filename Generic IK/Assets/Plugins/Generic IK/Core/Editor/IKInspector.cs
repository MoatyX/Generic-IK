using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Generics.Dynamics
{
    /// <summary>
    /// Inspector for the IK component
    /// </summary>
    [CustomEditor(typeof(InverseKinematics))]
    public class IKInspector : Editor
    {
        private InverseKinematics system { get { return target as InverseKinematics; } }
        private RigReader.RigType rigType;

        private void OnEnable()
        {
            system.animator = system.GetComponent<Animator>();
            if (!system.animator) system.animator = system.GetComponentInChildren<Animator>();

            if (system.animator)
            {
                system.DetectRig();
                rigType = system.rigReader.rigType;
            }
            else
            {
                rigType = RigReader.RigType.Generic;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("IK Configuration for a : " + rigType.ToString() + " Rig", MessageType.Info, true);
            SerializedProperty solver = serializedObject.FindProperty("solver");

            EditorGUILayout.PropertyField(solver);
            EditorGUILayout.Separator();

            switch (rigType)
            {
                case RigReader.RigType.Humanoid:
                    DrawHumanoidRig();
                    break;
                case RigReader.RigType.Generic:
                    DrawGenericRig();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(system);
        }

        private void OnSceneGUI()
        {
            if (rigType == RigReader.RigType.Humanoid)
            {
                DrawHumanoidScene(system.rArm);
                DrawHumanoidScene(system.lArm);
                DrawHumanoidScene(system.rLeg);
                DrawHumanoidScene(system.lLeg);
            }

            DrawGenericChainsScene();
            DrawDynamicChainsScene();
        }

        private void DrawHumanoidRig()
        {
            SerializedProperty rArm = serializedObject.FindProperty("rArm");
            SerializedProperty lArm = serializedObject.FindProperty("lArm");
            SerializedProperty rLeg = serializedObject.FindProperty("rLeg");
            SerializedProperty lLeg = serializedObject.FindProperty("lLeg");
            SerializedProperty otherChains = serializedObject.FindProperty("otherChains");
            SerializedProperty otherKChains = serializedObject.FindProperty("otherKChains");


            EditorGUILayout.LabelField(new GUIContent("Humanoid Chains"), EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(rArm, new GUIContent("Right Arm"), true);
            EditorGUILayout.PropertyField(lArm, new GUIContent("Left Arm"), true);
            EditorGUILayout.PropertyField(rLeg, new GUIContent("Right Leg"), true);
            EditorGUILayout.PropertyField(lLeg, new GUIContent("Left Leg"), true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(otherChains, new GUIContent("Other Chains"), true);
            EditorGUILayout.PropertyField(otherKChains, new GUIContent("Dynamic Chains"), true);

            if (GUILayout.Button("Initialise Rig"))
            {
                system.BuildRig();
            }

        }

        private void DrawGenericRig()
        {
            SerializedProperty otherChains = serializedObject.FindProperty("otherChains");
            SerializedProperty otherKChains = serializedObject.FindProperty("otherKChains");

            EditorGUILayout.PropertyField(otherChains, new GUIContent("Generic Chains"), true);
            EditorGUILayout.PropertyField(otherKChains, new GUIContent("Dynamic Chains"), true);
        }

        private void DrawHumanoidScene(Core.Chain chain)
        {
            if (chain == null) return;
            if (chain.Joints.Count <= 0) return;

            Color yellow = Color.yellow;
            Color blue = Color.blue;
            Color green = Color.green;
            Color red = Color.red;
            Dictionary<int, Color> colorMap = new Dictionary<int, Color>();

            Action<Vector3> drawSphere = v => Handles.SphereHandleCap(0, v, Quaternion.identity, 0.04f, EventType.Repaint);
            Action<Vector3> drawCube = v => Handles.CubeHandleCap(0, v, Quaternion.identity, 0.03f, EventType.Repaint);
            Action<int> alternateColor = i => Handles.color = colorMap[(int)Mathf.Pow(-1, i)];

            colorMap.Add(1, yellow);
            colorMap.Add(-1, green);

            for (int i = 0; i < chain.Joints.Count - 1; i++)
            {
                if (!chain.Joints[i].joint) break;
                alternateColor(i);
                Handles.DrawLine(chain.Joints[i].joint.position, chain.Joints[i + 1].joint.position);
                alternateColor(i + 1);
                drawSphere(chain.Joints[i].joint.position);
            }

            Func<float, Color> lerpColor = x => Color.Lerp(green, red, x);
            Action<Core.Chain> toTarget = x => Handles.DrawLine(x.GetEndEffector().position, x.Target && x.Weight > 0 ? x.Target.position : x.GetEndEffector().position);

            Handles.color = lerpColor(chain.Weight);
            toTarget(chain);
            drawCube(chain.GetEndEffector().position);
            drawCube(chain.Target ? chain.Target.position : chain.GetEndEffector().position);
        }

        /// <summary>
        /// visualise the bones and joints for generic rigs
        /// </summary>
        private void DrawGenericChainsScene()
        {
            if (system.otherChains == null) return;
            if (system.otherChains.Length <= 0) return;

            Color yellow = Color.yellow;
            Color blue = Color.blue;
            Color green = Color.green;
            Color red = Color.red;
            Dictionary<int, Color> colorMap = new Dictionary<int, Color>();

            Action<Vector3> drawCube = v => Handles.CubeHandleCap(0, v, Quaternion.identity, 0.03f, EventType.Repaint);
            Action<int> alternateColor = i => Handles.color = colorMap[(int)Mathf.Pow(-1, i)];

            Func<float, Color> lerpColor = x => Color.Lerp(green, red, x);
            Action<Core.Chain> toTarget = x => Handles.DrawLine(x.GetEndEffector().position, x.Target && x.Weight > 0 ? x.Target.position : x.GetEndEffector().position);

            colorMap.Add(1, yellow);
            colorMap.Add(-1, green);

            for (int i = 0; i < system.otherChains.Length; i++)
            {
                for (int j = 0; j < system.otherChains[i].Joints.Count - 1; j++)
                {
                    if (!system.otherChains[i].Joints[j].joint) return;
                    if (!system.otherChains[i].Joints[j + 1].joint) return;
                    alternateColor(j);

                    Vector3 p0 = system.otherChains[i].Joints[j].joint.position;
                    Vector3 p1 = system.otherChains[i].Joints[j + 1].joint.position;
                    Handles.DrawLine(p0, p1);

                    alternateColor(j + 1);
                }

                if (!system.otherChains[i].GetEndEffector()) return;

                Handles.color = lerpColor(system.otherChains[i].Weight);
                toTarget(system.otherChains[i]);
                drawCube(system.otherChains[i].GetEndEffector().position);
                drawCube(system.otherChains[i].Target ? system.otherChains[i].Target.position : system.otherChains[i].GetEndEffector().position);
            }
        }

        private void DrawDynamicChainsScene()
        {
            if (system.otherKChains == null) return;
            if (system.otherKChains.Length <= 0) return;

            Color yellow = Color.yellow;
            Color blue = Color.blue;
            Color green = Color.green;
            Color red = Color.red;
            Dictionary<int, Color> colorMap = new Dictionary<int, Color>();

            Action<int> alternateColor = i => Handles.color = colorMap[(int)Mathf.Pow(-1, i)];

            colorMap.Add(1, yellow);
            colorMap.Add(-1, green);

            for (int i = 0; i < system.otherKChains.Length; i++)
            {
                for (int j = 0; j < system.otherKChains[i].joints.Count - 1; j++)
                {
                    if (!system.otherKChains[i].joints[j].joint) return;
                    if (!system.otherKChains[i].joints[j + 1].joint) return;
                    alternateColor(j);

                    Vector3 p0 = system.otherKChains[i].joints[j].joint.position;
                    Vector3 p1 = system.otherKChains[i].joints[j + 1].joint.position;
                    Handles.DrawLine(p0, p1);

                    alternateColor(j + 1);
                }
            }
        }
    }
}
