using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xy.Barcode
{
    internal class PhScannerException : Exception
    {
        private PhScannerCommErrorCode _errorCode;

        public PhScannerException(PhScannerCommErrorCode errorCode,
            string message) : base(message)
        {
            _errorCode = errorCode;
        }

        public PhScannerCommErrorCode ErrorCode => _errorCode;
    }
}
