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
                        List<string> barcodeList = new List<string>();
                        List<string> duplicatedList = new List<string>()
                        {
                            "6931875000120",
                            "6909320002586",
                            "6925436700321",
                            "6925212800016",
                            "6938979800700",
                            "6935766600123",
                            "6942729200061",
                            "6942729200061",
                            "6942729200061",
                            "6922131511139",
                            "6921413254375",
                            "6900230851707",
                            "6922043942557",
                            "6922043942588",
                            "6931919670425",
                            "6933386202284",
                            "6933386202284",
                            "8809083284220",
                            "6924671677504",
                            "6930134700313",
                            "6936047450017",
                            "6938979800014",
                            "6927547200479",
                            "6932595500013",
                            "6920384870416",
                            "6931735700276",
                            "6904691952027",
                            "6933386200099",
                            "6933386200099",
                            "6933386200099",
                            "6933386200099",
                            "6933386236036",
                            "6933386236036",
                            "6908791200736",
                            "8809180012689",
                            "6941960800016",
                            "6925212800085",
                            "6932072300037",
                            "6931522200163",
                            "6926831179682",
                            "6933386202024",
                            "6933386202055",
                            "6933386202055",
                            "6933386202055",
                            "6933386202055",
                            "6931841600064",
                            "6920384896164",
                            "6931735700177",
                            "8809145833243",
                            "6940645965880",
                            "6936158440037",
                            "6903071002499",
                            "6933386200068",
                            "6933386200068",
                            "6933386200068",
                            "6933386200068",
                            "6923738820587",
                            "6920742400019",
                            "6920270512772",
                            "6932618700956",
                            "6935755060488",
                            "6935387768868",
                            "6924679201657",
                            "6933386202154",
                            "6933386202154",
                            "4935421337304",
                            "6909246003773",
                            "6932618700871",
                            "6920384870263",
                            "6935420100136",
                            "6901382911103",
                            "6942257030017",
                            "6925212800030",
                            "6935420100105",
                            "6904059002388",
                            "6930391900020",
                            "6932618766693",
                            "6935420100143",
                            "6932088868705",
                            "6932088868705",
                            "6932088868705",
                            "6936601420005",
                            "6925212800061",
                            "6938979800175",
                            "6922043942373",
                            "5015600000003",
                            "6939219018015",
                            "3046000000005",
                            "6937765222788"
                        };
                        foreach (string line in Lines)
                        {
                            if (i == 0)
                            {
                                // Skip the header line
                                i++;
                                continue;
                            }
                            string[] rows = line.Split(',');
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
                            for(int itemIndex = 0; itemIndex < rows.Length; itemIndex++)
                            {
                                rows[itemIndex] = rows[itemIndex].Replace("\"", "");
                                rows[itemIndex] = rows[itemIndex].Replace("'", "''"); 
                                rows[itemIndex] = rows[itemIndex].Trim();
                            }
                            if(rows[1].Length < 8)
                            {
                                XyLog.log(rows[1] + " barcode length less than 8");
                                continue;
                            }

                            if(barcodeList.Contains(rows[1]))
                            {
                                XyLog.log(rows[1] + " barcode duplicated");
                                continue;
                            }
                            else
                            {
                                //if (duplicatedList.Contains(rows[1]))
                                //{
                                //    barcodeList.Add(rows[1]);
                                //}
                                barcodeList.Add(rows[1]);
                            }

                            rowDict = new Dictionary<string, string?>();
                            rowDict.Add(GoodsDef.fID, rows[1]);
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
