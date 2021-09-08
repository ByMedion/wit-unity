/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using com.facebook.witai.lib;
using UnityEngine;

namespace com.facebook.witai.data
{
    [Serializable]
    public class WitTraitValue
    {
        [SerializeField] public string id;
        [SerializeField] public string value;

        #if UNITY_EDITOR
        public static WitTraitValue FromJson(WitResponseNode traitValueNode)
        {
            return new WitTraitValue()
            {
                id = traitValueNode["id"],
                value = traitValueNode["value"]
            };
        }
        #endif
    }
}
