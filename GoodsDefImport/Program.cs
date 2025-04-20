using TestBench;
using xy.ORM;
using xyPOSOrm;

namespace GoodsDefImport
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            //add all models to model list
            BaseModel.addToModelList(typeof(GoodsDef));

            FrmDbSelector frmDbSelector = new FrmDbSelector();
            if (frmDbSelector.ShowDialog() == DialogResult.OK)
            {
                Form1 form1 = new Form1();
                form1.Text = "GoodsDefImport - " + frmDbSelector.DbType;
                Application.Run(form1);
            }
        }
    }
}