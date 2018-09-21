﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Elektronik.Offline.Events
{
    public class MainThreadEvent : ISlamEvent
    {
        public SlamEventType EventType { get; set; }
        public int Timestamp { get; set; }
        public bool IsKeyEvent { get; set; }

        public uint NewPointsCount { get; set; }
        public int RecognizedPointsCount { get; set; }
        public int RecalcPointsCount { get; set; }
        public int LocalPointsCount { get; set; }
        public int NewKeyObservationId { get; set; }
        public Pose ObservationPose { get; set; }
        public byte CovisibleObservationsCount { get; set; }

        public SlamObservation[] Observations { get; set; }
        public SlamPoint[] Points { get; private set; }
        public SlamLine[] Lines { get; private set; }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MAIN THREAD")
              .AppendFormat("Timestamp: {0}", TimeSpan.FromMilliseconds(Timestamp).ToString())
              .AppendLine()
              .AppendFormat("New points count: {0}", NewPointsCount)
              .AppendLine()
              .AppendFormat("Recognized points: {0}", RecognizedPointsCount)
              .AppendLine()
              .AppendFormat("Recalculated points: {0}", RecalcPointsCount)
              .AppendLine()
              .AppendFormat("Local points count: {0}", LocalPointsCount)
              .AppendLine()
              .AppendFormat("New key observation ID: {0}", NewKeyObservationId);
            return sb.ToString();
        }

        public MainThreadEvent()
        {
            EventType = SlamEventType.MainThreadEvent;
            IsKeyEvent = false;
        }

        public static MainThreadEvent Parse(BinaryReader stream)
        {
            MainThreadEvent parsed = new MainThreadEvent();
            parsed.Timestamp = (int)stream.ReadUInt32();

            parsed.NewPointsCount = stream.ReadUInt32();
            Debug.Log(String.Format("New points count = {0}", parsed.NewPointsCount));

            SlamPoint[] newPoints = new SlamPoint[parsed.NewPointsCount];

            for (int i = 0; i < parsed.NewPointsCount; ++i)
            {
                newPoints[i].id = stream.ReadInt32();
                newPoints[i].color = Color.blue;
            }
            for (int i = 0; i < parsed.NewPointsCount; ++i)
            {
                newPoints[i].position.x = stream.ReadSingle();
                newPoints[i].position.y = stream.ReadSingle();
                newPoints[i].position.z = stream.ReadSingle();
            }

            parsed.RecognizedPointsCount = stream.ReadInt32();

            SlamPoint[] recognizedPoints = new SlamPoint[parsed.RecognizedPointsCount];
            for (int i = 0; i < parsed.RecognizedPointsCount; ++i)
            {
                recognizedPoints[i].id = stream.ReadInt32();
                recognizedPoints[i].color = new Color32(0xf4, 0xb3, 0x42, 0xff);
            }

            parsed.RecalcPointsCount = stream.ReadInt32();
            SlamPoint[] recalcPoints = new SlamPoint[parsed.RecalcPointsCount];
            for (int i = 0; i < parsed.RecalcPointsCount; ++i)
            {
                recalcPoints[i].id = stream.ReadInt32();
                recalcPoints[i].color = new Color32(0xf4, 0xb3, 0x42, 0xff);
            }

            parsed.LocalPointsCount = stream.ReadInt32();
            SlamPoint[] localPoints = new SlamPoint[parsed.LocalPointsCount];
            for (int i = 0; i < parsed.LocalPointsCount; ++i)
            {
                localPoints[i].id = stream.ReadInt32();
                localPoints[i].color = Color.green;
            }

            parsed.Points = newPoints.Concat(recognizedPoints).Concat(recalcPoints).Concat(localPoints).ToArray();

            SlamObservation observation = new SlamObservation();

            observation.id = stream.ReadInt32();
            if (observation.id != -1)
                parsed.IsKeyEvent = true;

            observation.color = Color.gray;

            float x = stream.ReadSingle(), y = stream.ReadSingle(), z = stream.ReadSingle();
            float qw = stream.ReadSingle(), qx = stream.ReadSingle(), qy = stream.ReadSingle(), qz = stream.ReadSingle();

            observation.position = new Vector3(x, y, z);
            observation.orientation = new Quaternion(qx, qy, qz, qw);

            parsed.CovisibleObservationsCount = stream.ReadByte();

            observation.covisibleObservationsIds = new int[parsed.CovisibleObservationsCount];
            for (int i = 0; i < parsed.CovisibleObservationsCount; ++i)
            {
                observation.covisibleObservationsIds[i] = stream.ReadInt32();
            }

            observation.covisibleObservationsOfCommonPointsCount = new int[parsed.CovisibleObservationsCount];
            for (int i = 0; i < parsed.CovisibleObservationsCount; ++i)
            {
                observation.covisibleObservationsOfCommonPointsCount[i] = stream.ReadInt32();
            }
            parsed.Observations = new[] { observation };

            return parsed;
        }

    }
}
