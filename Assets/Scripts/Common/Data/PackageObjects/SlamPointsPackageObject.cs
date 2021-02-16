﻿using Elektronik.Common.Extensions;
using System;
using System.Text;
using UnityEngine;

namespace Elektronik.Common.Data.PackageObjects
{
    public static class SlamPointsPackageObject
    {
        private static readonly int MAX_MESSAGE_LENGTH_IN_BYTES = 128;

        public static int GetSizeOfActionInBytes(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Create:
                    return sizeof(float) * 3 + sizeof(byte) * 3; // Vector3 + color
                case ActionType.Tint:
                    return sizeof(byte) * 3; // color
                case ActionType.Move:
                    return sizeof(float) * 3; // Vector3
                case ActionType.Remove:
                    return 0;
                case ActionType.Fuse:
                    return sizeof(int) + sizeof(byte) * 6; // id + color1 + color2
                case ActionType.Message:
                    return sizeof(int) + sizeof(byte) * MAX_MESSAGE_LENGTH_IN_BYTES; // size + message bytes
                default:
                    throw new ArgumentException(String.Format("Bad action type ({0})", actionType));
            }
        }

        public static void ParseActions(byte[] actions, int id, out SlamPoint point, out SlamLine? fuse)
        {
            int offset = 0;
            point = new SlamPoint();
            fuse = null;
            point.id = id;
            bool wasMoved = false;
            while (offset != actions.Length)
            {
                Debug.AssertFormat(offset <= actions.Length, "[SlamPointsPackageObject.ParseActions] offset ({0}) out of range", offset);
                ActionType type = (ActionType)actions[offset++];
                if (type == ActionType.Create || type == ActionType.Move)
                {
                    point.position = BitConverterEx.ToVector3(actions, offset, ref offset);
                    wasMoved = true;
                }
                if (type == ActionType.Create)
                {
                    point.isNew = true;
                    point.defaultColor = BitConverterEx.ToRGBColor32(actions, offset, ref offset);
                    point.color = Color.blue;
                }
                if (type == ActionType.Tint)
                {
                    point.color = BitConverterEx.ToRGBColor32(actions, offset, ref offset);
                    point.justColored = !wasMoved;
                }
                if (type == ActionType.Remove)
                {
                    point.color = Color.red;
                    point.isRemoved = true;
                }
                if (type == ActionType.Fuse)
                {
                    point.color = Color.magenta;
                    SlamLine fuseLine = new SlamLine()
                    {
                        pointId1 = point.id,
                        pointId2 = BitConverterEx.ToInt32(actions, offset, ref offset),
                        color1 = BitConverterEx.ToRGBColor32(actions, offset, ref offset),
                        color2 = BitConverterEx.ToRGBColor32(actions, offset, ref offset),
                        isRemoved = true,
                    };
                    if (fuseLine.pointId1 != -1 && fuseLine.pointId2 != -1)
                        fuse = fuseLine;
                }
                if (type == ActionType.Message)
                {
                    int countOfMsgBytes = BitConverterEx.ToInt32(actions, offset, ref offset);
                    if (countOfMsgBytes >= MAX_MESSAGE_LENGTH_IN_BYTES)
                        throw new Exception();
                    point.message = countOfMsgBytes > 0 ? Encoding.ASCII.GetString(actions, offset, countOfMsgBytes) : "";
                    offset += sizeof(byte) * MAX_MESSAGE_LENGTH_IN_BYTES;
                }
            }
        }
    }
}