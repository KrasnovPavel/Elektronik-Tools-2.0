﻿using Elektronik.Common.Containers;
using System.Collections.Generic;
using System.Linq;
using Elektronik.Common.Data.PackageObjects;

namespace Elektronik.Common.Commands.Generic
{
    public class AddConnectionsCommand<T> : ICommand where T: ICloudItem
    {
        protected readonly IList<(int, int)> Connections;
        protected readonly IConnectableObjectsContainer<T> Container;

        public AddConnectionsCommand(IConnectableObjectsContainer<T> container, IEnumerable<(int, int)> newConnections)
        {
            Connections = newConnections.ToList();
            Container = container;
        }

        public virtual void Execute() => Container.AddConnections(Connections);
        public virtual void UnExecute() => Container.RemoveConnections(Connections);
    }
}
