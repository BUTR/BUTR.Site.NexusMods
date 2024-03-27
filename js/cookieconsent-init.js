// obtain plugin
const cc = initCookieConsent();

// run plugin with your configuration
cc.run({
    current_lang: 'en',
    autoclear_cookies: true,
    page_scripts: true,
    mode: 'opt-in',
    auto_language: 'browser',
    autorun: true,
    cookie_name: 'cc_cookie',
    cookie_expiration: 182,
    revision: 1,

    languages: {
        'en': {
            consent_modal: {
                title: 'We use cookies!',
                description: 'This website uses strictly necessary and metrics cookies to have a basic understanding of our user base. The latter will be used only after consent. <button type="button" data-cc="c-settings" class="cc-link">Settings</button>',
                primary_btn: {
                    text: 'Accept all',
                    role: 'accept_all'
                },
                secondary_btn: {
                    text: 'Reject all',
                    role: 'accept_necessary'
                }
            },
            settings_modal: {
                title: 'Cookie preferences',
                save_settings_btn: 'Save settings',
                accept_all_btn: 'Accept all',
                reject_all_btn: 'Reject all',
                close_btn_label: 'Close',
                cookie_table_headers: [
                    {col1: 'Name'},
                    {col2: 'Domain'},
                    {col3: 'Expiration'},
                    {col4: 'Description'}
                ],
                blocks: [
                    {
                        title: 'Cookie usage 📢',
                        description: 'You can choose for each category to opt-in/out whenever you want.'
                    },
                    {
                        title: 'Strictly necessary cookies',
                        description: 'Strictly necessary cookies are used to make the site work.',
                        toggle: {
                            value: 'necessary',
                            enabled: true,
                            readonly: true
                        },
                        cookie_table: [
                            {
                                col1: 'cc_cookie',
                                col2: 'This site',
                                col3: '182 days',
                                col4: 'The settings of user\'s cookie consent'
                            }
                        ]
                    },
                    {
                        title: 'Metrics cookies',
                        description: 'Metrics cookies are used by us to check how the site is used by users. AdBlockers will block the metrics cookies even if consent is given. No bypass is used by us.',
                        toggle: {
                            value: 'metrics',
                            enabled: false,
                            readonly: false
                        },
                        cookie_table: [
                            {
                                col1: '_ym_d',
                                col2: 'yandex.ru',
                                col3: '1 year',
                                col4: 'Saves the date of the user\'s first site session'
                            },
                            {
                                col1: '_ym_uid',
                                col2: 'yandex.ru',
                                col3: '1 year',
                                col4: 'Used for identifying site users',
                            },
                            {
                                col1: 'i',
                                col2: 'yandex.ru',
                                col3: '1 year',
                                col4: 'Used for identifying site users',
                            },
                            {
                                col1: 'yandexuid',
                                col2: 'yandex.ru',
                                col3: '1 year',
                                col4: 'Used for identifying site users',
                            },
                            {
                                col1: 'yuidss',
                                col2: 'yandex.ru',
                                col3: '1 year',
                                col4: 'Used for identifying site users',
                            },
                            {
                                col1: 'ymex',
                                col2: 'yandex.ru',
                                col3: '1 year',
                                col4: 'Stores auxiliary information for Yandex.Metrica performance: ID creation time and their alternative values',
                            },
                            {
                                col1: '_ym_isad',
                                col2: 'yandex.ru',
                                col3: '2 days',
                                col4: 'Determines whether a user has ad blockers',
                            },
                            {
                                col1: 'yabs-sid',
                                col2: 'yandex.ru',
                                col3: 'Session',
                                col4: 'Session ID',
                            }
                        ]
                    },
                    {
                        title: 'More information',
                        description: 'For any questions about the cookies and your choices, open an <a class="cc-link" href="https://github.com/BUTR/BUTR.Site.NexusMods/issues">issue</a> in our repository!',
                    }
                ]
            }
        },
        'ru': {
            consent_modal: {
                title: 'Мы используем куки!',
                description: 'Этот сайт использует базовые и метрические куки, чтобы получить базовое представление о нашей пользовательской базе. Последние будут использованы только после согласия. <button type="button" data-cc="c-settings" class="cc-link">Настройки</button> ',
                primary_btn: {
                    text: 'Разрешить все',
                    role: 'accept_all'
                },
                secondary_btn: {
                    text: 'Запретить все',
                    role: 'accept_necessary'
                }
            },
            settings_modal: {
                title: 'Настройки',
                save_settings_btn: 'Сохранить',
                accept_all_btn: 'Разрешить все',
                reject_all_btn: 'Запретить все',
                close_btn_label: 'Закрыть',
                cookie_table_headers: [
                    {col1: 'Имя'},
                    {col2: 'Домен'},
                    {col3: 'Срок действия'},
                    {col4: 'Описание'}
                ],
                blocks: [
                    {
                        title: 'Использование куки 📢',
                        description: 'Вы можете выбрать для каждой категории разрешение/запрет в любое время.'
                    },
                    {
                        title: 'Необходимые',
                        description: 'Необходимые куки для работы сайта.',
                        toggle: {
                            value: 'necessary',
                            enabled: true,
                            readonly: true
                        },
                        cookie_table: [
                            {
                                col1: 'cc_cookie',
                                col2: 'Этот сайт',
                                col3: '182 дня',
                                col4: 'Настроки предпочтений использования куки'
                            }
                        ]
                    },
                    {
                        title: 'Метрики',
                        description: 'Метрические куки используются нами для того, чтобы получить базовое представление о нашей пользовательской базе. Блокировщики рекламы будут блокировать метрики, даже если будет дано согласие. Мы не обходим блокировщики. ',
                        toggle: {
                            value: 'metrics',
                            enabled: false,
                            readonly: false
                        },
                        cookie_table: [
                            {
                                col1: '_ym_d',
                                col2: 'yandex.ru',
                                col3: '1 год',
                                col4: 'Хранит дату первого визита посетителя на сайт'
                            },
                            {
                                col1: '_ym_uid',
                                col2: 'yandex.ru',
                                col3: '1 год',
                                col4: 'Позволяет различать посетителей',
                            },
                            {
                                col1: 'i',
                                col2: 'yandex.ru',
                                col3: '1 год',
                                col4: 'Позволяет различать посетителей',
                            },
                            {
                                col1: 'yandexuid',
                                col2: 'yandex.ru',
                                col3: '1 год',
                                col4: 'Позволяет различать посетителей',
                            },
                            {
                                col1: 'yuidss',
                                col2: 'yandex.ru',
                                col3: '1 год',
                                col4: 'Позволяет различать посетителей',
                            },
                            {
                                col1: 'ymex',
                                col2: 'yandex.ru',
                                col3: '1 год',
                                col4: 'Хранит вспомогательную информацию для работы Метрики: время создания идентификаторов и их альтернативные значения',
                            },
                            {
                                col1: '_ym_isad',
                                col2: 'yandex.ru',
                                col3: '2 дня',
                                col4: 'Используется для определения наличия у посетителя блокировщиков рекламы',
                            },
                            {
                                col1: 'yabs-sid',
                                col2: 'yandex.ru',
                                col3: 'Сессия',
                                col4: 'Идентификатор визита',
                            }
                        ]
                    },
                    {
                        title: 'Больше информации',
                        description: 'Если у вас возникнут вопросы о куки, создайте <a class="cc-link" href="https://github.com/BUTR/BUTR.Site.NexusMods/issues">issue</a> в нашем репозиторий! ',
                    }
                ]
            }
        }
    }
});