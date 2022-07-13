-- phpMyAdmin SQL Dump
-- version 4.4.15.10
-- https://www.phpmyadmin.net
--
-- Хост: localhost
-- Время создания: Июл 13 2022 г., 10:47
-- Версия сервера: 10.4.18-MariaDB-log
-- Версия PHP: 5.4.16

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `etk`
--

DELIMITER $$
--
-- Функции
--
$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_cron_task`
--

CREATE TABLE IF NOT EXISTS `etk_app_cron_task` (
  `task_id` int(11) NOT NULL,
  `task_type_id` int(11) DEFAULT NULL,
  `linked_price_list_guid` varchar(36) DEFAULT NULL,
  `name` varchar(64) NOT NULL,
  `description` text DEFAULT NULL,
  `enabled` bit(1) NOT NULL DEFAULT b'1',
  `archived` bit(1) NOT NULL DEFAULT b'0',
  `exec_time` time DEFAULT NULL,
  `additional_exec_time` text DEFAULT NULL,
  `last_exec_date_time` text DEFAULT NULL,
  `last_exec_result` int(1) DEFAULT NULL,
  `last_exec_file_size` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_cron_task_history`
--

CREATE TABLE IF NOT EXISTS `etk_app_cron_task_history` (
  `task_result_id` int(11) NOT NULL,
  `task_id` int(11) NOT NULL,
  `date_time` text NOT NULL,
  `exec_result` int(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_cron_task_type`
--

CREATE TABLE IF NOT EXISTS `etk_app_cron_task_type` (
  `task_type_id` int(11) NOT NULL,
  `name` varchar(64) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_discount_to_category`
--

CREATE TABLE IF NOT EXISTS `etk_app_discount_to_category` (
  `category_id` int(11) NOT NULL,
  `discount` decimal(6,2) NOT NULL,
  `date_start` date NOT NULL,
  `date_end` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_discount_to_manufacturer`
--

CREATE TABLE IF NOT EXISTS `etk_app_discount_to_manufacturer` (
  `manufacturer_id` int(11) NOT NULL,
  `discount` decimal(6,2) NOT NULL,
  `date_start` date NOT NULL,
  `date_end` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_discount_to_stock`
--

CREATE TABLE IF NOT EXISTS `etk_app_discount_to_stock` (
  `stock_id` int(11) NOT NULL,
  `discount` decimal(6,2) NOT NULL,
  `date_start` date NOT NULL,
  `date_end` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_fixed_product`
--

CREATE TABLE IF NOT EXISTS `etk_app_fixed_product` (
  `product_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_log`
--

CREATE TABLE IF NOT EXISTS `etk_app_log` (
  `id` int(11) NOT NULL,
  `user` varchar(64) DEFAULT NULL,
  `group_name` varchar(64) DEFAULT NULL,
  `date_time` text NOT NULL DEFAULT 'CURRENT_TIMESTAMP',
  `title` varchar(64) DEFAULT NULL,
  `message` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_marketplace_brand_export`
--

CREATE TABLE IF NOT EXISTS `etk_app_marketplace_brand_export` (
  `marketplace` varchar(64) NOT NULL,
  `manufacturer_id` int(11) NOT NULL,
  `discount` decimal(10,0) NOT NULL,
  `checked_stocks` varchar(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_monobrand`
--

CREATE TABLE IF NOT EXISTS `etk_app_monobrand` (
  `monobrand_id` int(11) NOT NULL,
  `manufacturer_id` int(11) DEFAULT NULL,
  `website` varchar(256) NOT NULL,
  `currency_code` varchar(3) NOT NULL DEFAULT 'RUB',
  `is_update_enabled` bit(1) NOT NULL DEFAULT b'1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_ozon_seller_discount`
--

CREATE TABLE IF NOT EXISTS `etk_app_ozon_seller_discount` (
  `manufacturer_id` int(11) NOT NULL,
  `discount` decimal(8,4) NOT NULL,
  `enabled` bit(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_partner`
--

CREATE TABLE IF NOT EXISTS `etk_app_partner` (
  `id` varchar(36) NOT NULL,
  `name` varchar(128) NOT NULL,
  `website` varchar(256) DEFAULT NULL,
  `email` varchar(256) DEFAULT NULL,
  `phone_number` varchar(256) DEFAULT NULL,
  `address` varchar(256) DEFAULT NULL,
  `contact_person` varchar(128) DEFAULT NULL,
  `description` text DEFAULT NULL,
  `priority` int(11) NOT NULL DEFAULT 1,
  `discount` decimal(6,2) NOT NULL DEFAULT 0.00,
  `created` datetime NOT NULL DEFAULT current_timestamp(),
  `updated` datetime NOT NULL,
  `price_list_password` varchar(36) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_partner_checked_brand`
--

CREATE TABLE IF NOT EXISTS `etk_app_partner_checked_brand` (
  `partner_id` varchar(36) NOT NULL,
  `manufacturer_id` int(11) NOT NULL,
  `discount` decimal(6,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_partner_request_history`
--

CREATE TABLE IF NOT EXISTS `etk_app_partner_request_history` (
  `request_id` int(11) NOT NULL,
  `partner_id` varchar(36) NOT NULL,
  `date_time` datetime NOT NULL,
  `answer_time` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template` (
  `id` varchar(36) NOT NULL,
  `stock_partner_id` int(11) DEFAULT NULL,
  `title` varchar(64) NOT NULL,
  `group_name` varchar(64) DEFAULT NULL,
  `description` text DEFAULT NULL,
  `image` varchar(256) DEFAULT NULL,
  `content_type_id` int(1) NOT NULL DEFAULT 1,
  `discount` decimal(6,2) NOT NULL DEFAULT 1.00,
  `nds` bit(1) NOT NULL DEFAULT b'0',
  `remote_uri` varchar(256) DEFAULT NULL,
  `remote_uri_method_id` int(2) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template_credentials`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template_credentials` (
  `template_guid` varchar(36) NOT NULL,
  `login` varchar(64) DEFAULT NULL,
  `password` varchar(64) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template_email_search_criteria`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template_email_search_criteria` (
  `template_guid` varchar(36) NOT NULL,
  `subject` varchar(256) NOT NULL,
  `sender` varchar(256) NOT NULL,
  `file_name_pattern` varchar(32) NOT NULL,
  `max_age_in_days` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template_load_method`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template_load_method` (
  `id` int(11) NOT NULL,
  `name` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template_manufacturer_list`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template_manufacturer_list` (
  `price_list_guid` varchar(36) NOT NULL,
  `manufacturer_id` int(11) NOT NULL,
  `list_type` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template_manufacturer_map`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template_manufacturer_map` (
  `price_list_guid` varchar(36) NOT NULL,
  `text` varchar(255) NOT NULL,
  `manufacturer_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template_quantity_map`
--

CREATE TABLE IF NOT EXISTS `etk_app_price_list_template_quantity_map` (
  `price_list_guid` varchar(36) NOT NULL,
  `text` varchar(64) NOT NULL,
  `quantity` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_prikat_template`
--

CREATE TABLE IF NOT EXISTS `etk_app_prikat_template` (
  `template_id` int(11) NOT NULL,
  `manufacturer_id` int(11) NOT NULL,
  `discount1` decimal(8,4) NOT NULL,
  `discount2` decimal(8,4) NOT NULL,
  `currency_code` varchar(3) DEFAULT NULL,
  `enabled` bit(1) NOT NULL DEFAULT b'1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_setting`
--

CREATE TABLE IF NOT EXISTS `etk_app_setting` (
  `setting_id` int(11) NOT NULL,
  `name` varchar(512) NOT NULL,
  `value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_user`
--

CREATE TABLE IF NOT EXISTS `etk_app_user` (
  `user_id` int(11) NOT NULL,
  `user_group_id` int(11) NOT NULL,
  `status` int(1) NOT NULL DEFAULT 1,
  `login` varchar(32) NOT NULL,
  `password` varchar(256) NOT NULL,
  `ip` varchar(15) DEFAULT NULL,
  `creation_date` datetime NOT NULL DEFAULT current_timestamp(),
  `last_login_date` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_user_group`
--

CREATE TABLE IF NOT EXISTS `etk_app_user_group` (
  `user_group_id` int(11) NOT NULL,
  `name` varchar(32) NOT NULL,
  `permission` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `etk_app_cron_task`
--
ALTER TABLE `etk_app_cron_task`
  ADD PRIMARY KEY (`task_id`),
  ADD UNIQUE KEY `name` (`name`),
  ADD KEY `task_type_id` (`task_type_id`),
  ADD KEY `linked_price_list_guid` (`linked_price_list_guid`);

--
-- Индексы таблицы `etk_app_cron_task_history`
--
ALTER TABLE `etk_app_cron_task_history`
  ADD PRIMARY KEY (`task_result_id`),
  ADD KEY `FK_task_id` (`task_id`);

--
-- Индексы таблицы `etk_app_cron_task_type`
--
ALTER TABLE `etk_app_cron_task_type`
  ADD PRIMARY KEY (`task_type_id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Индексы таблицы `etk_app_discount_to_category`
--
ALTER TABLE `etk_app_discount_to_category`
  ADD PRIMARY KEY (`category_id`);

--
-- Индексы таблицы `etk_app_discount_to_manufacturer`
--
ALTER TABLE `etk_app_discount_to_manufacturer`
  ADD PRIMARY KEY (`manufacturer_id`);

--
-- Индексы таблицы `etk_app_discount_to_stock`
--
ALTER TABLE `etk_app_discount_to_stock`
  ADD PRIMARY KEY (`stock_id`);

--
-- Индексы таблицы `etk_app_fixed_product`
--
ALTER TABLE `etk_app_fixed_product`
  ADD PRIMARY KEY (`product_id`);

--
-- Индексы таблицы `etk_app_log`
--
ALTER TABLE `etk_app_log`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `etk_app_marketplace_brand_export`
--
ALTER TABLE `etk_app_marketplace_brand_export`
  ADD PRIMARY KEY (`marketplace`,`manufacturer_id`);

--
-- Индексы таблицы `etk_app_monobrand`
--
ALTER TABLE `etk_app_monobrand`
  ADD PRIMARY KEY (`monobrand_id`);

--
-- Индексы таблицы `etk_app_ozon_seller_discount`
--
ALTER TABLE `etk_app_ozon_seller_discount`
  ADD PRIMARY KEY (`manufacturer_id`);

--
-- Индексы таблицы `etk_app_partner`
--
ALTER TABLE `etk_app_partner`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Индексы таблицы `etk_app_partner_checked_brand`
--
ALTER TABLE `etk_app_partner_checked_brand`
  ADD PRIMARY KEY (`partner_id`,`manufacturer_id`);

--
-- Индексы таблицы `etk_app_partner_request_history`
--
ALTER TABLE `etk_app_partner_request_history`
  ADD PRIMARY KEY (`request_id`),
  ADD KEY `FK_Partner` (`partner_id`);

--
-- Индексы таблицы `etk_app_price_list_template`
--
ALTER TABLE `etk_app_price_list_template`
  ADD PRIMARY KEY (`id`),
  ADD KEY `remote_uri_method_id` (`remote_uri_method_id`),
  ADD KEY `content_type_id` (`content_type_id`);

--
-- Индексы таблицы `etk_app_price_list_template_credentials`
--
ALTER TABLE `etk_app_price_list_template_credentials`
  ADD PRIMARY KEY (`template_guid`);

--
-- Индексы таблицы `etk_app_price_list_template_email_search_criteria`
--
ALTER TABLE `etk_app_price_list_template_email_search_criteria`
  ADD PRIMARY KEY (`template_guid`);

--
-- Индексы таблицы `etk_app_price_list_template_load_method`
--
ALTER TABLE `etk_app_price_list_template_load_method`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Индексы таблицы `etk_app_price_list_template_manufacturer_list`
--
ALTER TABLE `etk_app_price_list_template_manufacturer_list`
  ADD UNIQUE KEY `list_type` (`list_type`,`manufacturer_id`,`price_list_guid`);

--
-- Индексы таблицы `etk_app_price_list_template_manufacturer_map`
--
ALTER TABLE `etk_app_price_list_template_manufacturer_map`
  ADD UNIQUE KEY `price_list_guid` (`price_list_guid`,`text`);

--
-- Индексы таблицы `etk_app_price_list_template_quantity_map`
--
ALTER TABLE `etk_app_price_list_template_quantity_map`
  ADD UNIQUE KEY `price_list_guid` (`price_list_guid`,`text`);

--
-- Индексы таблицы `etk_app_prikat_template`
--
ALTER TABLE `etk_app_prikat_template`
  ADD PRIMARY KEY (`template_id`),
  ADD UNIQUE KEY `UNQ_manufacturer_id` (`manufacturer_id`) USING BTREE;

--
-- Индексы таблицы `etk_app_setting`
--
ALTER TABLE `etk_app_setting`
  ADD PRIMARY KEY (`setting_id`),
  ADD UNIQUE KEY `UQ_name` (`name`);

--
-- Индексы таблицы `etk_app_user`
--
ALTER TABLE `etk_app_user`
  ADD PRIMARY KEY (`user_id`),
  ADD UNIQUE KEY `name` (`login`),
  ADD KEY `user_group_id` (`user_group_id`);

--
-- Индексы таблицы `etk_app_user_group`
--
ALTER TABLE `etk_app_user_group`
  ADD PRIMARY KEY (`user_group_id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `etk_app_cron_task`
--
ALTER TABLE `etk_app_cron_task`
  MODIFY `task_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_cron_task_history`
--
ALTER TABLE `etk_app_cron_task_history`
  MODIFY `task_result_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_cron_task_type`
--
ALTER TABLE `etk_app_cron_task_type`
  MODIFY `task_type_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_log`
--
ALTER TABLE `etk_app_log`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_monobrand`
--
ALTER TABLE `etk_app_monobrand`
  MODIFY `monobrand_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_partner_request_history`
--
ALTER TABLE `etk_app_partner_request_history`
  MODIFY `request_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_price_list_template_load_method`
--
ALTER TABLE `etk_app_price_list_template_load_method`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_prikat_template`
--
ALTER TABLE `etk_app_prikat_template`
  MODIFY `template_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_setting`
--
ALTER TABLE `etk_app_setting`
  MODIFY `setting_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_user`
--
ALTER TABLE `etk_app_user`
  MODIFY `user_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT для таблицы `etk_app_user_group`
--
ALTER TABLE `etk_app_user_group`
  MODIFY `user_group_id` int(11) NOT NULL AUTO_INCREMENT;
--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `etk_app_cron_task`
--
ALTER TABLE `etk_app_cron_task`
  ADD CONSTRAINT `etk_app_cron_task_ibfk_1` FOREIGN KEY (`task_type_id`) REFERENCES `etk_app_cron_task_type` (`task_type_id`),
  ADD CONSTRAINT `etk_app_cron_task_ibfk_2` FOREIGN KEY (`linked_price_list_guid`) REFERENCES `etk_app_price_list_template` (`id`);

--
-- Ограничения внешнего ключа таблицы `etk_app_user`
--
ALTER TABLE `etk_app_user`
  ADD CONSTRAINT `etk_app_user_ibfk_1` FOREIGN KEY (`user_group_id`) REFERENCES `etk_app_user_group` (`user_group_id`);

DELIMITER $$
--
-- События
--
CREATE DEFINER=`etk_remote`@`localhost` EVENT `Обновление статуса на складе` ON SCHEDULE EVERY 30 MINUTE STARTS '2021-08-02 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO UPDATE oc_product
SET stock_status_id = CASE quantity
WHEN 0 THEN (SELECT stock_status_id FROM oc_stock_status WHERE name = 'Нет в наличии')
ELSE (SELECT stock_status_id FROM oc_stock_status WHERE name = 'В наличии')
END
WHERE stock_status_id != (SELECT stock_status_id FROM oc_stock_status WHERE name = 'Снято с производства')$$

CREATE DEFINER=`etk`@`localhost` EVENT `Удаление товаров из категории 'Распродажа'` ON SCHEDULE EVERY 1 HOUR STARTS '2021-06-28 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO DELETE FROM oc_product_to_category 
WHERE category_id = 60396 AND product_id IN (SELECT product_id FROM oc_product_special WHERE date_end < NOW())$$

CREATE DEFINER=`etk`@`localhost` EVENT `Удаление просроченных акций на товары` ON SCHEDULE EVERY 6 HOUR STARTS '2021-06-28 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO DELETE FROM oc_product_special
WHERE date_end < NOW()$$

CREATE DEFINER=`etk`@`localhost` EVENT `Исправление минусовых остатков` ON SCHEDULE EVERY 1 HOUR STARTS '2021-06-28 00:00:00' ON COMPLETION NOT PRESERVE ENABLE COMMENT 'Убирает минусовые остатки в товарах' DO UPDATE oc_product
set quantity = 0
WHERE quantity < 0$$

CREATE DEFINER=`etk_remote`@`localhost` EVENT `Удаление просроченных акций на категории` ON SCHEDULE EVERY 6 HOUR STARTS '2021-06-28 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO DELETE FROM etk_app_discount_to_category
WHERE date_end < NOW()$$

CREATE DEFINER=`etk`@`localhost` EVENT `Удаление просроченных акций на производителей` ON SCHEDULE EVERY 6 HOUR STARTS '2021-06-28 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO DELETE FROM etk_app_discount_to_manufacturer
WHERE date_end < NOW()$$

CREATE DEFINER=`etk_remote`@`localhost` EVENT `Удаление просроченных акций на склады` ON SCHEDULE EVERY 6 HOUR STARTS '2021-06-28 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO DELETE FROM etk_app_discount_to_stock
WHERE date_end < NOW()$$

CREATE DEFINER=`etk_remote`@`localhost` EVENT `Обновление количества на складке` ON SCHEDULE EVERY 30 MINUTE STARTS '2021-08-02 00:00:00' ON COMPLETION NOT PRESERVE ENABLE DO UPDATE oc_product
JOIN oc_product_description ON (oc_product.product_id = oc_product_description.product_id)
SET quantity = GREATEST(0, (SELECT SUM(oc_product_to_stock.quantity) 
FROM oc_product_to_stock 
WHERE oc_product_to_stock.product_id = oc_product.product_id))
WHERE status = 1 AND oc_product_description.main_product = 0$$

DELIMITER ;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
