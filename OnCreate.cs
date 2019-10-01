using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    class OnCreate
    {
        /// <summary>
        /// Добавляет запись в базу,
        /// если прокси прошел проверку и не имеет дублей в базе.
        /// Возвращает кол-во затронутых строк
        /// </summary>
        /// <param name="prx_"></param>
        /// <returns></returns>
        public static int Add(string prx_, ref Sql sql)
        {
            prx_ = prx_.Trim();

            try
            {

                
                    if (sql.Exec("select count (proxy) from [xxxImport].[proxy].Proxys where proxy = '" + prx_ + "'") > 0) throw new Exception();
                    if (!WebR.Check(prx_)) throw new Exception();

                lock (_Service.addingSync)
                {
                    int n = sql.Exec(@"insert into [xxxImport].[proxy].Proxys (id, proxy, dtcreate) values 
((select isnull(max(id), 0) from [CursorImport].[proxy].Proxys) + 1, '" + prx_ + "', getdate())");
                    Console.WriteLine(prx_ + " : added to base : " + n);
                    return n;
                }
            }
            catch { return 0; }
        }
    }
}
