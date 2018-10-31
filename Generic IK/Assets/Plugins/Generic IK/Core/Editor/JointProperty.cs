using UnityEditor;
using UnityEngine;

namespace Generics.Dynamics
{
    [CustomPropertyDrawer(typeof(Core.Joint))]
    public class JointProperty : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            SerializedProperty jointTrans = property.FindPropertyRelative("joint");
            SerializedProperty freedomProp = property.FindPropertyRelative("motionFreedom");
            SerializedProperty weight = property.FindPropertyRelative("weight");
            SerializedProperty axis = property.FindPropertyRelative("axis");
            SerializedProperty maxAngle = property.FindPropertyRelative("maxAngle");
            SerializedProperty maxTwist = property.FindPropertyRelative("maxTwist");
            SerializedProperty hingLimit = property.FindPropertyRelative("hingLimit");

            position.height = base.GetPropertyHeight(property, label);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

            if (property.isExpanded)
            {
                position.height = GetPropertyHeight(property, label);
                EditorGUI.BeginProperty(position, label, property);

                //Joints Transform field
                position.y += base.GetPropertyHeight(jointTrans, new GUIContent(jointTrans.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                position.height = base.GetPropertyHeight(jointTrans, new GUIContent(jointTrans.displayName));
                EditorGUI.PropertyField(position, jointTrans);


                //Weight field
                position.y += base.GetPropertyHeight(weight, new GUIContent(weight.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                position.height = base.GetPropertyHeight(weight, new GUIContent(weight.displayName));
                EditorGUI.PropertyField(position, weight);

                //MotionLimit enum
                position.y += base.GetPropertyHeight(freedomProp, new GUIContent(freedomProp.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                position.height = base.GetPropertyHeight(freedomProp, new GUIContent(freedomProp.displayName));
                EditorGUI.PropertyField(position, freedomProp);

                switch (freedomProp.enumValueIndex)
                {
                    case (int)Core.Joint.MotionLimit.Full:
                        break;
                    case (int)Core.Joint.MotionLimit.FullRestricted:
                        //Axis field
                        position.y += base.GetPropertyHeight(axis, new GUIContent(axis.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                        position.height = base.GetPropertyHeight(axis, new GUIContent(axis.displayName));
                        EditorGUI.PropertyField(position, axis);

                        //Swing field
                        position.y += base.GetPropertyHeight(maxAngle, new GUIContent(maxAngle.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                        position.height = base.GetPropertyHeight(maxAngle, new GUIContent(maxAngle.displayName));
                        EditorGUI.PropertyField(position, maxAngle);

                        //twist field
                        position.y += base.GetPropertyHeight(maxTwist, new GUIContent(maxTwist.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                        position.height = base.GetPropertyHeight(maxTwist, new GUIContent(maxTwist.displayName));
                        EditorGUI.PropertyField(position, maxTwist);
                        break;
                    case (int)Core.Joint.MotionLimit.SingleDegree:
                        //Axis field
                        position.y += base.GetPropertyHeight(axis, new GUIContent(axis.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                        position.height = base.GetPropertyHeight(axis, new GUIContent(axis.displayName));
                        EditorGUI.PropertyField(position, axis);

                        //Swing field
                        position.y += base.GetPropertyHeight(maxAngle, new GUIContent(maxAngle.displayName)) + EditorGUIUtility.standardVerticalSpacing;
                        position.height = base.GetPropertyHeight(maxAngle, new GUIContent(maxAngle.displayName));
                        EditorGUI.PropertyField(position, hingLimit);
                        break;
                }

                EditorGUI.EndProperty();
                //EditorGUILayout.Space();
            }
            //base.OnGUI(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty freedomProp = property.FindPropertyRelative("motionFreedom");
            float h = base.GetPropertyHeight(property, label);
            int i = 1;
            if (property.isExpanded)
            {
                i += 4;

                switch (freedomProp.enumValueIndex)
                {
                    case (int)Core.Joint.MotionLimit.Full:

                        break;
                    case (int)Core.Joint.MotionLimit.FullRestricted:
                        i += 3;
                        break;
                    case (int)Core.Joint.MotionLimit.SingleDegree:
                        i += 2;
                        break;
                }
            }
            return h * i;
        }
    }
}
