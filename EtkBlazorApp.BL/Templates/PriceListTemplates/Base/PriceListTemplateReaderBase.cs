using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public abstract class PriceListTemplateReaderBase
    {
        //TODO: Тут стоит поменят словари и списки на ReadOnly версии
        protected Dictionary<string, string> ManufacturerNameMap { get; private set; }
        protected Dictionary<string, int> QuantityMap { get; private set; }
        protected List<string> BrandsWhiteList { get; private set; }
        protected List<string> BrandsBlackList { get; private set; }

        public PriceListTemplateReaderBase()
        {
            ManufacturerNameMap = new Dictionary<string, string>();
            QuantityMap = new Dictionary<string, int>();
            BrandsWhiteList = new List<string>();
            BrandsBlackList = new List<string>();

            /*
            INSERT INTO etk_app_price_list_template_manufacturer_map(price_list_guid, text, manufacturer_id) VALUES
            ('4EA34EEA-5407-4807-8E33-D8A8FA71ECBA', 'Proskit', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Pro'sKit")),
            ('4EA34EEA-5407-4807-8E33-D8A8FA71ECBA', 'Lambda', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "TDK-Lambda")),
            ('4EA34EEA-5407-4807-8E33-D8A8FA71ECBA', 'АКТАКОМ', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Aktakom")),
            ('4EA34EEA-5407-4807-8E33-D8A8FA71ECBA', 'Dinolite', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Dino-Lite")),
            ('4EA34EEA-5407-4807-8E33-D8A8FA71ECBA', 'Megeon', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Мегеон")),
            ('B048D3E6-D8D1-4867-944B-6D5D3A6D4396', 'Proskit', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Pro'sKit")),
            ('B048D3E6-D8D1-4867-944B-6D5D3A6D4396', 'Lambda', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "TDK-Lambda")),
            ('B048D3E6-D8D1-4867-944B-6D5D3A6D4396', 'АКТАКОМ', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Aktakom")),
            ('B048D3E6-D8D1-4867-944B-6D5D3A6D4396', 'Dinolite', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Dino-Lite")),
            ('B048D3E6-D8D1-4867-944B-6D5D3A6D4396', 'Megeon', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Мегеон")),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', 'MeanWell', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Mean Well")),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', 'ICSComponents', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "ICS Components")),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', 'SpectrahDynamics', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Spectrah Dynamics")),
            ('83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA', 'МЕГЕОН', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Мегеон")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'Pro'skit', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Pro'sKit")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'TIANMA Europe GmbH', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Tianma")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'BOE Technology Group Corp', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "BOE")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'TechStar Electronics Corp', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "TechStar")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'Sinotectronics Inc.', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Sinotectronics")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'Disteck Display Inc.', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Disteck")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'Apex Material Technology Corp.', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "AMT")),
            ('3853B988-DB37-4B6E-861F-3000B643FAC4', 'Onetouch Technologies Co., Ltd', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Onetouch")),
            ('C53B8C85-3115-421F-A579-0B5BFFF6EF48', 'ITECH ВЭД', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "ITECH")),	
            ('438B5182-62DD-42C4-846F-4901C3B38B14', 'Teledyne LeCroy', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "LeCroy")),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', 'Keysight Technologies', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Keysight")),
            ('E11729F8-244B-420A-801C-110FC81BE61B', 'PROSKIT', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Pro'sKit")),
            ('E11729F8-244B-420A-801C-110FC81BE61B', 'MASTECH', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Mastech")),
            ('58F658D6-483C-4EE6-9E23-9932514624CF', 'Leica Geosystems', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Leica"))		

            INSERT INTO etk_app_price_list_manufacturer_list(price_list_guid, text, manufacturer_id, list_type) VALUES
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Tianma"), 'white_list'),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "NEC"), 'white_list'),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Mean Well"), 'white_list'),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "DMC"), 'white_list'),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Avalue"), 'white_list'),
            ('3D41DDC2-BB5C-4D6A-8129-C486BD953A3D', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "ICS Components"), 'white_list'),
            ('83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Мегеон"), 'white_list'),			
            ('C53B8C85-3115-421F-A579-0B5BFFF6EF48', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Hakko"), 'white_list'),
            ('C53B8C85-3115-421F-A579-0B5BFFF6EF48', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Keysight"), 'white_list'), 
            ('C53B8C85-3115-421F-A579-0B5BFFF6EF48', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "ITECH"), 'white_list'),
            ('E11729F8-244B-420A-801C-110FC81BE61B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Pro'sKit"), 'white_list'),
            ('E11729F8-244B-420A-801C-110FC81BE61B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Mastech"), 'white_list'),
            ('E11729F8-244B-420A-801C-110FC81BE61B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "UNI-T"), 'white_list'),			
            ('C1412CC4-79E5-467F-A8E9-ACF18E320B92', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Зубр"), 'white_list'),
            ('C1412CC4-79E5-467F-A8E9-ACF18E320B92', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Kraftool"), 'white_list'),
            ('C1412CC4-79E5-467F-A8E9-ACF18E320B92', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Grinda"), 'white_list'),
            ('C1412CC4-79E5-467F-A8E9-ACF18E320B92', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Stayer"), 'white_list'),
            ('2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Зубр"), 'white_list'),
            ('2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Kraftool"), 'white_list'),
            ('2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Grinda"), 'white_list'),
            ('2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Stayer"), 'white_list'),
            ('9EEB7A82-1029-4C1F-A282-196C0907160B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "MTD"), 'white_list'),
            ('9EEB7A82-1029-4C1F-A282-196C0907160B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Cub Cadet"), 'white_list'),
            ('9EEB7A82-1029-4C1F-A282-196C0907160B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Wolf Garten"), 'white_list'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Wiha"), 'white_list'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Klauke"), 'white_list'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Weicon"), 'white_list'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Brady"), 'white_list'),
            ('91EE5CFF-4752-4D6F-8E36-E5557149225B', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "FIT"), 'white_list'),
            ('FFA35661-230F-431F-AEA0-BC57F4A7C8AE', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Wiha"), 'white_list'),
            ('FFA35661-230F-431F-AEA0-BC57F4A7C8AE', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Schut"), 'white_list'),
            ('4EA34EEA-5407-4807-8E33-D8A8FA71ECBA', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Etari"), 'black_list'),
            ('B048D3E6-D8D1-4867-944B-6D5D3A6D4396', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Etari"), 'black_list'),
            ('83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Mean Well"), 'black_list'),
            ('83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "FIT"), 'black_list'),
            ('83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Viking"), 'black_list'),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "TDK-Lambda"), 'black_list'),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Weller"), 'black_list'),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Pro'sKit"), 'black_list'),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Bernstein"), 'black_list'),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Testo"), 'black_list'),
            ('438B5182-62DD-42C4-846F-4901C3B38B14', (SELECT manufacturer_id FROM oc_manufacturer WHERE name = "Viking"), 'black_list')

            INSERT INTO etk_app_price_list_quantity_map(price_list_guid, text, quantity) VALUES
            ('83488F9E-CCA7-4BDB-A6CC-7C3D4CF054EA', 'Более 10', '10'),
            ('5785C822-A57D-4DD2-9B68-E0301DDF135B', 'A', '10'),
            ('5785C822-A57D-4DD2-9B68-E0301DDF135B', 'B', '5'),
            ('5785C822-A57D-4DD2-9B68-E0301DDF135B', 'C', '0'),
            ('C1412CC4-79E5-467F-A8E9-ACF18E320B92', 'Есть', '5'),
            ('C1412CC4-79E5-467F-A8E9-ACF18E320B92', 'Нет', '0'),
            ('2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6', 'Да', '5'),
            ('2267B1A2-F80C-4AA4-B5AC-D3CBFF6793C6', 'Нет', '0'),		
            ('B6B23CAF-0D0C-416F-AB96-C7FD42FD0DED', '+', '3'),
            ('9EEB7A82-1029-4C1F-A282-196C0907160B', 'Достаточное количество', '50'),			
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', 'нет', '0'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', '*', '5'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', '**', '10'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', '***', '30'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', '****', '40'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', '*****', '50'),
            ('6C238D2C-145E-4320-B4E3-DCA8B8FAECB0', '******', '100')
            */

        }

        protected string MapManufacturerName(string manufacturerName)
        {
            if (!string.IsNullOrWhiteSpace(manufacturerName) && ManufacturerNameMap.Any() && ManufacturerNameMap.ContainsKey(manufacturerName))
            {
                return ManufacturerNameMap[manufacturerName];
            }
            return manufacturerName;
        }

        protected virtual decimal? ParsePrice(string str, bool canBeNull = false, int? roundDigits = null)
        {
            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                string strValue = str.Replace(",", ".").Replace(" ", string.Empty);
                if (decimal.TryParse(strValue, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = Math.Max(parsedPrice, 0);
                    if (roundDigits.HasValue)
                    {
                        price = Math.Round(price.Value, roundDigits.Value);
                    }
                }
            }

            return canBeNull ? price : (price ?? 0);
        }

        protected virtual int? ParseQuantity(string str, bool canBeNull = false)
        {
            int? quantity = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedQuantity))
                {
                    quantity = Math.Max((int)parsedQuantity, 0);
                }
                else if(QuantityMap.ContainsKey(str))
                {
                    quantity = QuantityMap[str];
                }
            }

            return canBeNull ? quantity : (quantity ?? 0);
        }

        /// <summary>
        /// Проверка на пропуск из загрузки прайс-листа. Скорее всего нужно перенести эту проверки на момент после загрузки прайс-листа, там можно будет убрать проверку из каждого шаблона и оставить только в одном месте
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        protected bool ManufacturerSkipCheck(string manufacturer)
        {
            bool blackListCondition = BrandsBlackList.Any() && BrandsBlackList.Contains(manufacturer, StringComparer.OrdinalIgnoreCase);
            bool whiteListCondition = BrandsWhiteList.Any() && (BrandsWhiteList.Contains(manufacturer, StringComparer.OrdinalIgnoreCase) == false);

            return blackListCondition || whiteListCondition;
        }

        public void FillTemplateInfo(PriceListTemplateEntity templateInfo)
        {
            if (templateInfo.quantity_map != null)
            {
                foreach (var kvp in templateInfo.quantity_map)
                {
                    QuantityMap[kvp.text] = kvp.quantity;
                }
            }
            if (templateInfo.manufacturer_name_map != null)
            {
                foreach (var kvp in templateInfo.manufacturer_name_map)
                {
                    ManufacturerNameMap[kvp.text] = kvp.name;
                }
            }
            if (templateInfo.manufacturer_skip_list != null)
            {
                var listsSource = templateInfo.manufacturer_skip_list.Where(i => !string.IsNullOrWhiteSpace(i.name) && !string.IsNullOrWhiteSpace(i.list_type));
                foreach (var item in listsSource)
                {
                    if (item.list_type == "black_list" && !BrandsBlackList.Contains(item.name))
                    {
                        BrandsBlackList.Add(item.name);
                    }
                    if (item.list_type == "white_list" && !BrandsWhiteList.Contains(item.name))
                    {
                        BrandsWhiteList.Add(item.name);
                    }
                }
            }
        }
    }
}
