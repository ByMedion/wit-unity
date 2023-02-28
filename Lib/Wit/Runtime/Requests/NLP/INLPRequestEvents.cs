/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine.Events;

namespace Meta.Voice
{
    /// <summary>
    /// Interface for NLP request events callbacks
    /// </summary>
    public interface INLPRequestEvents<TUnityEvent> : IVoiceRequestEvents<TUnityEvent>
        where TUnityEvent : UnityEventBase
    {
    }
}
