﻿using NTMiner.ServiceContracts.DataObjects;
using NTMiner.Views.Ucs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace NTMiner.Vms {
    public class MinerClientsViewModel : ViewModelBase {
        public static readonly MinerClientsViewModel Current = new MinerClientsViewModel();

        private List<MinerClientViewModel> _minerClients = new List<MinerClientViewModel>();
        private int _minerClientPageIndex = 1;
        private int _minerClientPageSize = 20;
        private int _minerClientTotal;
        private EnumItem<MineStatus> _mineStatusEnumItem;
        private string _minerIp;
        private string _minerName;
        private CoinViewModel _mainCoin;
        private CoinViewModel _dualCoin;
        private PoolViewModel _mainCoinPool;
        private PoolViewModel _dualCoinPool;
        private MineWorkViewModel _selectedMineWork;
        private MinerGroupViewModel _selectedMinerGroup;
        public ICommand PageUp { get; private set; }
        public ICommand PageDown { get; private set; }
        public ICommand PageFirst { get; private set; }
        public ICommand PageLast { get; private set; }
        public ICommand PageRefresh { get; private set; }
        public ICommand ManageCoin { get; private set; }
        public ICommand ManagePool { get; private set; }
        public ICommand ManageWallet { get; private set; }

        private MinerClientsViewModel() {
            this._mineStatusEnumItem = this.MineStatusEnumItems.FirstOrDefault(a => a.Value == MineStatus.All);
            this._mainCoin = CoinViewModel.PleaseSelect;
            this._dualCoin = CoinViewModel.PleaseSelect;
            this._selectedMineWork = MineWorkViewModel.PleaseSelect;
            this._selectedMinerGroup = MinerGroupViewModel.PleaseSelect;
            this._mainCoinPool = _mainCoin.OptionPools.First();
            this._dualCoinPool = _dualCoin.OptionPools.First();
            this._mainCoinWallet = WalletViewModel.PleaseSelect;
            this._dualCoinWallet = WalletViewModel.PleaseSelect;
            this.PageUp = new DelegateCommand(() => {
                this.MinerClientPageIndex = this.MinerClientPageIndex - 1;
            });
            this.PageDown = new DelegateCommand(() => {
                this.MinerClientPageIndex = this.MinerClientPageIndex + 1;
            });
            this.PageFirst = new DelegateCommand(() => {
                this.MinerClientPageIndex = 1;
            });
            this.PageLast = new DelegateCommand(() => {
                this.MinerClientPageIndex = MinerClientPageCount;
            });
            this.PageRefresh = new DelegateCommand(() => {
                QueryMinerClients();
            });
            this.ManageCoin = new DelegateCommand<CoinViewModel>((coinVm) => {
                CoinPage.ShowWindow(coinVm);
            });
            this.ManagePool = new DelegateCommand<CoinViewModel>((coinVm) => {
                this.ManageCoin.Execute(coinVm);
                CoinPageViewModel.Current.SwitchToPoolTab();
            });
            this.ManageWallet = new DelegateCommand<CoinViewModel>((coinVm) => {
                this.ManageCoin.Execute(coinVm);
                CoinPageViewModel.Current.SwitchToWalletTab();
            });
            System.Timers.Timer t = new System.Timers.Timer(50);
            t.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => {
                if (this._logoRotateTransformAngle > 3600000) {
                    this._logoRotateTransformAngle = 0;
                }
                this.LogoRotateTransformAngle += 50;
            };
            t.Start();
        }

        private double _logoRotateTransformAngle;
        private WalletViewModel _mainCoinWallet;
        private WalletViewModel _dualCoinWallet;

        public int MinerClientRefreshPeriod {
            get {
                return 120;
            }
        }

        public double LogoRotateTransformAngle {
            get => _logoRotateTransformAngle;
            set {
                _logoRotateTransformAngle = value;
                OnPropertyChanged(nameof(LogoRotateTransformAngle));
            }
        }

        private static readonly List<int> _pageSizeItems = new List<int>() { 10, 20, 30, 40 };
        public List<int> PageSizeItems {
            get {
                return _pageSizeItems;
            }
        }

        public bool IsPageUpEnabled {
            get {
                if (this.MinerClientPageIndex <= 1) {
                    return false;
                }
                return true;
            }
        }

        public bool IsPageDownEnabled {
            get {
                if (this.MinerClientPageIndex >= this.MinerClientPageCount) {
                    return false;
                }
                return true;
            }
        }

        public int MinerClientPageIndex {
            get => _minerClientPageIndex;
            set {
                _minerClientPageIndex = value;
                OnPropertyChanged(nameof(MinerClientPageIndex));
                QueryMinerClients();
            }
        }

        public int MinerClientPageCount {
            get {
                return (int)Math.Ceiling((double)this.MinerClientTotal / this.MinerClientPageSize);
            }
        }

        public int MinerClientPageSize {
            get => _minerClientPageSize;
            set {
                _minerClientPageSize = value;
                OnPropertyChanged(nameof(MinerClientPageSize));
                QueryMinerClients();
            }
        }

        public int MinerClientTotal {
            get => _minerClientTotal;
            set {
                _minerClientTotal = value;
                OnPropertyChanged(nameof(MinerClientTotal));
            }
        }

        public void QueryMinerClients() {
            int total = _minerClientTotal;
            Guid? mineWorkId = null;
            if (SelectedMineWork != MineWorkViewModel.PleaseSelect) {
                mineWorkId = SelectedMineWork.Id;
            }
            string mainCoin = string.Empty;
            string dualCoin = string.Empty;
            string mainCoinPool = string.Empty;
            string dualCoinPool = string.Empty;
            string mainCoinWallet = string.Empty;
            string dualCoinWallet = string.Empty;
            if (mineWorkId == null || mineWorkId.Value == Guid.Empty) {
                if (this.MainCoin != CoinViewModel.PleaseSelect) {
                    mainCoin = this.MainCoin.Code;
                    if (this.MainCoinPool != null) {
                        mainCoinPool = this.MainCoinPool.Server;
                    }
                }
                if (this.DualCoin != CoinViewModel.PleaseSelect) {
                    dualCoin = this.DualCoin.Code;
                    if (this.DualCoin == CoinViewModel.DualCoinEnabled) {
                        dualCoin = "*";
                    }
                    if (this.DualCoinPool != null) {
                        dualCoinPool = this.DualCoinPool.Server;
                    }
                }
                if (this.MainCoinWallet != null && this.MainCoinWallet != WalletViewModel.PleaseSelect) {
                    mainCoinWallet = this.MainCoinWallet.Address;
                }
                if (this.DualCoinWallet != null && this.DualCoinWallet != WalletViewModel.PleaseSelect) {
                    dualCoinWallet = this.DualCoinWallet.Address;
                }
            }
            Server.ControlCenterService.QueryClients(
                this.MinerClientPageIndex,
                this.MinerClientPageSize,
                mineWorkId,
                this.MinerIp, this.MinerName,
                this.MineStatusEnumItem.Value,
                mainCoin, mainCoinPool, mainCoinWallet,
                dualCoin, dualCoinPool, dualCoinWallet, (response)=> {
                    if (response != null) {
                        Execute.OnUIThread(() => {
                            this.MinerClients = response.Data.Select(a => new MinerClientViewModel(a)).ToList();
                            OnPropertyChanged(nameof(MinerClientTotal));
                            OnPropertyChanged(nameof(MinerClientPageCount));
                            OnPropertyChanged(nameof(IsPageDownEnabled));
                            OnPropertyChanged(nameof(IsPageUpEnabled));
                            RefreshPagingUI(response.Total);
                        });
                    }
                });
        }

        public void LoadClients() {
            Server.ControlCenterService.LoadClients(this.MinerClients.Select(a => a.ClientDataVm.Id).ToList(), (response)=> {
                Execute.OnUIThread(() => {
                    if (response != null) {
                        foreach (var item in this.MinerClients) {
                            ClientData data = response.Data.FirstOrDefault(a => a.Id == item.ClientDataVm.Id);
                            if (data != null) {
                                item.ClientDataVm.Update(data);
                            }
                        }
                    }
                });
            });
        }

        private void RefreshPagingUI(int total) {
            _minerClientTotal = total;
            OnPropertyChanged(nameof(MinerClientTotal));
            OnPropertyChanged(nameof(MinerClientPageCount));
            OnPropertyChanged(nameof(IsPageDownEnabled));
            OnPropertyChanged(nameof(IsPageUpEnabled));
            if (MinerClientTotal == 0) {
                _minerClientPageIndex = 0;
                OnPropertyChanged(nameof(MinerClientPageIndex));
            }
            else if (MinerClientPageIndex == 0) {
                _minerClientPageIndex = 1;
                OnPropertyChanged(nameof(MinerClientPageIndex));
            }
        }

        public List<MinerClientViewModel> MinerClients {
            get {
                return _minerClients;
            }
            private set {
                _minerClients = value;
                OnPropertyChanged(nameof(MinerClients));
            }
        }

        public CoinViewModels MineCoinVms {
            get {
                return CoinViewModels.Current;
            }
        }

        private IEnumerable<CoinViewModel> GetDualCoinVmItems() {
            yield return CoinViewModel.PleaseSelect;
            yield return CoinViewModel.DualCoinEnabled;
            foreach (var item in CoinViewModels.Current.AllCoins) {
                yield return item;
            }
        }
        public List<CoinViewModel> DualCoinVmItems {
            get {
                return GetDualCoinVmItems().ToList();
            }
        }

        public CoinViewModel MainCoin {
            get { return _mainCoin; }
            set {
                if (_mainCoin != value) {
                    _mainCoin = value;
                    OnPropertyChanged(nameof(MainCoin));
                    OnPropertyChanged(nameof(MainCoinPool));
                    this.MainCoinPool = PoolViewModel.PleaseSelect;
                    this.MainCoinWallet = WalletViewModel.PleaseSelect;
                    OnPropertyChanged(nameof(IsMainCoinSelected));
                    QueryMinerClients();
                }
            }
        }

        public bool IsMainCoinSelected {
            get {
                if (MainCoin == CoinViewModel.PleaseSelect) {
                    return false;
                }
                return true;
            }
        }

        public PoolViewModel MainCoinPool {
            get => _mainCoinPool;
            set {
                if (_mainCoinPool != value) {
                    _mainCoinPool = value;
                    OnPropertyChanged(nameof(MainCoinPool));
                    QueryMinerClients();
                }
            }
        }

        public WalletViewModel MainCoinWallet {
            get => _mainCoinWallet;
            set {
                _mainCoinWallet = value;
                OnPropertyChanged(nameof(MainCoinWallet));
                QueryMinerClients();
            }
        }

        public WalletViewModel DualCoinWallet {
            get => _dualCoinWallet;
            set {
                _dualCoinWallet = value;
                OnPropertyChanged(nameof(DualCoinWallet));
                QueryMinerClients();
            }
        }

        public CoinViewModel DualCoin {
            get {
                return _dualCoin;
            }
            set {
                if (_dualCoin != value) {
                    _dualCoin = value;
                    OnPropertyChanged(nameof(DualCoin));
                    this.DualCoinPool = PoolViewModel.PleaseSelect;
                    this.DualCoinWallet = WalletViewModel.PleaseSelect;
                    OnPropertyChanged(nameof(IsDualCoinSelected));
                    QueryMinerClients();
                }
            }
        }

        public bool IsDualCoinSelected {
            get {
                if (DualCoin == CoinViewModel.PleaseSelect || DualCoin == CoinViewModel.DualCoinEnabled) {
                    return false;
                }
                return true;
            }
        }

        public PoolViewModel DualCoinPool {
            get => _dualCoinPool;
            set {
                if (_dualCoinPool != null) {
                    _dualCoinPool = value;
                    OnPropertyChanged(nameof(DualCoinPool));
                    QueryMinerClients();
                }
            }
        }

        public string MinerIp {
            get => _minerIp;
            set {
                if (_minerIp != value) {
                    _minerIp = value;
                    OnPropertyChanged(nameof(MinerIp));
                    if (!string.IsNullOrEmpty(value)) {
                        IPAddress ip;
                        if (!IPAddress.TryParse(value, out ip)) {
                            throw new ValidationException("IP地址格式不正确");
                        }
                    }
                    QueryMinerClients();
                }
            }
        }
        public string MinerName {
            get => _minerName;
            set {
                if (_minerName != value) {
                    _minerName = value;
                    OnPropertyChanged(nameof(MinerName));
                    QueryMinerClients();
                }
            }
        }

        public MineWorkViewModels MineWorkVms {
            get {
                return MineWorkViewModels.Current;
            }
        }

        private IEnumerable<MinerGroupViewModel> GetMinerGroupVmItems() {
            yield return MinerGroupViewModel.PleaseSelect;
            foreach (var item in MinerGroupViewModels.Current.List) {
                yield return item;
            }
        }
        public List<MinerGroupViewModel> MinerGroupVmItems {
            get {
                return GetMinerGroupVmItems().ToList();
            }
        }

        public MineWorkViewModel SelectedMineWork {
            get => _selectedMineWork;
            set {
                _selectedMineWork = value;
                OnPropertyChanged(nameof(SelectedMineWork));
                OnPropertyChanged(nameof(IsMineWorkSelected));
                QueryMinerClients();
            }
        }

        public bool IsMineWorkSelected {
            get {
                if (SelectedMineWork != MineWorkViewModel.PleaseSelect && SelectedMineWork != MineWorkViewModel.FreeMineWork) {
                    return true;
                }
                return false;
            }
        }

        public MinerGroupViewModel SelectedMinerGroup {
            get => _selectedMinerGroup;
            set {
                _selectedMinerGroup = value;
                OnPropertyChanged(nameof(SelectedMinerGroup));
            }
        }

        public IEnumerable<EnumItem<MineStatus>> MineStatusEnumItems {
            get {
                return MineStatus.All.GetEnumItems();
            }
        }

        public EnumItem<MineStatus> MineStatusEnumItem {
            get => _mineStatusEnumItem;
            set {
                if (_mineStatusEnumItem != value) {
                    _mineStatusEnumItem = value;
                    OnPropertyChanged(nameof(MineStatusEnumItem));
                    QueryMinerClients();
                }
            }
        }
    }
}
