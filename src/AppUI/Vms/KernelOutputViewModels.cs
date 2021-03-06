﻿using NTMiner.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Vms {
    public class KernelOutputViewModels : ViewModelBase {
        public static readonly KernelOutputViewModels Current = new KernelOutputViewModels();

        private readonly Dictionary<Guid, KernelOutputViewModel> _dicById = new Dictionary<Guid, KernelOutputViewModel>();

        private KernelOutputViewModels() {
            Global.Access<KernelOutputAddedEvent>(
                Guid.Parse("E1864E02-ED87-49D6-AD9F-A54953D6BC7E"),
                "添加了内核输出组后刷新VM内存",
                LogEnum.None,
                action: message => {
                    var vm = new KernelOutputViewModel(message.Source);
                    _dicById.Add(message.Source.GetId(), vm);
                    OnPropertyChanged(nameof(AllKernelOutputVms));
                    OnPropertyChanged(nameof(PleaseSelectVms));
                });
            Global.Access<KernelOutputUpdatedEvent>(
                Guid.Parse("AA98F304-B7E1-4E93-8AFB-55F72EA37689"),
                "更新了内核输出组后刷新VM内存",
                LogEnum.None,
                action: message => {
                    if (_dicById.ContainsKey(message.Source.GetId())) {
                        var item = _dicById[message.Source.GetId()];
                        if (item != null) {
                            item.Update(message.Source);
                        }
                    }
                });
            Global.Access<KernelOutputRemovedEvent>(
                Guid.Parse("BE4A8820-AA3E-474B-AF04-9798D4B08DFC"),
                "移除了内核输出组后刷新VM内存",
                LogEnum.None,
                action: message => {
                    if (_dicById.ContainsKey(message.Source.GetId())) {
                        _dicById.Remove(message.Source.GetId());
                        OnPropertyChanged(nameof(AllKernelOutputVms));
                        OnPropertyChanged(nameof(PleaseSelectVms));
                    }
                });
            foreach (var item in NTMinerRoot.Current.KernelOutputSet) {
                _dicById.Add(item.GetId(), new KernelOutputViewModel(item));
            }
        }

        public bool TryGetKernelOutputVm(Guid id, out KernelOutputViewModel kernelOutputVm) {
            return _dicById.TryGetValue(id, out kernelOutputVm);
        }

        public List<KernelOutputViewModel> AllKernelOutputVms {
            get {
                return _dicById.Values.OrderBy(a => a.Name).ToList();
            }
        }

        private IEnumerable<KernelOutputViewModel> GetPleaseSelectVms() {
            yield return KernelOutputViewModel.PleaseSelect;
            foreach (var item in _dicById.Values.OrderBy(a => a.Name)) {
                yield return item;
            }
        }

        public List<KernelOutputViewModel> PleaseSelectVms {
            get {
                return GetPleaseSelectVms().ToList();
            }
        }
    }
}
