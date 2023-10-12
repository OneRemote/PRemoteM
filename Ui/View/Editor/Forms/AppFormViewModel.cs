﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using _1RM.Model.Protocol;
using _1RM.Model.Protocol.Base;
using _1RM.Service;
using _1RM.Utils;
using Newtonsoft.Json;
using Shawn.Utils.Interface;
using Shawn.Utils.Wpf;
using Shawn.Utils.Wpf.FileSystem;

namespace _1RM.View.Editor.Forms
{
    // TODO 所有的 from 都改为 viewmodel, 并继承自同一基类
    public class AppFormViewModel : NotifyPropertyChangedBaseScreen, IDataErrorInfo
    {
        private readonly LocalApp _localApp;
        public LocalApp New => _localApp;
        public AppFormViewModel(LocalApp localApp)
        {
            _localApp = localApp;
            _localApp.ArgumentList.CollectionChanged -= ArgumentListOnCollectionChanged;
            _localApp.ArgumentList.CollectionChanged += ArgumentListOnCollectionChanged;
            CheckMacroRequirement();
        }

        ~AppFormViewModel()
        {
            _localApp.ArgumentList.CollectionChanged -= ArgumentListOnCollectionChanged;
        }

        private void ArgumentListOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is AppArgument arg)
                    {
                        arg.PropertyChanged -= ArgOnPropertyChanged;
                    }
                }
            }
            CheckMacroRequirement();
        }

        private void ArgOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppArgument.Value))
                RaisePropertyChanged(nameof(Demo));
        }

        public Visibility SelectionsVisibility { get; set; } = Visibility.Collapsed;
        public bool IsConst { get; set; } = false;




        public string ExePath
        {
            get => New.ExePath;
            set
            {
                if (New.ExePath != value)
                {
                    New.ExePath = value;
                    RaisePropertyChanged();

                    var t = AppArgumentHelper.GetPresetArgumentList(ExePath);
                    if (t != null)
                    {
                        bool same = New.ArgumentList.Count == t.Item2.Count;
                        if (same)
                            for (int i = 0; i < New.ArgumentList.Count; i++)
                            {
                                if (!New.ArgumentList[i].IsConfigEqualTo(t.Item2[i]))
                                {
                                    same = false;
                                    break;
                                }
                            }
                        if (!same && (New.ArgumentList.All(x => x.IsDefaultValue())
                                      || MessageBoxHelper.Confirm("TXT: 用适配 path 的参数覆盖当前参数列表？")))
                        {
                            New.RunWithHosting = t.Item1;
                            _localApp.ArgumentList.CollectionChanged -= ArgumentListOnCollectionChanged;
                            New.ArgumentList = new ObservableCollection<AppArgument>(t.Item2);
                            _localApp.ArgumentList.CollectionChanged += ArgumentListOnCollectionChanged;
                            CheckMacroRequirement();
                        }
                    }

                    RaisePropertyChanged(nameof(Demo));
                }
            }
        }

        public string Address
        {
            get => New.Address;
            set
            {
                var v = value;
                if (!RequiredHostName)
                    v = "";
                else if (New.Address != v)
                {
                    New.Address = v;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Demo));
                }
            }
        }

        public string Port
        {
            get => New.Port;
            set
            {
                var v = value;
                if (!RequiredPort)
                    v = "";
                else if (New.Port != v)
                {
                    New.Port = v;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Demo));
                }
            }
        }

        public string UserName
        {
            get => New.UserName;
            set
            {
                var v = value;
                if (!RequiredUserName)
                    v = "";
                else if (New.UserName != v)
                {
                    New.UserName = v;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Demo));
                }
            }
        }

        public string Password
        {
            get => New.Password;
            set
            {
                var v = value;
                if (!RequiredPassword)
                    v = "";
                else if (New.Password != v)
                {
                    New.Password = v;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Demo));
                }
            }
        }

        public string PrivateKey
        {
            get => New.PrivateKey;
            set
            {
                var v = value;
                if (!RequiredPrivateKey)
                    v = "";
                else if (New.PrivateKey != v)
                {
                    New.PrivateKey = v;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Demo));
                }
            }
        }

        public string Demo => ExePath + " " + New.GetDemoArguments();


        public bool? IsPingBeforeConnect
        {
            get => New.IsPingBeforeConnect;
            set
            {
                if (New.IsPingBeforeConnect != value)
                {
                    New.IsPingBeforeConnect = value;
                    RaisePropertyChanged();
                }
            }
        }


        public bool? IsAutoAlternateAddressSwitching
        {
            get => New.IsAutoAlternateAddressSwitching;
            set
            {
                if (New.IsAutoAlternateAddressSwitching != value)
                {
                    New.IsAutoAlternateAddressSwitching = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ObservableCollection<Credential> AlternateCredentials
        {
            get => New.AlternateCredentials;
            set
            {
                if (New.AlternateCredentials != value)
                {
                    New.AlternateCredentials = value;
                    RaisePropertyChanged();
                }
            }
        }




        public string HintHostName { get; set; }
        public string HintPort { get; set; }
        public string HintUserName { get; set; }
        public string HintPassword { get; set; }
        public string HintPrivateKey { get; set; }


        public bool RequiredHostName { get; set; } = false;
        public bool RequiredPort { get; set; } = false;
        public bool RequiredUserName { get; set; } = false;
        public bool RequiredPassword { get; set; } = false;
        public bool RequiredPrivateKey { get; set; } = false;
        public bool RequiredHostNameIsNullable { get; set; } = false;
        public bool RequiredPortIsNullable { get; set; } = false;
        public bool RequiredUserNameIsNullable { get; set; } = false;
        public bool RequiredPasswordIsNullable { get; set; } = false;
        public bool RequiredPrivateKeyIsNullable { get; set; } = false;
        private void CheckMacroRequirement()
        {
            foreach (var arg in New.ArgumentList)
            {
                arg.PropertyChanged -= ArgOnPropertyChanged;
                arg.PropertyChanged += ArgOnPropertyChanged;
            }
            var tmpRequiredHostName = false;
            var tmpRequiredPort = false;
            var tmpRequiredUserName = false;
            var tmpRequiredPassword = false;
            var tmpRequiredPrivateKey = false;
            foreach (var argument in New.ArgumentList)
            {
                if (!tmpRequiredHostName && argument.Value.IndexOf(ProtocolBaseWithAddressPort.MACRO_HOST_NAME, StringComparison.Ordinal) >= 0)
                {
                    tmpRequiredHostName = true;
                    RequiredHostNameIsNullable = argument.IsNullable;
                    HintHostName = argument.HintDescription;
                }
                if (!tmpRequiredPort && argument.Value.IndexOf(ProtocolBaseWithAddressPort.MACRO_PORT, StringComparison.Ordinal) >= 0)
                {
                    tmpRequiredPort = true;
                    RequiredPortIsNullable = argument.IsNullable;
                    HintPort = argument.HintDescription;
                }
                if (!tmpRequiredUserName && argument.Value.IndexOf(ProtocolBaseWithAddressPortUserPwd.MACRO_USERNAME, StringComparison.Ordinal) >= 0)
                {
                    tmpRequiredUserName = true;
                    RequiredUserNameIsNullable = argument.IsNullable;
                    HintUserName = argument.HintDescription;
                }
                if (!tmpRequiredPassword && argument.Value.IndexOf(ProtocolBaseWithAddressPortUserPwd.MACRO_PASSWORD, StringComparison.Ordinal) >= 0)
                {
                    tmpRequiredPassword = true;
                    RequiredPasswordIsNullable = argument.IsNullable;
                    HintPassword = argument.HintDescription;
                }
                if (!tmpRequiredPrivateKey && argument.Value.IndexOf(ProtocolBaseWithAddressPortUserPwd.MACRO_PRIVATE_KEY_PATH, StringComparison.Ordinal) >= 0)
                {
                    tmpRequiredPrivateKey = true;
                    RequiredPrivateKeyIsNullable = argument.IsNullable;
                    HintPrivateKey = argument.HintDescription;
                }
            }

            if (RequiredHostName && !tmpRequiredHostName) Address = "";
            if (RequiredPort && !tmpRequiredPort) Port = "";
            if (RequiredUserName && !tmpRequiredUserName) UserName = "";
            if (RequiredPassword && !tmpRequiredPassword) Password = "";
            if (RequiredPrivateKey && !tmpRequiredPrivateKey) PrivateKey = "";
            RequiredHostName = tmpRequiredHostName;
            RequiredPort = tmpRequiredPort;
            RequiredUserName = tmpRequiredUserName;
            RequiredPassword = tmpRequiredPassword;
            RequiredPrivateKey = tmpRequiredPrivateKey;
            RaisePropertyChanged(nameof(RequiredHostName));
            RaisePropertyChanged(nameof(RequiredPort));
            RaisePropertyChanged(nameof(RequiredUserName));
            RaisePropertyChanged(nameof(RequiredPassword));
            RaisePropertyChanged(nameof(RequiredPrivateKey));
            RaisePropertyChanged(nameof(HintHostName));
            RaisePropertyChanged(nameof(HintPort));
            RaisePropertyChanged(nameof(HintUserName));
            RaisePropertyChanged(nameof(HintPassword));
            RaisePropertyChanged(nameof(HintPrivateKey));
            RaisePropertyChanged(nameof(Demo));
        }

        #region IDataErrorInfo
        [JsonIgnore] public string Error => "";

        [JsonIgnore]
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Address):
                        {
                            if (!RequiredHostNameIsNullable && string.IsNullOrWhiteSpace(Address))
                            {
                                return $"`{IoC.Get<ILanguageService>().Translate("Hostname")}` {IoC.Get<ILanguageService>().Translate(LanguageService.CAN_NOT_BE_EMPTY)}";
                            }
                            break;
                        }
                    case nameof(Port):
                        {
                            if (RequiredPort)
                            {
                                if (!RequiredPortIsNullable && string.IsNullOrWhiteSpace(Port))
                                    return $"`{IoC.Get<ILanguageService>().Translate("Port")}` {IoC.Get<ILanguageService>().Translate(LanguageService.CAN_NOT_BE_EMPTY)}";
                                if (!long.TryParse(Port, out _))
                                    return "TXT: not a number";
                            }
                            break;
                        }
                    case nameof(UserName):
                        {
                            if (!RequiredUserNameIsNullable && string.IsNullOrWhiteSpace(UserName))
                            {
                                return $"`{IoC.Get<ILanguageService>().Translate("User")}` {IoC.Get<ILanguageService>().Translate(LanguageService.CAN_NOT_BE_EMPTY)}";
                            }
                            break;
                        }
                    case nameof(Password):
                        {
                            if (!RequiredPasswordIsNullable && string.IsNullOrWhiteSpace(Password))
                            {
                                return $"`{IoC.Get<ILanguageService>().Translate("Password")}` {IoC.Get<ILanguageService>().Translate(LanguageService.CAN_NOT_BE_EMPTY)}";
                            }
                            break;
                        }
                        //default:
                        //    {
                        //        return New[columnName];
                        //    }
                }
                return "";
            }
        }
        #endregion




        private RelayCommand? _cmdSelectExePath;
        [JsonIgnore]
        public RelayCommand CmdSelectExePath
        {
            get
            {
                return _cmdSelectExePath ??= new RelayCommand((o) =>
                {
                    string initPath;
                    try
                    {
                        initPath = new FileInfo(ExePath).DirectoryName!;
                    }
                    catch (Exception)
                    {
                        initPath = Environment.CurrentDirectory;
                    }

                    var path = SelectFileHelper.OpenFile(filter: "Exe|*.exe", initialDirectory: initPath, currentDirectoryForShowingRelativePath: Environment.CurrentDirectory);
                    if (path == null) return;
                    ExePath = path;
                });
            }
        }


        private RelayCommand? _cmdPreview;
        [JsonIgnore]
        public RelayCommand CmdPreview
        {
            get
            {
                return _cmdPreview ??= new RelayCommand((o) =>
                {
                    MessageBoxHelper.Info(ExePath + " " + New.GetDemoArguments(), ownerViewModel: IoC.Get<MainWindowViewModel>());
                });
            }
        }

        private RelayCommand? _cmdTest;
        [JsonIgnore]
        public RelayCommand CmdTest
        {
            get
            {
                return _cmdTest ??= new RelayCommand((o) =>
                {
                    try
                    {
                        Process.Start(ExePath, New.GetArguments());
                    }
                    catch (Exception e)
                    {
                        MessageBoxHelper.ErrorAlert(e.Message);
                    }
                });
            }
        }
    }
}
