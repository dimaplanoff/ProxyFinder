using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;

namespace ProxyService
{
    class OnRemove
    {
        private static Task FindUnactive()
        {
            return Task.Run(() => {

                while (!_Service.exit)
                {
                    var startWiatDT = DateTime.Now;
                    while ((DateTime.Now - startWiatDT).TotalMinutes < 3)
                        Task.Delay(100);
                    Sql sql = new Sql();
                    using (DataTable DT = sql.GetTable("select proxy, dtcreate from [xxxImport].[proxy].Proxys"))
                    {
                        var dt = DT.AsEnumerable().OrderBy(m => m.Field<DateTime>("dtcreate")).Select(m => m.Field<string>("proxy").Trim()).Distinct();
                        foreach (string proxy in dt)
                        {
                            if (!WebR.Check(proxy))
                            {
                                int n = sql.Exec("delete from [xxxImport].[proxy].Proxys where proxy = '" + proxy + "'");
                                Console.WriteLine(proxy + " : removed from base : " + n);
                            }
                            else
                            {
                                Console.WriteLine(proxy + " : still alive");
                            }
                        }
                    }
                    sql.Dispose();
                }
            });
        }

        /// <summary>
        /// проверяем все ссылки из базы на актуальность
        /// </summary>
        public async void FindUnactiveAsync()
        {
            await FindUnactive();
        }

    }
}
