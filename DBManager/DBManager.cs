using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBManager
{
    public class DBManager : DBEngine
    {
        private const string RESULT_TBL = "Result_tbl";
        private const string PART_RESULT_TBL = "Part_Result_tbl";

        public long InsertResult(DateTime dateTime, string model, string info1, string info2)
        {
            return InsertData(RESULT_TBL, 1, dateTime.ToString("yyyy-MM-dd HH:mm:ss"), model, info1, info2);
        }

        public long InsertPartResult(long resultIdx, string inspection, int count, bool result, string path)
        {
            long ret = 0;

            try
            {
                string resultStr = "1";

                if (!result)
                {
                    resultStr = "0";
                }

                ret = InsertData(PART_RESULT_TBL, 1, "$" + resultIdx, "$" + resultStr, path, inspection, "$" + count);

                return ret; 
            }
            catch
            {
                Console.WriteLine("이력 저장 실패");
                return ret;
            }
        }
    }
}
