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
            openFileDialog.Filter = "CSV Files|*.csv;*.csv";
            openFileDialog.Title = "Select a CSV file";

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
                    await Task.Run(
                    async () =>
                    {
                        IEnumerable<string> Lines = File.ReadLines(filePath);
                        progress.Report(Lines.Count());

                        int i = 0;
                        Dictionary<string, string?> rowDict;
                        var recordList = new List<Dictionary<string, string>>();
                        string[] buffer = null;
                        var barCodeCheckList = new List<string>();
                        foreach (string line in Lines)
                        {
                            if (i == 0)
                            {
                                // Skip the header line
                                i++;
                                continue;
                            }

                            string[] rows = line.Split(",");

                            if (rows.Length > 12 && buffer == null)
                            {
                                //there are "," in the data
                                List<string> rowList = new List<string>();
                                bool conn = false;
                                string connStr = "";
                                for (int j = 0; j < rows.Length; j++)
                                {
                                    if(!conn)
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
                            if (rows.Length < 12)
                            {
                                buffer = rows;
                                XyLog.log(rows[1] + " Only " + rows.Length + " items");
                                continue;
                            }

                            if (barCodeCheckList.Contains(rows[1]))
                            {
                                //Skip duplicate record
                                XyLog.log(rows[1] + " duplicate BarCode");
                                continue;
                            }
                            if(barCodeCheckList.Count > 100)
                            {
                                barCodeCheckList.RemoveAt(0);
                            }
                            barCodeCheckList.Add(rows[1]);

                            for (int itemIndex = 0; itemIndex < rows.Length; itemIndex++)
                            {
                                rows[itemIndex] = rows[itemIndex].Replace("\"", "");
                                rows[itemIndex] = rows[itemIndex].Replace("'", "''"); 
                                rows[itemIndex] = rows[itemIndex].Trim();
                            }

                            rowDict = new Dictionary<string, string?>();
                            rowDict.Add(GoodsDef.fID, i.ToString("0000000"));
                            rowDict.Add(GoodsDef.GoodsBarcode, rows[1]);
                            rowDict.Add(GoodsDef.GoodsName, rows[2]);
                            rowDict.Add(GoodsDef.GoodsSpec, rows[3]);
                            rowDict.Add(GoodsDef.GoodsUnit, rows[4]);


                            StringBuilder sb = new StringBuilder(rows[5].Length);
                            foreach (char c in rows[5])
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
                                if (sb.Length>0 && !(c == '0'
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
                            rows[5] = sb.ToString();
                            if (
                            rows[5] != ""
                            && rows[5] != "NULL"
                            )
                            {
                                rowDict.Add(GoodsDef.GoodsPrice, rows[5]);
                            }
                            else
                            {
                                rowDict.Add(GoodsDef.GoodsPrice, null);
                            }

                            rowDict.Add(GoodsDef.GoodsBrand, rows[6]);
                            rowDict.Add(GoodsDef.GoodsSupplier, rows[7]);
                            rowDict.Add(GoodsDef.GoodsMadeIn, rows[8]);
                            recordList.Add(rowDict);

                            i++;
                            if (recordList.Count >= 1000)
                            {
                                await GoodsDef.i.Insert(recordList);
                                recordList.Clear();
                                if (canecel)
                                {
                                    break;
                                }
                                progress.Report(i);
                            }
                        }
                        XyLog.log("Importing " + i + " records");
                    }
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading file: " + ex.Message);
                    XyLog.log(ex);
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
    }
}
