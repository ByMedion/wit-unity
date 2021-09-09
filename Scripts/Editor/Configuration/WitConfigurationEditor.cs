﻿/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using System.Linq;
using com.facebook.witai;
using com.facebook.witai.data;
using com.facebook.witai.utility;
using UnityEditor;
using UnityEngine;

#if !WIT_DISABLE_UI
[CustomEditor(typeof(WitConfiguration), true)]
#endif
public class WitConfigurationEditor : Editor
{
    private WitConfiguration configuration;

    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    private int selectedToolPanel;

    public string[] toolPanelNames = new[]
    {
        "Application",
        "Intents",
        "Entities",
        "Traits"
    };

    public string[] toolPanelNamesOnlyAppInfo = new[]
    {
        "Application"
    };

    private readonly string[] toolPanelNamesWithoutAppInfo = new[]
    {
        "Intents",
        "Entities",
        "Traits"
    };

    private const int TOOL_PANEL_APP = 0;
    private const int TOOL_PANEL_INTENTS = 1;
    private const int TOOL_PANEL_ENTITIES = 2;
    private const int TOOL_PANEL_TRAITS = 3;

    private Editor applicationEditor;
    private Vector2 scroll;
    private bool appConfigurationFoldout;
    private bool initialized = false;
    public bool drawHeader = true;

    private bool IsTokenValid => !string.IsNullOrEmpty(configuration.clientAccessToken) &&
                                 configuration.clientAccessToken.Length == 32;

    private void Initialize()
    {
        WitAuthUtility.InitEditorTokens();
        configuration = target as WitConfiguration;
        if (!string.IsNullOrEmpty(configuration?.clientAccessToken))
        {
            configuration?.UpdateData(() =>
            {
                EditorForegroundRunner.Run(() => EditorUtility.SetDirty(configuration));
            });
        }
    }

    public override void OnInspectorGUI()
    {
        configuration = target as WitConfiguration;
        if (!initialized)
        {
            Initialize();
            initialized = true;
        }

        if (drawHeader)
        {
            string link = null;
            if (configuration && null != configuration.application &&
                !string.IsNullOrEmpty(configuration.application.id))
            {
                link = $"https://wit.ai/apps/{configuration.application.id}/settings";
            }
            BaseWitWindow.DrawHeader(headerLink: link);
        }

        GUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.BeginHorizontal();
        appConfigurationFoldout = EditorGUILayout.Foldout(appConfigurationFoldout,
            "Application Configuration");
        if (!string.IsNullOrEmpty(configuration?.application?.name))
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(configuration?.application?.name);
        }

        GUILayout.EndHorizontal();

        if (appConfigurationFoldout || !IsTokenValid)
        {
            GUILayout.BeginHorizontal();
            var token = EditorGUILayout.PasswordField("Server Access Token",
                WitAuthUtility.AppServerToken);
            if (token != WitAuthUtility.AppServerToken)
            {
                WitAuthUtility.AppServerToken = token;

                if (token.Length == 32)
                {
                    configuration.UpdateData();
                }

                EditorUtility.SetDirty(configuration);
            }

            GUILayout.EndHorizontal();

            if(configuration && GUILayout.Button("Refresh Application Configuration"))
            {
                configuration.FetchAppConfigFromServerToken(() =>
                {
                    EditorForegroundRunner.Run(Repaint);
                    appConfigurationFoldout = false;
                });
            }
        }

        GUILayout.EndVertical();

        bool hasApplicationInfo = configuration && null != configuration.application;

        if (hasApplicationInfo)
        {
            if(!string.IsNullOrEmpty(WitAuthUtility.AppServerToken) && WitAuthUtility.AppServerToken.Length == 32){
                selectedToolPanel = GUILayout.Toolbar(selectedToolPanel, toolPanelNames);    
            }else{
                selectedToolPanel = GUILayout.Toolbar(selectedToolPanel, toolPanelNamesOnlyAppInfo);    
            }
        }
        else
        {
            selectedToolPanel = GUILayout.Toolbar(selectedToolPanel, toolPanelNamesWithoutAppInfo);
        }

        scroll = GUILayout.BeginScrollView(scroll);
        switch (hasApplicationInfo ? selectedToolPanel : selectedToolPanel + 1)
        {
            case TOOL_PANEL_APP:
                DrawApplication(configuration.application);
                break;
            case TOOL_PANEL_INTENTS:
                DrawIntents();
                break;
            case TOOL_PANEL_ENTITIES:
                DrawEntities();
                break;
            case TOOL_PANEL_TRAITS:
                DrawTraits();
                break;
        }
        GUILayout.EndScrollView();

