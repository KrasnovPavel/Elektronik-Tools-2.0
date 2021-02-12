using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Elektronik.Common.Clouds
{
    public class PointCloudRenderer : MonoBehaviour
    {
        public FastPointCloudV2 PointsCloud;
        public float PointSize = 1f;
        public Shader PointCloudShader;
        
        private int _capacity = 1024 * 1024;
        private static readonly int PointBuffer = Shader.PropertyToID("_PointBuffer");
        private static readonly int Size = Shader.PropertyToID("_PointSize");
        private Material _renderMaterial;
        private ComputeBuffer _pointsBuffer;
        private NativeArray<CloudPointV2> _gpuArray;
        private bool _toUpdate;
        private bool _resizeNeeded;

        private void Start()
        {
            _pointsBuffer = new ComputeBuffer(_capacity, CloudPointV2.Size, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
            _pointsBuffer.SetData(Enumerable.Repeat(CloudPointV2.Empty(), _capacity).ToArray());
            _gpuArray = _pointsBuffer.BeginWrite<CloudPointV2>(0, _capacity);
            _renderMaterial = new Material(PointCloudShader);
            _renderMaterial.hideFlags = HideFlags.DontSave;
            _renderMaterial.EnableKeyword("_COMPUTE_BUFFER");
            
            PointsCloud.PointsAdded += OnPointsAdded;
            PointsCloud.PointsUpdated += OnPointsUpdated;
            PointsCloud.PointsRemoved += OnPointsRemoved;
            PointsCloud.PointsCleared += OnPointsCleared;
        }

        private void Update()
        {
            if (_resizeNeeded)
            {
                _pointsBuffer.EndWrite<CloudPointV2>(_capacity);
                _pointsBuffer.Release();
                _capacity *= 2;
                _pointsBuffer = new ComputeBuffer(_capacity, 
                                                  CloudPointV2.Size, 
                                                  ComputeBufferType.Default,
                                                  ComputeBufferMode.SubUpdates);
                
                var newCapacity = _capacity * 2;
                var tmpBuffer = new ComputeBuffer(newCapacity, 
                                                  CloudPointV2.Size, 
                                                  ComputeBufferType.Default,
                                                  ComputeBufferMode.SubUpdates);
                tmpBuffer.SetData(PointsCloud.GetAll().Select(p => new CloudPointV2(p)).ToArray());
                _pointsBuffer = tmpBuffer;
                _capacity = newCapacity;
                _gpuArray = _pointsBuffer.BeginWrite<CloudPointV2>(0, _capacity);
                _toUpdate = false;
                _resizeNeeded = false;
            }
            else
            {
                if (_toUpdate)
                {
                    _pointsBuffer.EndWrite<CloudPointV2>(_capacity);
                    _gpuArray = _pointsBuffer.BeginWrite<CloudPointV2>(0, _capacity);
                    _toUpdate = false;
                }
            }
        }
        
        private void OnRenderObject()
        {
            if (_pointsBuffer == null) return;
            _renderMaterial.SetPass(0);
            _renderMaterial.SetBuffer(PointBuffer, _pointsBuffer);
            _renderMaterial.SetFloat(Size, PointSize);
            Graphics.DrawProceduralNow(MeshTopology.Points, _pointsBuffer.count, 1);
        }

        private void OnPointsAdded(IList<CloudPoint> points)
        {
            foreach (var point in points)
            {
                _gpuArray[point.idx] = new CloudPointV2(point);
            }

            _toUpdate = true;
            if (PointsCloud.Count > _capacity * 3 / 4)
            {
                _resizeNeeded = true;
            }
            Debug.LogError(PointsCloud.Count);
        }

        private void OnPointsUpdated(IList<CloudPoint> points)
        {
            // foreach (var point in points)
            // {
            //     int layer = point.idx / PointCloudBlock.Capacity;
            //     int inLayerId = point.idx % PointCloudBlock.Capacity;
            //     _blocks[layer].Points[inLayerId] = new CloudPointV2(point);
            //     _blocks[layer].Updated = true;
            // }
        }

        private void OnPointsRemoved(IList<int> removedPointsIds)
        {
            // foreach (var pointId in removedPointsIds)
            // {
            //     int layer = pointId / PointCloudBlock.Capacity;
            //     int inLayerId = pointId % PointCloudBlock.Capacity;
            //     _blocks[layer].Points[inLayerId] = CloudPointV2.Empty();
            //     _blocks[layer].Updated = true;
            // }
        }

        private void OnPointsCleared()
        {
            // foreach (var block in _blocks)
            // {
            //     Destroy(block);
            // }
        }
    }
}