﻿using System;
using PRM.Core.External.KiTTY;
using PRM.Core.Model;
using PRM.Core.Protocol;
using PRM.Core.Protocol.FileTransmit.FTP;
using PRM.Core.Protocol.FileTransmit.SFTP;
using PRM.Core.Protocol.Putty.SSH;
using PRM.Core.Protocol.Putty.Telnet;
using PRM.Core.Protocol.RDP;
using PRM.Core.Protocol.VNC;
using PRM.View.ProtocolHosts;
using VncHost = PRM.View.ProtocolHosts.VncHost;

namespace PRM.Model
{
    public static class ProtocolHostFactory
    {
        public static HostBase Get(PrmContext context, ProtocolServerBase server, double width = 0, double height = 0)
        {
            switch (server)
            {
                case ProtocolServerRDP _:
                    {
                        var host = new AxMsRdpClient09Host(context, server, width, height);
                        return host;
                    }
                case ProtocolServerSSH ssh:
                    {
                        var host = new IntegrateHost(context, ssh, ssh.GetExeFullPath(), ssh.GetExeArguments(context))
                        {
                            // TODO 读取 Kitty 主题
                            RunBeforeConnect = () => ssh.SetKittySessionConfig(14, "", ssh.PrivateKey),
                            RunAfterConnected = () => ssh.DelKittySessionConfig()
                        };
                        return host;
                    }
                case ProtocolServerTelnet telnet:
                    {
                        var host = new IntegrateHost(context, telnet, telnet.GetExeFullPath(), telnet.GetExeArguments(context))
                        {
                            RunBeforeConnect = () => telnet.SetKittySessionConfig(14, "", ""),
                            RunAfterConnected = () => telnet.DelKittySessionConfig()
                        };
                        return host;
                    }
                case ProtocolServerVNC vnc:
                    {
                        var host = new VncHost(context, vnc);
                        return host;
                    }
                case ProtocolServerSFTP sftp:
                    {
                        var host = new FileTransmitHost(context, sftp);
                        return host;
                        //var host2 = new IntegrateHost(context, sftp, @"C:\Program Files (x86)\WinSCP\WinSCP.exe", $@"sftp://{sftp.UserName}:{context.DbOperator.DecryptOrReturnOriginalString(sftp.Password)}@{sftp.Address}:{sftp.GetPort()}");
                        //return host2;
                    }
                case ProtocolServerFTP ftp:
                    {
                        var host = new FileTransmitHost(context, ftp);
                        return host;
                    }
                default:
                    throw new NotImplementedException($"Host of {server.GetType()} is not implemented");
            }
        }

        public static bool IsConnWithFullScreen(this ProtocolServerBase server)
        {
            switch (server)
            {
                case ProtocolServerRDP rdp:
                    {
                        if (rdp.RdpFullScreenFlag == ERdpFullScreenFlag.EnableFullAllScreens)
                            return true;
                        if (rdp.IsConnWithFullScreen == true)
                            return true;
                        return rdp.AutoSetting?.FullScreenLastSessionIsFullScreen ?? false;
                    }
                default:
                    return false;
            }
        }
    }
}