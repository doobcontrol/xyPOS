using xy.Comm;

namespace xy.Barcode
{
    public class PhScanner
    {
        private IXyComm myIXyComm;
        private PhScannerRequestHandler _phScannerRequestHandler;
        event EventHandler<XyFileIOEventArgs> _xyFileIOEventHandler;

        public PhScanner(
            string localIp, int localPort,
            string targetIp, int targetPort,
            PhScannerRequestHandler phScannerRequestHandler,
            EventHandler<XyFileIOEventArgs> xyFileIOEventHandler
            )
        {
            myIXyComm = new XyUdpComm(
                localIp, localPort,
                targetIp, targetPort,
                XyCommRequestHandler);
            _phScannerRequestHandler = phScannerRequestHandler;
            _xyFileIOEventHandler = xyFileIOEventHandler;
            myIXyComm.startListen();
        }
        public void clean()
        {
            if (myIXyComm != null)
            {
                myIXyComm.stopListen();
            }
        }

        private string XyCommRequestHandler(string receivedString)
        {
            CommResult commResult;

            try
            {
                CommData commData = CommData.fromReceivedString(receivedString);
                commResult = new CommResult(commData);
                commResult.cmdSucceed = true;

                switch (commData.cmd)
                {
                    case PhScannerCmd.Register:
                        //register target ip and port
                        Dictionary<string, string> setDic = new Dictionary<string, string>();
                        setDic[XyUdpCommTargetSetPar.ip.ToString()]
                            = commData.cmdParDic[CmdPar.ip.ToString()];
                        setDic[XyUdpCommTargetSetPar.port.ToString()]
                            = commData.cmdParDic[CmdPar.port.ToString()];
                        myIXyComm.set(setDic);

                        _phScannerRequestHandler(commData, commResult);

                        commResult.resultDataDic.Add(
                            CmdPar.returnMsg.ToString(),
                            "Regist ok");
                        break;
                    case PhScannerCmd.GetInitFolder:
                        _phScannerRequestHandler(commData, commResult);
                        break;
                    case PhScannerCmd.GetFolder:
                        _phScannerRequestHandler(commData, commResult);
                        break;
                    case PhScannerCmd.GetFile:
                        _phScannerRequestHandler(commData, commResult);

                        _ = Task.Run(async () =>
                        {
                            _xyFileIOEventHandler(
                            this,
                            new XyFileIOEventArgs(
                                FileIOEventType.start,
                                XyCommFileSendReceive.Send,
                                int.Parse(commResult.resultDataDic[CmdPar.fileLength.ToString()]),
                                0
                                )
                            );

                            CancellationTokenSource succeedTokenSource = new CancellationTokenSource();
                            succeedTokenSourceDic.Add(commData.cmdID, succeedTokenSource);
                            await myIXyComm.sendFile(
                                commData.cmdParDic[CmdPar.targetFile.ToString()],
                                commResult.resultDataDic[CmdPar.fileLength.ToString()],
                                commData.cmdParDic[CmdPar.streamReceiverPar.ToString()],
                                succeedTokenSource.Token,
                                FileEventHandler);
                            succeedTokenSource.Dispose();
                            succeedTokenSourceDic.Remove(commData.cmdID);

                            _xyFileIOEventHandler(
                                this,
                                new XyFileIOEventArgs(
                                    FileIOEventType.end,
                                    XyCommFileSendReceive.Send,
                                    0,
                                    0
                                    )
                                );
                        });

                        break;
                    case PhScannerCmd.SendFile:
                        _phScannerRequestHandler(commData, commResult);

                        _ = Task.Run(async () => {
                            _xyFileIOEventHandler(
                                this,
                                new XyFileIOEventArgs(
                                    FileIOEventType.start,
                                    XyCommFileSendReceive.Receive,
                                    int.Parse(commData.cmdParDic[CmdPar.fileLength.ToString()]),
                                    0
                                    )
                                );

                            await myIXyComm.prepareStreamReceiver(
                                commData.cmdParDic[CmdPar.targetFile.ToString()],
                                commData.cmdParDic[CmdPar.fileLength.ToString()],
                                commResult.resultDataDic[CmdPar.streamReceiverPar.ToString()],
                                FileEventHandler);

                            comfirmReceiveFileSucceed(commData.cmdID);

                            _xyFileIOEventHandler(
                                this,
                                new XyFileIOEventArgs(
                                    FileIOEventType.end,
                                    XyCommFileSendReceive.Receive,
                                    0,
                                    0
                                    )
                                );
                        });

                        break;
                    case PhScannerCmd.SendFileSucceed:
                        d("succeedTokenSourceDic[commData.cmdID]?.Cancel();");
                        string sendFileCmdID =
                            commData.cmdParDic[CmdPar.sendFileCmdID.ToString()];
                        if (succeedTokenSourceDic.ContainsKey(sendFileCmdID))
                        {
                            d("succeedTokenSourceDic[commData.cmdID]?.Cancel():"
                                + sendFileCmdID);
                            succeedTokenSourceDic[sendFileCmdID]?.Cancel();
                            succeedTokenSourceDic.Remove(sendFileCmdID);
                        }

                        break;
                    case PhScannerCmd.SendText:
                        commData.cmdParDic[CmdPar.text.ToString()] =
                            commData.cmdParDic[CmdPar.text.ToString()];
                        _phScannerRequestHandler(commData, commResult);
                        break;

                    default:
                        commResult.resultDataDic.Add(
                            CmdPar.errorMsg.ToString(),
                            "Cannot hand this command");
                        commResult.cmdSucceed = false;
                        break;
                }
            }
            catch (Exception e)
            {
                commResult = null;
            }

            string retStr = null;
            if (commResult != null)
            {
                retStr = commResult.toSendString();
            }
            return retStr;
        }

