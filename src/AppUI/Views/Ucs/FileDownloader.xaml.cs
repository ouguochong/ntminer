﻿using NTMiner.Vms;
using System;
using System.Windows.Controls;

namespace NTMiner.Views.Ucs {
    public partial class FileDownloader : UserControl {
        public static void ShowWindow(string downloadFileUrl, string fileTitle, Action<ContainerWindow, bool, string, string> downloadComplete) {
            Execute.OnUIThread(() => {
                ContainerWindow.ShowWindow(new ContainerWindowViewModel {
                    IconName = "Icon_Download",
                    CloseVisible = System.Windows.Visibility.Visible,
                }, ucFactory: (window) => {
                    FileDownloaderViewModel vm = new FileDownloaderViewModel(downloadFileUrl, (isSuccess, message, saveFileFullName) => {
                        downloadComplete(window, isSuccess, message, saveFileFullName);
                    });
                    return new FileDownloader(vm);
                }, fixedSize: true);
            });
        }

        public FileDownloaderViewModel Vm {
            get {
                return (FileDownloaderViewModel)this.DataContext;
            }
        }

        public FileDownloader(FileDownloaderViewModel vm) {
            this.DataContext = vm;
            InitializeComponent();
            ResourceDictionarySet.Instance.FillResourceDic(this, this.Resources);
        }
    }
}
