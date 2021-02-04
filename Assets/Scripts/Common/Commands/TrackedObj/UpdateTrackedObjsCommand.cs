﻿using Elektronik.Common.Containers;
using Elektronik.Common.Data.PackageObjects;
using Elektronik.Common.Data.Pb;
using Elektronik.Common.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elektronik.Common.Commands.Generic
{
    public class UpdateTrackedObjsCommand : UpdateCommand<SlamTrackedObject>
    {
        private GameObjectsContainer<SlamTrackedObject> m_goContainer;

        public UpdateTrackedObjsCommand(GameObjectsContainer<SlamTrackedObject> container, IEnumerable<SlamTrackedObject> data)
            : base(container, data)
        {
            m_goContainer = container;
        }

        public override void Execute()
        {
            base.Execute();
            for (int i = 0; i < m_objs2Update.Count; ++i)
            {
                if (m_goContainer.TryGet(m_objs2Update[i], out GameObject helmetGO))
                {
                    helmetGO.GetComponent<Helmet>().IncrementTrack();
                }
            }
        }

        public override void UnExecute()
        {
            base.UnExecute();
            foreach (var o in m_objs2Update)
            {
                if (m_goContainer.TryGet(o, out GameObject helmetGO))
                {
                    helmetGO.GetComponent<Helmet>().DecrementTrack();
                }
            }
        }
    }
}