        private void FileEventHandler(object? sender, XyCommFileEventArgs e)
        {
            _xyFileIOEventHandler(
                this,
                new XyFileIOEventArgs(
                    (e.Length > e.Progress) ? FileIOEventType.progress :
                        FileIOEventType.end,
                    e.FileSendReceive,
                    e.Length,
                    e.Progress
                    )
                );
        }

        private async Task<CommResult> XyCommRequestAsync(CommData commData)
        {
            string resultString = null;

            try
            {
                resultString = await myIXyComm.sendForResponseAsync(
                    commData.toSendString()
                    );
            }
            catch (XySoftCommException xse)
            {
                switch (xse.ErrorCode)
                {
                    case XyCommErrorCode.TimedOut:
                        throw new PhScannerException(
                            PhScannerCommErrorCode.TimedOut,
                            "TimedOut");
                    default:
                        throw new PhScannerException(
                            PhScannerCommErrorCode.OtherError,
                            "Net work error");
                }
            }

            return CommResult.fromReturnString(resultString, commData);
        }

        public async Task<CommResult> Register(
            string ip, int port, string hostName
            )
        {
            CommData commData = new CommData(PhScannerCmd.Register);
            commData.cmdParDic.Add(CmdPar.ip.ToString(), ip);
            commData.cmdParDic.Add(CmdPar.port.ToString(), port.ToString());
            commData.cmdParDic.Add(CmdPar.hostName.ToString(), hostName);

            return await XyCommRequestAsync(commData);
        }

        public async Task<CommResult> GetInitFolder()
        {
            CommData commData = new CommData(PhScannerCmd.GetInitFolder);

            CommResult commResult = await XyCommRequestAsync(commData);

            return commResult;
        }

        public async Task<CommResult> GetFolder(string path)
        {
            CommData commData = new CommData(PhScannerCmd.GetFolder);
            commData.cmdParDic.Add(CmdPar.requestPath.ToString(), path);

            return await XyCommRequestAsync(commData);
        }

