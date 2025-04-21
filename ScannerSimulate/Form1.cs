using System.IO;
using xy.Barcode;

namespace ScannerSimulate
{
    public partial class Form1 : Form
    {
        string title = "ScannerSimulate";
        PhScanner? phScanner;

        string localIP = "192.168.168.129";
        int localPort = 13267;
        //stream receiver listen port for other side to connect
        string streamReceiverPar = "13268";

        string remoteIP = "192.168.168.129"; //"192.168.168.129"  "192.168.168.1"
        int remotePort = 13265;

        public Form1()
        {
            InitializeComponent();
            Text = title;
        }
        private void showMsg(string msg)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() => { showMsg(msg); }));
            }
            else
            {
                textBox1.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - "
                    + msg + Environment.NewLine);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            showMsg("start regist бнбн");

            phScanner = new PhScanner(
                localIP, localPort,
                remoteIP, remotePort, DroidFolderRequestHandler,
                FileEventHandler
                );

            try
            {
                CommResult registResult = await phScanner.Register(
                    localIP,
                    localPort,
                    System.Environment.MachineName
                    );

                if (registResult.errorCmdID)
                {
                    showMsg("Comm error");
                }
                else if (!registResult.cmdSucceed)
                {
                    showMsg("Regist error: " + registResult.resultDataDic[
                        CmdPar.errorMsg.ToString()
                        ]);
                }
                else
                {
                    showMsg(registResult.resultDataDic[
                        CmdPar.returnMsg.ToString()
                        ]);
                }
                button1.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " - " + ex.StackTrace);
                //xyPtoPEnd.clean();
            }
        }
        private void DroidFolderRequestHandler(CommData commData, CommResult commResult)
        {
            showMsg("in request: " + commData.cmd.ToString());
            string[] subdirectoryEntries;
            string folderStr = "";
            string[] fileEntries;
            string fileStr = "";
            switch (commData.cmd)
            {
                case PhScannerCmd.GetInitFolder:

                    break;
                case PhScannerCmd.GetFolder:

                    break;
                case PhScannerCmd.GetFile:

                    break;
                case PhScannerCmd.SendFile:

                    break;
                case PhScannerCmd.SendText:

                    break;

                default:
                    break;
            }
        }

        private void FileEventHandler(object? sender, XyFileIOEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            phScanner?.SendText(textBox2.Text); ;
        }
    }
}
