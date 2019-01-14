﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Elektronik.Common.Data
{
    public static class SlamObservationPackageObject
    {
        public static int GetSizeOfActionInBytes(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Create:
                    return sizeof(float) * 3 + sizeof(float) * 4; // Vector3
                case ActionType.Tint:
                    return sizeof(byte) * 3; // color
                case ActionType.Move:
                    return sizeof(float) * 3; // Vector3
                case ActionType.Remove:
                    return 0;
                case ActionType.Fuse:
                    return sizeof(int) + sizeof(byte) * 6; // id + color1 + color2
                case ActionType.Connect:
                    return sizeof(int) * 2; // id + count of common points
                default:
                    return -1;
            }
        }

        public static void ParseActions(byte[] actions, int id, out SlamObservation observation)
        {
            int offset = 0;
            observation = new SlamObservation();
            observation.id = id;
            while (offset != actions.Length)
            {
                Debug.AssertFormat(offset <= actions.Length, "[SlamObservationPackageObject.ParseActions] offset ({0}) out of range", offset);
                ActionType type = (ActionType)actions[offset++];
                if (type == ActionType.Create || type == ActionType.Move)
                {
                    observation.position = SlamBitConverter.ToVector3(actions, offset);
                    offset += sizeof(float) * 3;
                    observation.orientation = SlamBitConverter.ToQuaternion(actions, offset);
                    offset += sizeof(float) * 4;
                    observation.color = Color.gray;
                    return;
                }
                if (type == ActionType.Tint)
                {
                    observation.color = SlamBitConverter.ToRGBColor(actions, offset);
                    offset += sizeof(byte) * 3;
                    return;
                }
                if (type == ActionType.Remove)
                {
                    observation.color = Color.red;
                    observation.isRemoved = true;
                    return;
                }
                if (type == ActionType.Connect)
                {
                    int covisibleId = BitConverter.ToInt32(actions, offset);
                    offset += sizeof(int);
                    int countOfCommonPoints = BitConverter.ToInt32(actions, offset);
                    offset += sizeof(int);
                    observation.covisibleObservationsIds.Add(covisibleId);
                    observation.covisibleObservationsOfCommonPointsCount.Add(countOfCommonPoints);
                }
            }
        }

    }
}