        public async Task GetFile(
            string receiveFile,
            string targetFile,
            string streamReceiverPar
            )
        {
            CommData commData = new CommData(PhScannerCmd.GetFile);
            commData.cmdParDic.Add(CmdPar.targetFile.ToString(), targetFile);
            commData.cmdParDic.Add(CmdPar.streamReceiverPar.ToString(),
                streamReceiverPar);

            CommResult commResult = await XyCommRequestAsync(commData);

            _xyFileIOEventHandler(
                this,
                new XyFileIOEventArgs(
                    FileIOEventType.start,
                    XyCommFileSendReceive.Receive,
                    int.Parse(commResult.resultDataDic[CmdPar.fileLength.ToString()]),
                    0
                    )
                );

            await myIXyComm.prepareStreamReceiver(
                receiveFile,
                commResult.resultDataDic[CmdPar.fileLength.ToString()],
                streamReceiverPar,
                FileEventHandler);

            comfirmReceiveFileSucceed(commData.cmdID);

            _xyFileIOEventHandler(
                this,
                new XyFileIOEventArgs(
                    FileIOEventType.end,
                    XyCommFileSendReceive.Receive,
                    0,
                    0
                    )
                );
        }

        private Dictionary<string, CancellationTokenSource> succeedTokenSourceDic
            = new Dictionary<string, CancellationTokenSource>();
        public async Task SendFile(string sendFile, string targetFile)
        {
            string fileLengthStr = new FileInfo(sendFile).Length.ToString();
            CommData commData = new CommData(PhScannerCmd.SendFile);
            commData.cmdParDic.Add(CmdPar.targetFile.ToString(), targetFile);
            commData.cmdParDic.Add(CmdPar.fileLength.ToString(),
                new FileInfo(sendFile).Length.ToString());

            d("request send file ...");
            CommResult commResult = await XyCommRequestAsync(commData);
            d("request confirmed, start send ...");

            _xyFileIOEventHandler(
                this,
                new XyFileIOEventArgs(
                    FileIOEventType.start,
                    XyCommFileSendReceive.Send,
                    int.Parse(fileLengthStr),
                    0
                    )
                );

            CancellationTokenSource succeedTokenSource = new CancellationTokenSource();
            succeedTokenSourceDic.Add(commData.cmdID, succeedTokenSource);
            await myIXyComm.sendFile(
                sendFile,
                fileLengthStr,
                commResult.resultDataDic[CmdPar.streamReceiverPar.ToString()],
                succeedTokenSource.Token,
                FileEventHandler);
            d("send done");
            succeedTokenSource.Dispose();
            succeedTokenSourceDic.Remove(commData.cmdID);

            _xyFileIOEventHandler(
                this,
                new XyFileIOEventArgs(
                    FileIOEventType.end,
                    XyCommFileSendReceive.Send,
                    0,
                    0
                    )
                );
        }

        public async Task<CommResult> SendText(string sendText)
        {
            CommData commData = new CommData(PhScannerCmd.SendText);
            commData.cmdParDic.Add(CmdPar.text.ToString(), sendText);

            return await XyCommRequestAsync(commData);
        }

        private void comfirmReceiveFileSucceed(string cmdID)
        {
            CommData commData = new CommData(PhScannerCmd.SendFileSucceed);
            commData.cmdParDic.Add(CmdPar.sendFileCmdID.ToString(), cmdID);

            XyCommRequestAsync(commData);
        }

        //debug
        static public void d(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }
    }
    public enum PhScannerCmd
    {
        Register,
        GetInitFolder,
        GetFolder,
        GetFile,
        SendFile,
        SendFileSucceed,
        SendText
    }
    public enum CmdPar
    {
        cmd,
        cmdID,
        sendFileCmdID,
        cmdSucceed,
        ip,
        port,
        hostName,
        requestPath,
        targetFile,
        text,
        fileLength,
        streamReceiverPar,
        folders,
        files,
        returnMsg,
        errorMsg
    }
    public enum PhScannerCommErrorCode
    {
        TimedOut = 10060,
        OtherError = 0
    }

    public delegate void
        PhScannerRequestHandler(CommData commData, CommResult commResult);

    public class XyFileIOEventArgs : EventArgs
    {
        public XyFileIOEventArgs(
            FileIOEventType type,
            XyCommFileSendReceive fileSendReceive,
            long length,
            long progress)
        {
            this.FileSendReceive = fileSendReceive;
            Length = length;
            Progress = progress;
            Type = type;
        }

        public XyCommFileSendReceive FileSendReceive { get; private set; }
        public long Length { get; private set; }
        public long Progress { get; private set; }
        public FileIOEventType Type { get; private set; }
    }
    public enum FileIOEventType
    {
        start,
        end,
        progress
    }
}
