﻿/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using UnityEditor;
using UnityEngine;

namespace Facebook.WitAi.Windows
{
    public abstract class WitScriptableWizard : ScriptableWizard
    {
        protected Vector2 scrollOffset;

        protected virtual Texture2D HeaderIcon => WitStyles.HeaderIcon;
        protected virtual string HeaderUrl => WitStyles.Texts.WitUrl;
        
        protected abstract GUIContent Title { get; }
        protected abstract string ButtonLabel { get; }
        protected abstract string ContentSubheaderLabel { get; }

        protected virtual void OnEnable()
        {
            WitAuthUtility.InitEditorTokens();
            createButtonName = ButtonLabel;
        }
        protected override bool DrawWizardGUI()
        {
            // Reapply title if needed
            if (titleContent != Title)
            {
                titleContent = Title;
            }
            
            // Layout window
            Vector2 size = Vector2.zero;
            WitEditorUI.LayoutWindow(titleContent.text, HeaderIcon, HeaderUrl, LayoutContent, ref scrollOffset, out size);
            
            // Clamp wizard sizes
            maxSize = minSize = size;
            
            // True if valid server token
            return false;
        }
        protected virtual void LayoutContent()
        {
            if (!string.IsNullOrEmpty(ContentSubheaderLabel))
            {
                WitEditorUI.LayoutSubheaderLabel(ContentSubheaderLabel);
                GUILayout.Space(WitStyles.HeaderPaddingBottom * 2f);
            }
            LayoutFields();
        }
        protected abstract void LayoutFields();
        protected virtual void OnWizardCreate()
        {
            
        }
    }
}
