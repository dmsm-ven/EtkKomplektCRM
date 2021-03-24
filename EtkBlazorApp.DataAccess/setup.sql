SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `etk_backup`
--

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_ban_list`
--

CREATE TABLE `etk_app_ban_list` (
  `last_access` datetime NOT NULL DEFAULT current_timestamp(),
  `login` varchar(64) NOT NULL,
  `current_try_counter` int(2) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_cron_task`
--

CREATE TABLE `etk_app_cron_task` (
  `task_id` int(11) NOT NULL,
  `name` varchar(64) NOT NULL,
  `enabled` bit(1) NOT NULL DEFAULT b'1',
  `exec_time` time DEFAULT NULL,
  `last_exec_date_time` text DEFAULT NULL,
  `details_page` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_log`
--

CREATE TABLE `etk_app_log` (
  `id` int(11) NOT NULL,
  `user` varchar(64) DEFAULT NULL,
  `group_name` varchar(64) DEFAULT NULL,
  `date_time` text NOT NULL DEFAULT 'CURRENT_TIMESTAMP',
  `title` varchar(64) DEFAULT NULL,
  `message` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_price_list_template`
--

CREATE TABLE `etk_app_price_list_template` (
  `id` varchar(36) NOT NULL,
  `title` varchar(64) NOT NULL,
  `group_name` varchar(64) DEFAULT NULL,
  `manufacturer` varchar(64) DEFAULT 'NULL',
  `description` text DEFAULT NULL,
  `image` varchar(256) DEFAULT NULL,
  `price_list_type` int(1) NOT NULL DEFAULT 0,
  `discount` decimal(10,0) NOT NULL DEFAULT 1,
  `remote_uri` varchar(256) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_prikat_template`
--

CREATE TABLE `etk_app_prikat_template` (
  `template_id` int(11) NOT NULL,
  `manufacturer_id` int(11) NOT NULL,
  `discount1` decimal(10,0) NOT NULL,
  `discount2` decimal(10,0) NOT NULL,
  `currency_code` varchar(3) NOT NULL,
  `enabled` bit(1) NOT NULL DEFAULT b'1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_setting`
--

CREATE TABLE `etk_app_setting` (
  `setting_id` int(11) NOT NULL,
  `name` varchar(64) NOT NULL,
  `value` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_user`
--

CREATE TABLE `etk_app_user` (
  `user_id` int(11) NOT NULL,
  `user_group_id` int(11) NOT NULL,
  `status` int(1) NOT NULL DEFAULT 1,
  `login` varchar(32) NOT NULL,
  `password` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- Структура таблицы `etk_app_user_group`
--

CREATE TABLE `etk_app_user_group` (
  `user_group_id` int(11) NOT NULL,
  `name` varchar(32) NOT NULL,
  `permission` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `etk_app_ban_list`
--
ALTER TABLE `etk_app_ban_list`
  ADD PRIMARY KEY (`login`),
  ADD UNIQUE KEY `last_access` (`last_access`);

--
-- Индексы таблицы `etk_app_cron_task`
--
ALTER TABLE `etk_app_cron_task`
  ADD PRIMARY KEY (`task_id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Индексы таблицы `etk_app_log`
--
ALTER TABLE `etk_app_log`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `etk_app_price_list_template`
--
ALTER TABLE `etk_app_price_list_template`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `etk_app_prikat_template`
--
ALTER TABLE `etk_app_prikat_template`
  ADD PRIMARY KEY (`template_id`);

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
-- AUTO_INCREMENT для таблицы `etk_app_log`
--
ALTER TABLE `etk_app_log`
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
-- Ограничения внешнего ключа таблицы `etk_app_user`
--
ALTER TABLE `etk_app_user`
  ADD CONSTRAINT `etk_app_user_ibfk_1` FOREIGN KEY (`user_group_id`) REFERENCES `etk_app_user_group` (`user_group_id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;