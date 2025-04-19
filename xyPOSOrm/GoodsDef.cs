using System.ComponentModel.Design;
using xy.ORM;

namespace xyPOSOrm
{
    public class GoodsDef : KModel
    {
        #region Constructor and helper peroperty
        public GoodsDef()
        {
            _bmName = "GoodsDef";
            _bmCode = "GoodsDef";
            InitFieldList();
        }
        //helper peroperty
        public static GoodsDef i
        {
            get => BaseModel.i<GoodsDef>();
        }

        #endregion

        static public string GoodsBarcode = "GoodsBarcode";
        static public string GoodsName = "GoodsName";
        static public string GoodsSpec = "GoodsSpec";
        static public string GoodsUnit = "GoodsUnit";
        static public string GoodsPrice = "GoodsPrice";
        static public string GoodsBrand = "GoodsBrand";
        static public string GoodsSupplier = "GoodsSupplier";
        static public string GoodsMadeIn = "GoodsMadeIn";

        private void InitFieldList()
        {
            base.InitFieldList();

            fieldList.Add(new FieldDef(GoodsBarcode, "GoodsBarcode",
                typeof(string)));

            fieldList.Add(new FieldDef(GoodsName, "GoodsName",
                typeof(string)));

            fieldList.Add(new FieldDef(GoodsSpec, "GoodsSpec",
                typeof(string)));

            fieldList.Add(new FieldDef(GoodsUnit, "GoodsUnit",
                typeof(string)));

            fieldList.Add(new FieldDef(GoodsPrice, "GoodsPrice",
                typeof(float)));

            fieldList.Add(new FieldDef(GoodsBrand, "GoodsBrand",
                typeof(string)));

            fieldList.Add(new FieldDef(GoodsSupplier, "GoodsSupplier",
                typeof(string)));

            fieldList.Add(new FieldDef(GoodsMadeIn, "GoodsMadeIn",
                typeof(string)));
        }
    }
}
