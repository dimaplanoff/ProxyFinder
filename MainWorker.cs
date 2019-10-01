using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyService
{
    class MainWorker
    {

        private string[] searchers = new string[] {
            @"https://www.yandex.ru/search/?offline_search=1&text=бесплатные%20прокси%20сервера",
            @"https://www.google.ru/search?q=%D0%B1%D0%B5%D1%81%D0%BF%D0%BB%D0%B0%D1%82%D0%BD%D1%8B%D0%B5+%D0%BF%D1%80%D0%BE%D0%BA%D1%81%D0%B8&newwindow=1&hl=ru&ei=iSUbXZzkNNCJk74PpNi0qA4&start=10&sa=N&ved=0ahUKEwjczKvM95XjAhXQxMQBHSQsDeUQ8tMDCHk&biw=1920&bih=969",
            @"https://www.yandex.ru/search/?text=%D1%85%D0%B0%D0%BB%D1%8F%D0%B2%D0%BD%D1%8B%D0%B5%20%D0%BF%D1%80%D0%BE%D0%BA%D1%81%D0%B8&lr=213&p=1",
            @"https://www.google.ru/search?q=%D1%85%D0%B0%D0%BB%D1%8F%D0%B2%D0%BD%D1%8B%D0%B5+%D0%BF%D1%80%D0%BE%D0%BA%D1%81%D0%B8+%D1%81%D0%BF%D0%B8%D1%81%D0%BE%D0%BA&newwindow=1&hl=ru&ei=anAbXaeeONKCk74Pza2w2AQ&start=20&sa=N&ved=0ahUKEwin--2Av5bjAhVSwcQBHc0WDEsQ8tMDCH4&biw=1920&bih=969",

        };
        #region старый метод 
        /// <summary>
        /// Cтавим в очередь ссылки найденые в поисковиках
        /// </summary>
        private void Start()
        {
            int threadCount = 10;

            while (!_Service.exit)
            {
                foreach (string search in searchers)
                {
                    try
                    {
                        string[] hrefs = WebR.GetYandexHrefs(search);                        

                        int threads = hrefs.Length >= threadCount ? threadCount : hrefs.Length;//своеобразный threadpool                    

                        for (int i = 0; i < hrefs.Length; i+=threads)//перебераем найденые сайты
                        {
                            Sql sql = new Sql();
                            Task[] Tsks = new Task[threads];

                            for (int y = 0; y < threads && y + i < hrefs.Length; y++)//разбиваем по потокам
                            {
                                int yi = y + i;
                                Tsks[y] = Task.Run(() =>
                                {
                                    string href = hrefs[yi];
                                    string[] findedProxy = WebR.GetProxys(href);
                                    Console.WriteLine("created thread - " + href);                                    
                                    foreach (string prx in findedProxy)
                                    {
                                        OnCreate.Add(prx, ref sql);
                                        if (_Service.exit) return;
                                    }     
                                    Console.WriteLine("dispose thread - " + href);
                                });
                            }

                            Task.WaitAll(Tsks.Where(t=>t != null).ToArray());
                            sql.Dispose();
                            if (_Service.exit) return;
                        }

                        
                    }
                    catch (Exception e) { Console.WriteLine("finder : " + e.Message); }
                    if (_Service.exit) return;
                }
                GC.Collect();
                GC.GetTotalMemory(false);
            }
        }
        #endregion


        private async void Start(int threads)
        {
            

            while (!_Service.exit)
            {
                foreach (string search in searchers)
                {
                    try
                    {
                        string[] hrefs = WebR.GetYandexHrefs(search);

                        List<Task> taskList = new List<Task>();
                                          

                        for (int i = 0; i < hrefs.Length; i ++)
                        {
                           
                            

                            
                                int yi = i;
                                taskList.Add( Task.Run(() =>
                                {
                                    Sql sql = new Sql();
                                    if (sql.connectionIsOpen)
                                    {
                                        try
                                        {

                                            string href = hrefs[yi];
                                            string[] findedProxy = WebR.GetProxys(href);
                                            Console.WriteLine("created thread - " + href);
                                            foreach (string prx in findedProxy)
                                            {
                                                OnCreate.Add(prx, ref sql);
                                                if (_Service.exit) return;
                                            }
                                        }
                                        catch (Exception e) { Console.WriteLine("one of main threads : " + e.Message); }
                                        finally
                                        {
                                            sql.Dispose();
                                        }
                                    }
                                }));

                            //ограничиваем кол-во потоков за раз
                            if (taskList.Where(e => e != null).Count() > threads)
                                await Task.WhenAny(taskList.ToArray());
                           
                                
                          
                            
                            if (_Service.exit) return;
                        }

                        await Task.WhenAll(taskList.Where(e=>e !=null).ToArray());
                        GC.Collect();
                        GC.GetTotalMemory(false);
                    }
                    catch (Exception e) { Console.WriteLine("finder : " + e.Message); }
                    if (_Service.exit) return;
                }
                
            }
        }




        /// <summary>
        /// медот запускает поиск проксей по всему интернету =)
        /// </summary>
        public async void StartAsync()
        {
            await Task.Run(()=> {
                Start(15);
            });
        }

    }
}
