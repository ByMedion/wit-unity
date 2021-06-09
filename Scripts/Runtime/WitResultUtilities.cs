﻿/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using com.facebook.witai.lib;

namespace com.facebook.witai
{
    public static class WitResultUtilities
    {
        public static string GetFirstEntityValue(this WitResponseNode witResponse, string name)
        {
            return witResponse?["entities"]?[name]?[0]?["value"]?.Value;
        }

        public static WitResponseNode GetFirstEntity(this WitResponseNode witResponse, string name)
        {
            return witResponse?["entities"]?[name][0];
        }

        public static string GetIntentName(this WitResponseNode witResponse)
        {
            return witResponse?["intents"]?[0]?["name"]?.Value;
        }

        public static WitResponseNode GetFirstIntent(this WitResponseNode witResponse)
        {
            return witResponse?["intents"]?[0];
        }

        public static string GetPathValue(this WitResponseNode response, string path)
        {

            string[] nodes = path.Trim('.').Split('.');

            var node = response;

            foreach (var nodeName in nodes)
            {
                string[] arrayElements = nodeName.Split('[');

                node = node[arrayElements[0]];
                for (int i = 1; i < arrayElements.Length; i++)
                {
                    node = node[int.Parse(arrayElements[i])];
                }
            }

            return node.Value;
        }

        public static WitResponseReference GetWitResponseReference(string path)
        {

            string[] nodes = path.Trim('.').Split('.');

            var rootNode = new WitResponseReference();
            var node = rootNode;

            foreach (var nodeName in nodes)
            {
                string[] arrayElements = nodeName.Split('[');

                var childObject = new ObjectNodeReference();
                childObject.key = arrayElements[0];
                node.child = childObject;
                node = childObject;
                for (int i = 1; i < arrayElements.Length; i++)
                {
                    var childIndex = new ArrayNodeReference();
                    childIndex.index = int.Parse(arrayElements[i]);
                    node.child = childIndex;
                    node = childIndex;
                }
            }

            return rootNode;
        }

        public static string GetCodeFromPath(string path)
        {
            string[] nodes = path.Trim('.').Split('.');
            string code = "witResponse";
            foreach (var nodeName in nodes)
            {
                string[] arrayElements = nodeName.Split('[');

                code += $"[\"{arrayElements[0]}\"]";
                for (int i = 1; i < arrayElements.Length; i++)
                {
                    code += $"[{arrayElements[i]}]";
                }
            }

            code += ".Value";
            return code;
        }
    }

    public class WitResponseReference
    {
        public WitResponseReference child;

        public virtual string GetStringValue(WitResponseNode response)
        {
            return child.GetStringValue(response);
        }

        public virtual int GetIntValue(WitResponseNode response)
        {
            return child.GetIntValue(response);
        }
    }

    public class ArrayNodeReference : WitResponseReference
    {
        public int index;

        public override string GetStringValue(WitResponseNode response)
        {
            if (null != child)
            {
                return child.GetStringValue(response[index]);
            }

            return response[index].Value;
        }

        public override int GetIntValue(WitResponseNode response)
        {
            if (null != child)
            {
                return child.GetIntValue(response[index]);
            }

            return response[index].AsInt;
        }
    }

    public class ObjectNodeReference : WitResponseReference
    {
        public string key;

        public override string GetStringValue(WitResponseNode response)
        {
            if (null != child && null != response?[key])
            {
                return child.GetStringValue(response[key]);
            }

            return response?[key]?.Value;
        }

        public override int GetIntValue(WitResponseNode response)
        {
            if (null != child)
            {
                return child.GetIntValue(response[key]);
            }

            return response[key].AsInt;
        }
    }
}
