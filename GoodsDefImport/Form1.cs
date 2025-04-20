using System.Text;
using xyPOSOrm;
using xySoft.log;

namespace GoodsDefImport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            lbDataFile.Text = null;
            lbProgress.Visible = false;
            lbStatus.Visible = false;
            progressBar1.Visible = false;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files|*.csv;*.csv|TEXT Files|*.txt;*.txt";
            openFileDialog.Title = "Select data file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                lbDataFile.Text = openFileDialog.FileName;
            }
        }

        bool canecel = false;
        private async void btnImport_Click(object sender, EventArgs e)
        {
            if (_inWorking)
            {
                canecel = true;
            }
            else
            {
                if (lbDataFile.Text == null || lbDataFile.Text == "")
                {
                    MessageBox.Show("Please select a data file.");
                    return;
                }
                setWorkingStatus(true);
                lbProgress.Text = "0/0";
                lbStatus.Text = "Opening data file ...";
                canecel = false;

                string filePath = lbDataFile.Text;
                DateTime startTime = DateTime.Now;
                // Read the CSV file and display the contents in the DataGridView
                try
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = 0;

                    IProgress<int> progress 
                        = new Progress<int>(
                        value => {
                            if (progressBar1.Maximum == 0)
                            {
                                progressBar1.Maximum = value;
                                lbStatus.Text = "Importing ...";
                            }
                            else
                            {
                                progressBar1.Value = value;
                                TimeSpan ts = DateTime.Now - startTime;
                                lbProgress.Text =
                                    value + "/" + progressBar1.Maximum + " - "
                                    + new TimeSpan(ts.Hours,ts.Minutes,ts.Seconds).ToString("g");
                            }
                        });

                    var controlPars = new Dictionary<string, string>();
                    string filename = Path.GetFileName(filePath);
                    var importMap = new Dictionary<string, int>();
                    switch (filename)
                    {
                        case "barcodes.csv":
                            controlPars.Add("SkipTheHeader", true.ToString());
                            controlPars.Add("Split", ",");
                            controlPars.Add("RowLength", "12");
                            controlPars.Add("Encoding", "UTF-8");
                            importMap.Add(GoodsDef.GoodsBarcode, 1);
                            importMap.Add(GoodsDef.GoodsName, 2);
                            importMap.Add(GoodsDef.GoodsSpec, 3);
                            importMap.Add(GoodsDef.GoodsUnit, 4);
                            importMap.Add(GoodsDef.GoodsPrice, 5);
                            importMap.Add(GoodsDef.GoodsBrand, 6);
                            importMap.Add(GoodsDef.GoodsSupplier, 7);
                            importMap.Add(GoodsDef.GoodsMadeIn, 8);
                            break;
                        case "YT_1.2M.txt":
                            controlPars.Add("SkipTheHeader", false.ToString());
                            controlPars.Add("Split", "\t");
                            controlPars.Add("RowLength", "5");
                            controlPars.Add("Encoding", "GB2312");
                            importMap.Add(GoodsDef.GoodsBarcode, 3);
                            importMap.Add(GoodsDef.GoodsName, 0);
                            importMap.Add(GoodsDef.GoodsSpec, 1);
                            importMap.Add(GoodsDef.GoodsUnit, 2);
                            break;
                        default:
                            MessageBox.Show("Unknown file format");
                            return;
                    }

                    await ImportFromFile(
                        filePath, progress, controlPars, importMap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading file: " + ex.Message);
                    XyLog.log(ex);
                    if(ex.InnerException != null)
                    {
                        XyLog.log(ex.InnerException);
                    }
                }
                setWorkingStatus(false);
            }
        }

        bool _inWorking = false;
        private void setWorkingStatus(bool inWorking)
        {
            if (inWorking)
            {
                progressBar1.Visible = true;
                lbProgress.Visible = true;
                lbStatus.Visible = true;
                btnSelectFile.Enabled = false;
                btnImport.Text = "Cancel";

                ControlBox = false;
            }
            else
            {
                progressBar1.Visible = false;
                lbProgress.Visible = false;
                lbStatus.Visible = false;
                btnSelectFile.Enabled = true;
                btnImport.Text = "Import";

                ControlBox = true;
            }
            _inWorking = inWorking;
        }

        private async Task ImportFromFile(
            string filePath, 
            IProgress<int> progress,
            Dictionary<string, string> controlPars,
            Dictionary<string, int> importMap
            )
        {
            await Task.Run(
                async () =>
                {
                    IEnumerable<string> Lines = File.ReadLines(filePath,
                        Encoding.GetEncoding(controlPars["Encoding"]));
                    progress.Report(Lines.Count());
                    int startID =
                        int.Parse((await GoodsDef.i.SelectMax(GoodsDef.fID)) ?? "0")
                        + 1;
                    int i = 0;
                    Dictionary<string, string?> rowDict;
                    var recordList = new List<Dictionary<string, string>>();
                    string[] buffer = null;
                    var barCodeCheckList = new List<string>();
                    int rowLength = int.Parse(controlPars["RowLength"]);
                    int bIndex = importMap[GoodsDef.GoodsBarcode];
                    foreach (string line in Lines)
                    {
                        if (i == 0 && bool.Parse(controlPars["SkipTheHeader"]))
                        {
                            // Skip the header line
                            i++;
                            continue;
                        }

                        string[] rows = line.Split(controlPars["Split"]);

                        if (rows.Length > rowLength && buffer == null)
                        {
                            //there are "," in the data
                            List<string> rowList = new List<string>();
                            bool conn = false;
                            string connStr = "";
                            for (int j = 0; j < rows.Length; j++)
                            {
                                if (!conn)
                                {
                                    if (rows[j].StartsWith("\"")
                                        && !rows[j].EndsWith("\"")
                                    )
                                    {
                                        conn = true;
                                        connStr = rows[j];
                                    }
                                    else
                                    {
                                        rowList.Add(rows[j]);
                                    }
                                }
                                else
                                {
                                    connStr += rows[j];
                                    if (!rows[j].StartsWith("\"")
                                        && rows[j].EndsWith("\"")
                                    )
                                    {
                                        conn = false;
                                        rowList.Add(connStr);
                                    }
                                }
                            }
                            rows = rowList.ToArray();
                        }

                        //handle line break
                        if (buffer != null)
                        {
                            rows = buffer.Concat(rows).ToArray();
                            buffer = null;
                        }
                        if (rows.Length < rowLength)
                        {
                            buffer = rows;
                            if (rows.Length - 1 >= bIndex)
                            {
                                XyLog.log(rows[bIndex] + " Only " + rows.Length + " items");
                            }
                            else
                            {
                                string tStr = "";
                                for (int j = 0; j < rows.Length; j++)
                                {
                                    tStr += rows[j] + ",";
                                }
                                XyLog.log(tStr + " Only " + rows.Length + " items");
                            }
                            continue;
                        }

                        if (barCodeCheckList.Contains(rows[bIndex]))
                        {
                            //Skip duplicate record
                            XyLog.log(rows[bIndex] + " duplicate BarCode");
                            continue;
                        }
                        if (barCodeCheckList.Count > 100)
                        {
                            barCodeCheckList.RemoveAt(0);
                        }
                        barCodeCheckList.Add(rows[bIndex]);

                        for (int itemIndex = 0; itemIndex < rows.Length; itemIndex++)
                        {
                            rows[itemIndex] = rows[itemIndex].Replace("\"", "");
                            rows[itemIndex] = rows[itemIndex].Replace("'", "''");
                            rows[itemIndex] = rows[itemIndex].Replace("\0", "");
                            rows[itemIndex] = rows[itemIndex].Trim();
                        }

                        rowDict = new Dictionary<string, string?>();
                        rowDict.Add(GoodsDef.fID, (i + startID).ToString("000000000"));
                        foreach (var item in importMap)
                        {
                            string k = item.Key;
                            int v = item.Value;
                            if (k == GoodsDef.GoodsPrice)
                            {
                                rows[v] = getFloatString(rows[v]);
                            }
                            if (rows[v] != "" && rows[v] != "NULL")
                            {
                                rowDict.Add(k, rows[v]);
                            }
                            else
                            {
                                rowDict.Add(k, null);
                            }
                        }
                        recordList.Add(rowDict);

                        i++;
                        if (recordList.Count >= 1000)
                        {
                            try
                            {
                                await GoodsDef.i.Insert(recordList);
                            }
                            catch (Exception ex)
                            {
                                XyLog.log(ex);
                                if (ex.InnerException != null)
                                {
                                    XyLog.log(ex.InnerException);
                                }
                            }
                            //await GoodsDef.i.Insert(recordList);
                            recordList.Clear();
                            if (canecel)
                            {
                                break;
                            }
                            progress.Report(i);
                        }
                    }
                    if (recordList.Count >= 0)
                    {
                        await GoodsDef.i.Insert(recordList);
                        progress.Report(i);
                    }
                    XyLog.log("Importing " + i + " records");
                }
                );
        }
        private string getFloatString(string oStr)
        {
            if (oStr == null || oStr == "")
            {
                return null;
            }
            StringBuilder sb = new StringBuilder(oStr.Length);
            foreach (char c in oStr)
            {
                if (c == '0'
                || c == '1'
                || c == '2'
                || c == '3'
                || c == '4'
                || c == '5'
                || c == '6'
                || c == '7'
                || c == '8'
                || c == '9'
                || c == '.'
                )
                {
                    sb.Append(c);
                }
                if (sb.Length > 0 && !(c == '0'
                || c == '1'
                || c == '2'
                || c == '3'
                || c == '4'
                || c == '5'
                || c == '6'
                || c == '7'
                || c == '8'
                || c == '9'
                || c == '.'))
                {
                    break;
                }
            }
            return sb.ToString();
        }
    }
}
