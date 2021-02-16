﻿using Elektronik.Common.Data.PackageObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elektronik.Common.Clouds.V2;
using UnityEngine;

namespace Elektronik.Common.Containers
{
    public class SlamLinesContainer : MonoBehaviour, ILinesContainer<SlamLine>
    {
        public LineCloudRenderer Renderer;
        
        [Obsolete("Don't use this getter. You can't get valid index outside the container anyway." +
                  "It exists only as implementation of IList interface")]
        public SlamLine this[int index]
        {
            get => _connections.Values.ElementAt(index);
            set => UpdateItem(value);
        }

        #region Unity events

        private void Start()
        {
            if (Renderer == null)
            {
                Debug.LogWarning($"No renderer set for {name}({GetType()})");
            }
            else
            {
                ItemsAdded += Renderer.OnItemsAdded;
                ItemsUpdated += Renderer.OnItemsUpdated;
                ItemsRemoved += Renderer.OnItemsRemoved;
                ItemsCleared += Renderer.OnItemsCleared;
            }
        }

        #endregion

        #region IContaitner implementation

        public event Action<IEnumerable<SlamLine>> ItemsAdded;

        public event Action<IEnumerable<SlamLine>> ItemsUpdated;

        public event Action<IEnumerable<int>> ItemsRemoved;

        public event Action ItemsCleared;

        public int Count => _connections.Count;

        public bool IsReadOnly => false;

        public bool TryGet(SlamLine obj, out SlamLine current) => TryGet(obj.pt1.id, obj.pt2.id, out current);

        public bool Contains(int id1, int id2) => TryGet(id1, id2, out SlamLine _);

        public bool Contains(SlamLine obj) => TryGet(obj.pt1.id, obj.pt2.id, out SlamLine _);

        public IEnumerator<SlamLine> GetEnumerator() => _connections.Select(kv => kv.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CopyTo(SlamLine[] array, int arrayIndex) => _connections.Values.CopyTo(array, arrayIndex);

        public int IndexOf(SlamLine item)
        {
            foreach (var c in _connections)
            {
                if (c.Value.pt1.id == item.pt1.id && c.Value.pt2.id == item.pt2.id ||
                    c.Value.pt2.id == item.pt1.id && c.Value.pt1.id == item.pt2.id)
                {
                    return c.Key;
                }
            }

            return -1;
        }

        public SlamLine this[SlamLine obj]
        {
            get => this[obj.pt1.id, obj.pt2.id];
            set => this[obj.pt1.id, obj.pt2.id] = value;
        }

        public void Add(SlamLine obj)
        {
            obj.Id = _freeIds.Count > 0 ? _freeIds.Dequeue() : _maxId++;
            _connectionsIndices[obj] = obj.Id;
            _connections[obj.Id] = obj;
            ItemsAdded?.Invoke(new[] {obj});
        }

        public void Insert(int index, SlamLine item) => Add(item);

        public void AddRange(IEnumerable<SlamLine> objects)
        {
            Debug.Assert(_linesBuffer.Count == 0);
            foreach (var obj in objects)
            {
                var line = obj;
                line.Id = _freeIds.Count > 0 ? _freeIds.Dequeue() : _maxId++;
                _connections[obj.Id] = obj;
                _connectionsIndices[obj] = line.Id;
                _linesBuffer.Add(line);
            }
            ItemsAdded?.Invoke(_linesBuffer);
            _linesBuffer.Clear();
        }

        public void UpdateItem(SlamLine obj)
        {
            int index = _connectionsIndices[obj];
            obj.Id = index;
            _connections[obj.Id] = obj;
            ItemsUpdated?.Invoke(new[] {obj});
        }

        public void UpdateItems(IEnumerable<SlamLine> objs)
        {
            Debug.Assert(_linesBuffer.Count == 0);
            foreach (var obj in objs)
            {
                var line = obj;
                line.Id = _connectionsIndices[line];
                _connections[line.Id] = line;
                _linesBuffer.Add(line);
            }

            ItemsUpdated?.Invoke(_linesBuffer);
            _linesBuffer.Clear();
        }

        public bool Remove(SlamLine obj) => Remove(obj.pt1.id, obj.pt2.id);

        public void Remove(IEnumerable<SlamLine> objs)
        {
            Debug.Assert(_linesBuffer.Count == 0);
            List<int> ids = new List<int>();
            foreach (var l in objs)
            {
                var index = _connectionsIndices[l];
                ids.Add(index);
                _connections.Remove(index);
                _freeIds.Enqueue(index);
            }

            ItemsRemoved?.Invoke(ids);
        }

        public void RemoveAt(int index)
        {
            var line = _connections[index];
            Remove(line);
        }

        public void Clear()
        {
            _connections.Clear();
            _freeIds.Clear();
            _maxId = 0;
            ItemsCleared?.Invoke();
        }

        #endregion

        #region ILinesContainer implementation

        public SlamLine this[int id1, int id2]
        {
            get
            {
                if (!TryGet(id1, id2, out SlamLine conn))
                    throw new InvalidSlamContainerOperationException(
                            $"[SlamPointsContainer.Get] Container doesn't contain point with id1({id1}) and id2({id2})");
                return conn;
            }
            set => UpdateItem(value);
        }

        public SlamLine Get(int id1, int id2)
        {
            if (TryGet(id1, id2, out SlamLine line))
            {
                return line;
            }

            throw new InvalidSlamContainerOperationException(
                    $"[SlamLinesContainer.Get] Line with id[{id1}, {id2}] doesn't exist");
        }

        public bool Remove(int id1, int id2)
        {
            if (TryGet(id1, id2, out SlamLine l))
            {
                _connections.Remove(l.Id);
                _freeIds.Enqueue(l.Id);
                ItemsRemoved?.Invoke(new[] {l.Id});
                return true;
            }

            return false;
        }

        public bool TryGet(int idx1, int idx2, out SlamLine value)
        {
            value = default;
            if (TryGet(idx1, idx2, out int lineId))
            {
                value = _connections[lineId];
                return true;
            }

            return false;
        }

        #endregion

        #region Private definitions

        private readonly List<SlamLine> _linesBuffer = new List<SlamLine>();

        private readonly IDictionary<int, SlamLine> _connections = new SortedDictionary<int, SlamLine>();
        private readonly IDictionary<SlamLine, int> _connectionsIndices = new SortedDictionary<SlamLine, int>();

        private int _maxId = 0;
        private readonly Queue<int> _freeIds = new Queue<int>();

        private bool TryGet(int idx1, int idx2, out int lineId)
        {
            foreach (var pair in _connections)
            {
                if (pair.Value.Equals(new SlamLine(idx1, idx2)))
                {
                    lineId = pair.Value.Id;
                    return true;
                }
            }

            lineId = -1;
            return false;
        }

        #endregion
    }
}