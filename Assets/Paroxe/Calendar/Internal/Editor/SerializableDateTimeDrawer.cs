using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Paroxe.SuperCalendar
{
    [CustomPropertyDrawer(typeof(Paroxe.SuperCalendar.SerializableDateTime), true)]
    public class SerializableDateTimeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rectPos = position;
            rectPos.width -= 200;
            rectPos.x = rectPos.width + 20;
            rectPos.width = position.width - rectPos.width - 10;
        }
    }
}
