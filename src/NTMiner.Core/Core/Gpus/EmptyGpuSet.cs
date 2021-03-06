﻿using NTMiner.Core.Gpus.Impl;
using System.Collections;
using System.Collections.Generic;

namespace NTMiner.Core.Gpus {
    public class EmptyGpuSet : IGpuSet {
        public static readonly EmptyGpuSet Instance = new EmptyGpuSet();

        private List<IGpu> _list = new List<IGpu> {
            Gpu.Total
        };

        private EmptyGpuSet() {

        }

        public IGpu this[int index] {
            get {
                return Impl.Gpu.Total;
            }
        }

        public GpuType GpuType {
            get {
                return GpuType.Empty;
            }
        }

        public int Count {
            get {
                return 0;
            }
        }

        public List<GpuSetProperty> Properties { get; private set; } = new List<GpuSetProperty>();

        public IEnumerator<IGpu> GetEnumerator() {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _list.GetEnumerator();
        }
    }
}
