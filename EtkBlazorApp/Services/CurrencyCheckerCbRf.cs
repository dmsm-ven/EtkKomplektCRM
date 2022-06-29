using EtkBlazorApp.BL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace EtkBlazorApp.Services
{
    public class CurrencyCheckerCbRf : ICurrencyChecker
    {
        Dictionary<CurrencyType, decimal> rates;
        public DateTime LastUpdate { get; private set; }

        //Время после которого курс валют запрашивается заного
        readonly TimeSpan expire_time = TimeSpan.FromMinutes(15);

        public async Task<decimal> GetCurrencyRate(CurrencyType type)
        {
            if (rates == null || (LastUpdate + expire_time) < DateTime.Now)
            {
                rates = await ReadCurrenciesFromCbRf();
            }
            return rates.ContainsKey(type) ? rates[type] : 0;
        }

        private async Task<Dictionary<CurrencyType, decimal>> ReadCurrenciesFromCbRf()
        {
            var dic = new Dictionary<CurrencyType, decimal>();
            //Инициализируем объекта типа XmlTextReader и
            //загружаем XML документ с сайта центрального банка
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");

            var xmlStreamTask = httpClient.GetStreamAsync("http://www.cbr.ru/scripts/XML_daily.asp");
            var delayTask = Task.Delay(TimeSpan.FromSeconds(10));

            var winner = await Task.WhenAny(xmlStreamTask, delayTask);
            if (winner == delayTask)
            {
                throw new TimeoutException("Не удалось получить цены с сайта ЦБ РФ");
            }

            string USDXml, EuroXML;
            ReadXmlData(xmlStreamTask.Result, out USDXml, out EuroXML);

            //Из выдернутых кусков XML кода создаем новые XML документы
            XmlDocument usdXmlDocument = new XmlDocument();
            usdXmlDocument.LoadXml(USDXml);
            XmlDocument euroXmlDocument = new XmlDocument();
            euroXmlDocument.LoadXml(EuroXML);
            //Метод возвращает узел, соответствующий выражению XPath
            XmlNode xmlNode = usdXmlDocument.SelectSingleNode("Valute/Value");
            //Считываем значение и конвертируем в decimal. Курс валют получен
            decimal usdValue = Convert.ToDecimal(xmlNode.InnerText);
            xmlNode = euroXmlDocument.SelectSingleNode("Valute/Value");
            decimal euroValue = Convert.ToDecimal(xmlNode.InnerText);

            dic[CurrencyType.USD] = usdValue;
            dic[CurrencyType.EUR] = euroValue;
            dic[CurrencyType.RUB] = 1;
            LastUpdate = DateTime.Now;

            return dic;
        }

        private static void ReadXmlData(Stream xmlStreamTask, out string USDXml, out string EuroXML)
        {
            XmlTextReader reader = new XmlTextReader(xmlStreamTask);
            //В эти переменные будем сохранять куски XML
            //с определенными валютами (Euro, USD)
            USDXml = "";
            EuroXML = "";
            //Перебираем все узлы в загруженном документе
            while (reader.Read())
            {
                //Проверяем тип текущего узла
                switch (reader.NodeType)
                {
                    //Если этого элемент Valute, то начинаем анализировать атрибуты
                    case XmlNodeType.Element:

                        if (reader.Name == "Valute")
                        {
                            if (reader.HasAttributes)
                            {
                                //Метод передвигает указатель к следующему атрибуту
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "ID")
                                    {
                                        //Если значение атрибута равно R01235, то перед нами информация о курсе доллара
                                        if (reader.Value == "R01235")
                                        {
                                            //Возвращаемся к элементу, содержащий текущий узел атрибута
                                            reader.MoveToElement();
                                            //Считываем содержимое дочерних узлом
                                            USDXml = reader.ReadOuterXml();
                                        }
                                    }

                                    //Аналогичную процедуру делаем для ЕВРО
                                    if (reader.Name == "ID")
                                    {
                                        if (reader.Value == "R01239")
                                        {
                                            reader.MoveToElement();
                                            EuroXML = reader.ReadOuterXml();
                                        }
                                    }
                                }
                            }
                        }

                        break;
                }
            }
        }
    }
}
