using System.Net.Sockets;
using System.Net;
using xy.Barcode;
using System.Windows.Forms;
using xy.Comm;
using xy.Db;
using System.Data;
using xyDbSample;
using xy.Db.SQLite64;
using xyPOSOrm;
using xy.ORM;

namespace xyBarcodeTest
{
    public partial class Form1 : Form
    {
        string dbServer = "localhost";
        string dbName = "xyPos";
        string dbUser = "xyPosUser";
        string dbPassword = "xyPosUserPassword";
        private DbService dbService;

        string title = "xyBarcodeTest";
        int Port = 13265;

        PhScanner? phScanner;
        public Form1()
        {
            InitializeComponent();
            Text = title;

            comboBox1.SelectedIndexChanged += (s, e) =>
            {
                ComboBox cb = comboBox1;
                if (cb.SelectedIndex != -1)
                {
                    setLocalIpEndPoint(
                        cb.Items[cb.SelectedIndex].ToString()
                        );
                }
            };

            getLocalIp();

            string ConnectionString =
                xyCfg.get(xyCfg.dT_SQLite, xyCfg.connStr);
            var dbAccess = new SQLite64DbAccess();
            dbService = new DbService(
                ConnectionString, dbAccess);
            dbService.openAsync();
            BaseModel.DefaultDbService = dbService;
        }

        private void getLocalIp()
        {
            string host = Dns.GetHostName();

            // Getting ip address using host name 
            IPHostEntry ip = Dns.GetHostEntry(host);

            foreach (var item in ip.AddressList)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    comboBox1.Items.Add(item.ToString());
                }
            }
        }
        private void setLocalIpEndPoint(string localIP)
        {
            if (phScanner != null)
            {
                phScanner.clean();
            }
            phScanner = new PhScanner(
                localIP, Port,
                null, 0, DroidFolderRequestHandler,
                FileEventHandler
                );
        }

        private async void DroidFolderRequestHandler(CommData commData, CommResult commResult)
        {
            try
            {
                switch (commData.cmd)
                {
                    case PhScannerCmd.Register:
                        Text = title + " - "
                            + commData.cmdParDic[CmdPar.hostName.ToString()];
                        break;
                    case PhScannerCmd.SendText:
                        string barcode =
                            commData.cmdParDic[CmdPar.text.ToString()];
                        DataTable dt = 
                            await GoodsDef.i.SelectByField(
                                GoodsDef.GoodsBarcode, barcode);
                        if(dt.Rows.Count == 0)
                        {
                            textBox1.Text = "No data";
                            return;
                        }
                        DataRow row = dt.Rows[0];
                        textBox1.Text = "";
                        textBox1.Text += GoodsDef.GoodsBarcode + ": "
                            + row[GoodsDef.GoodsBarcode] + "\r\n";
                        textBox1.Text += GoodsDef.GoodsName + ": "
                            + row[GoodsDef.GoodsName] + "\r\n";
                        textBox1.Text += GoodsDef.GoodsSpec + ": "
                            + row[GoodsDef.GoodsSpec] + "\r\n";
                        textBox1.Text += GoodsDef.GoodsUnit + ": "
                            + row[GoodsDef.GoodsUnit] + "\r\n";
                        break;
                    case PhScannerCmd.SendFile:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //XyLog.log(e);
            }
        }

        private void FileEventHandler(object? sender, XyFileIOEventArgs e)
        {
        }

    }
}
