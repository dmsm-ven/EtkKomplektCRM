using EtkBlazorApp.BL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace EtkBlazorApp.Services
{
    public interface ICurrencyChecker
    {
        Task<decimal> GetCurrencyRate(CurrencyType type);
    }

    public class CurrencyCheckerCbRf : ICurrencyChecker
    {
        Dictionary<CurrencyType, decimal> rates;

        public async Task<decimal> GetCurrencyRate(CurrencyType type)
        {
            if (rates == null)
            {
                rates = await ReadCurrenciesFromCbRf();
            }
            return rates.ContainsKey(type) ? rates[type] : 0;
        }

        private async static Task<Dictionary<CurrencyType, decimal>> ReadCurrenciesFromCbRf()
        {
            var dic = new Dictionary<CurrencyType, decimal>();
            //Инициализируем объекта типа XmlTextReader и
            //загружаем XML документ с сайта центрального банка
            var httpClient = new HttpClient();
            var xmlStreamTask = httpClient.GetStreamAsync("http://www.cbr.ru/scripts/XML_daily.asp");
            var delayTask = Task.Delay(TimeSpan.FromSeconds(10));

            var winner = await Task.WhenAny(xmlStreamTask, delayTask);
            if (winner == delayTask)
            {
                throw new TimeoutException("Не удалось получить цены с сайта ЦБ РФ");
            }

            XmlTextReader reader = new XmlTextReader(xmlStreamTask.Result);
            //В эти переменные будем сохранять куски XML
            //с определенными валютами (Euro, USD)
            string USDXml = "";
            string EuroXML = "";
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

            return dic;
        }
    }
}
