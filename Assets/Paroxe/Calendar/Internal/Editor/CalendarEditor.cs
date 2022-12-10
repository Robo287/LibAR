using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Paroxe.SuperCalendar.Internal
{
    [CustomEditor(typeof(Paroxe.SuperCalendar.Calendar), true)]
    public class CalendarEditor : Editor
    {
        GUIStyle m_Background1;
        GUIStyle m_Background2;
        GUIStyle m_Background3;
        Texture2D m_Logo;
        Calendar calendar;
        SCParoxeBanner m_ParoxeBanner;

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(calendar, "Calendar");

            if (m_Logo != null)
            {
                Rect rect = GUILayoutUtility.GetRect(m_Logo.width, m_Logo.height);
                GUI.DrawTexture(rect, m_Logo, ScaleMode.ScaleToFit);
            }

            SerializedProperty m_BannerIsOpened;
            m_BannerIsOpened = serializedObject.FindProperty("m_BannerIsOpened");

            m_BannerIsOpened.boolValue = m_ParoxeBanner.DoOnGUI(m_BannerIsOpened.boolValue);

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Configuration", EditorStyles.boldLabel);

            calendar.SelectionMode =
                (Calendar.SelectionModeType) EditorGUILayout.EnumPopup("Selection Mode", calendar.SelectionMode);

			if (calendar.SelectionMode != Calendar.SelectionModeType.TimeOnly)
				calendar.StartPanel = (Calendar.StartPanelEnum)EditorGUILayout.EnumPopup("Start Panel", calendar.StartPanel);

			switch (calendar.SelectionMode)
            {
                case Calendar.SelectionModeType.DateOnly:
                    calendar.UseNowDateTime = EditorGUILayout.Toggle("Use Now Date", calendar.UseNowDateTime);
                    break;
                case Calendar.SelectionModeType.TimeOnly:
                    calendar.UseNowDateTime = EditorGUILayout.Toggle("Use Now Time", calendar.UseNowDateTime);
                    break;
                case Calendar.SelectionModeType.DateAndTime:
                    calendar.UseNowDateTime = EditorGUILayout.Toggle("Use Now DateTime", calendar.UseNowDateTime);
                    break;
            }

            if (!calendar.UseNowDateTime)
            {
                DateTime parsedDateTime = DateTime.MinValue;

                try
                {
                    CultureInfo usCulture = new CultureInfo("en-US");
                    switch (calendar.SelectionMode)
                    {
                        case Calendar.SelectionModeType.DateOnly:
                            parsedDateTime = DateTime.Parse(EditorGUILayout.TextField("Date (MM/DD/YYYY)", calendar.DateTime.ToString(usCulture.DateTimeFormat.ShortDatePattern)));
                            break;
                        case Calendar.SelectionModeType.TimeOnly:
                            parsedDateTime = DateTime.Parse(EditorGUILayout.TextField("Time (HH:MM:SS tt)", calendar.DateTime.ToString(usCulture.DateTimeFormat.LongTimePattern)));
                            break;
                        case Calendar.SelectionModeType.DateAndTime:
                            parsedDateTime = DateTime.Parse(EditorGUILayout.TextField("DateTime (MM/DD/YYYY HH:MM:SS tt)", calendar.DateTime.ToString(usCulture)));
                            break;
                    }
                }
                catch (Exception)
                {
                    ///...
                }

                if (parsedDateTime != DateTime.MinValue)
                    calendar.SerializableDateTime.DateTime = parsedDateTime;
            }

            calendar.KeepSelected = EditorGUILayout.Toggle("Keep DateTime Selected", calendar.KeepSelected);
            calendar.Opened = EditorGUILayout.Toggle("Opened", calendar.Opened);
            calendar.AnimatePanels = EditorGUILayout.Toggle("Animate Panels", calendar.AnimatePanels);
            calendar.AnimateOpen = EditorGUILayout.Toggle("Animate Open/Close", calendar.AnimateOpen);
            calendar.Font = EditorGUILayout.ObjectField("Font", calendar.Font, typeof(Font), true) as Font;

            GUILayout.Space(10.0f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Localization", EditorStyles.boldLabel);

            calendar.UseCurrentHostCulture = EditorGUILayout.Toggle("Use Host Culture", calendar.UseCurrentHostCulture);

            if (!calendar.UseCurrentHostCulture)
            {
                CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

                Array.Sort(cultureInfos, delegate (CultureInfo culture1, CultureInfo culture2) {
                    return String.Compare(culture1.Name, culture2.Name, StringComparison.Ordinal);
                });

                string[] culturesNames = new string[cultureInfos.Length];

                int selectedIndex = 0;

                for (int i = 0; i < cultureInfos.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(calendar.CultureInfoName) && calendar.CultureInfoName.Equals(cultureInfos[i].Name))
                        selectedIndex = i;
                    culturesNames[i] = cultureInfos[i].ToString();
                }

                selectedIndex = EditorGUILayout.Popup("Culture", selectedIndex, culturesNames);

                calendar.CultureInfoName = cultureInfos[selectedIndex].Name;
            }

            calendar.LocalizedHourTerm = EditorGUILayout.TextField("Hour Term", calendar.LocalizedHourTerm);
            calendar.LocalizedMinuteTerm = EditorGUILayout.TextField("Minute Term", calendar.LocalizedMinuteTerm);

            GUILayout.Space(10.0f);
            GUILayout.EndVertical();

            SerializedProperty m_OnDateTimeChangedProperty;
            m_OnDateTimeChangedProperty = serializedObject.FindProperty("m_OnDateTimeChanged");

            EditorGUILayout.PropertyField(m_OnDateTimeChangedProperty);
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(calendar);
        }

        

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.hideFlags = HideFlags.HideAndDontSave;
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        protected virtual void OnDisable()
        {
            DestroyImmediate(m_Background1.normal.background);
            DestroyImmediate(m_Background2.normal.background);
            DestroyImmediate(m_Background3.normal.background);
        }

        protected virtual void OnEnable()
        {
            calendar = (Paroxe.SuperCalendar.Calendar)target;

            m_Background1 = new GUIStyle();
            m_Background1.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
            m_Background2 = new GUIStyle();
            m_Background2.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.0f));
            m_Background3 = new GUIStyle();
            m_Background3.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.05f));

            MonoScript script = MonoScript.FromScriptableObject(this);
            string scriptPath = AssetDatabase.GetAssetPath(script);
            m_Logo =
                (Texture2D)
                    AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(scriptPath) + "/logo.png", typeof(Texture2D));

            if (m_ParoxeBanner == null)
                m_ParoxeBanner = new SCParoxeBanner(Path.GetDirectoryName(scriptPath));
        }

        private static bool IsPrefabGhost(Transform This)
        {
            var TempObject = new GameObject();
            try
            {
                TempObject.transform.parent = This.parent;

                var OriginalIndex = This.GetSiblingIndex();

                This.SetSiblingIndex(int.MaxValue);
                if (This.GetSiblingIndex() == 0)
                    return true;

                This.SetSiblingIndex(OriginalIndex);
                return false;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(TempObject);
            }
        }
    }
}