        if (GUILayout.Button("Open Wit.ai"))
        {
            if (!string.IsNullOrEmpty(configuration.application?.id))
            {
                Application.OpenURL($"https://wit.ai/apps/{configuration.application.id}");
            }
            else
            {
                Application.OpenURL("https://wit.ai");
            }
        }
    }

    private void DrawTraits()
    {
        var traits = configuration.traits;
        if (null != traits)
        {
            var n = traits.Length;
            if (n == 0)
            {
                GUILayout.Label("No traits available.");
            }
            else
            {
                BeginIndent();
                for (int i = 0; i < n; i++) {
                    var trait = traits[i];
                    if (null != trait && Foldout("t:", trait.name))
                    {
                        DrawTrait(trait);
                    }
                }
                EndIndent();
            }
        }
        else
        {
            GUILayout.Label("Traits have not been loaded yet.", EditorStyles.helpBox);
        }
    }

    private void DrawTrait(WitTrait trait)
    {
        InfoField("ID", trait.id);
        InfoField("Name", trait.name);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Values", GUILayout.Width(100));
        GUILayout.EndHorizontal();
        var values = trait.values;
        var n = values.Length;
        if (n == 0) {
            GUILayout.Label("No values available.");
        }
        else
        {
            BeginIndent();
            for (int i = 0; i < n; i++)
            {
                var value = values[i];
                if (null != value && Foldout("v:", value.value))
                {
                    DrawTraitValue(value);
                }
            }
            EndIndent();
        }
    }

    private void DrawTraitValue(WitTraitValue traitValue)
    {
        InfoField("ID", traitValue.id);
        InfoField("Value", traitValue.value);
    }

    private void DrawEntities()
    {
        var entities = configuration.entities;
        if (null != entities)
        {
            var n = entities.Length;
            if (n == 0)
            {
                GUILayout.Label("No entities available.");
            }
            else
            {
                BeginIndent();
                for (int i = 0; i < n; i++)
                {
                    var entity = entities[i];
                    if (null != entity && Foldout("e:", entity.name))
                    {
                        DrawEntity(entity);
                    }
                }
                EndIndent();
            }
        }
        else
        {
            GUILayout.Label("Entities have not been loaded yet.", EditorStyles.helpBox);
        }
    }

    private void DrawEntity(WitEntity entity)
    {
        InfoField("ID", entity.id);
        if (null != entity.roles && entity.roles.Length > 0)
        {
            EditorGUILayout.Popup("Roles", 0, entity.roles);
        }

        if (null != entity.lookups && entity.lookups.Length > 0)
        {
            EditorGUILayout.Popup("Lookups", 0, entity.lookups);
        }
    }

    private void DrawIntents()
    {
        var intents = configuration.intents;
        if (null != intents)
        {
            var n = intents.Length;
            if (n == 0)
            {
                GUILayout.Label("No intents available.");
            }
            else
            {
                BeginIndent();
                for (int i = 0; i < n; i++)
                {
                    var intent = intents[i];
                    if (null != intent && Foldout("i:", intent.name))
                    {
                        DrawIntent(intent);
                    }
                }

                EndIndent();
            }
        }
        else
        {
            GUILayout.Label("Intents have not been loaded yet.", EditorStyles.helpBox);
        }
    }

    private void DrawIntent(WitIntent intent)
    {
        InfoField("ID", intent.id);
        var entities = intent.entities;
        if (entities.Length > 0)
        {
            var entityNames = entities.Select(e => e.name).ToArray();
            EditorGUILayout.Popup("Entities", 0, entityNames);
        }
    }

    private void DrawApplication(WitApplication application)
    {
        if (string.IsNullOrEmpty(application.name))
        {
            GUILayout.Label("Loading...");
        }
        else
        {
            InfoField("Name", application.name);
            InfoField("ID", application.id);
            InfoField("Language", application.lang);
            InfoField("Created", application.createdAt);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Private", GUILayout.Width(100));
            GUILayout.Toggle(application.isPrivate, "");
            GUILayout.EndHorizontal();
        }
    }

    #region UI Components

    private void BeginIndent()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
    }

    private void EndIndent()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void InfoField(string name, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(name, GUILayout.Width(100));
        GUILayout.Label(value, "TextField");
        GUILayout.EndHorizontal();
    }

    private bool Foldout(string keybase, string name)
    {
        string key = keybase + name;
        bool show = false;
        if (!foldouts.TryGetValue(key, out show))
        {
            foldouts[key] = false;
        }

        show = EditorGUILayout.Foldout(show, name, true);
        foldouts[key] = show;
        return show;
    }

    #endregion
}
